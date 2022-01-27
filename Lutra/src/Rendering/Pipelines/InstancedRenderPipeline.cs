using System.Collections.Generic;
using System.Numerics;
using Lutra.Rendering.Shaders;
using Lutra.Utility.Collections;
using Veldrid;

namespace Lutra.Rendering.Pipelines;

public class InstancedRenderPipeline
{
    // TODO: resize instance buffers when too large rather than this arbitrary maximum
    public const uint MAX_INSTANCES = 256u;
    public const uint INITIAL_INSTANCE_BUFFER_SIZE = MAX_INSTANCES * Instance.SizeInBytes;

    private readonly PipelineCommon PipelineCommon;

    private readonly DeviceBuffer VertexBuffer;
    private readonly ResourceLayout PerBatchResourceLayout;
    private readonly Pipeline Pipeline;

    private readonly Dictionary<int, BatchResources> ResourceDict = new();
    private readonly Dictionary<int, InstanceBatch> BatchesDict = new();
    private readonly List<InstanceBatch> BatchesList = new();

    internal InstancedRenderPipeline(PipelineCommon common)
    {
        PipelineCommon = common;

        VertexBuffer = CreateSingleQuadVertexBuffer();

        PerBatchResourceLayout = VeldridResources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("InstancesBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var shaderSet = new ShaderSetDescription(
            new[] { VertexPosition.LayoutDescription },
            VeldridResources.CreateShaders(BuiltinShader.InstancedVertexBytes, BuiltinShader.InstancedFragmentBytes)
        );

        var description = new GraphicsPipelineDescription();
        description.BlendState = BlendStateDescription.SingleAlphaBlend;
        description.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        description.ResourceLayouts = new[] { PipelineCommon.PerFrameResourceLayout, PerBatchResourceLayout };
        description.ResourceBindingModel = ResourceBindingModel.Improved;
        description.ShaderSet = shaderSet;
        description.Outputs = VeldridResources.GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        description.DepthStencilState = DepthStencilStateDescription.Disabled;
        description.RasterizerState = RasterizerStateDescription.Default;

        Pipeline = VeldridResources.Factory.CreateGraphicsPipeline(description);
    }

    /// <summary>
    /// Add an instance to be drawn at the next Flush.
    /// </summary>
    public void Add(LutraTexture texture, int layer, Vector4 color, Matrix4x4 source, Matrix4x4 world)
    {
        var hashCode = HashCode.Combine(texture, layer);
        InstanceBatch batch;

        if (!BatchesDict.TryGetValue(hashCode, out batch))
        {
            batch = new InstanceBatch()
            {
                Resources = GetBatchResources(hashCode, texture)
            };

            BatchesDict.Add(hashCode, batch);
            BatchesList.Add(batch);
        }

        batch.Instances.Add(new Instance { Color = color, Source = source, World = world });
    }

    /// <summary>
    /// Flush current contents to the main CommandList.
    /// </summary>
    public void Flush()
    {
        if (BatchesList.IsEmpty()) return;

        Draw.CommandList.SetPipeline(Pipeline);
        Draw.CommandList.SetVertexBuffer(0u, VertexBuffer);
        Draw.CommandList.SetGraphicsResourceSet(0, PipelineCommon.PerFrameResourceSet);

        foreach (var batch in BatchesList)
        {
            Draw.CommandList.UpdateBuffer(batch.Resources.InstancesBuffer, 0, batch.Instances.Items);
            Draw.CommandList.SetGraphicsResourceSet(1, batch.Resources.ResourceSet);
            Draw.CommandList.Draw(4, (uint)batch.Instances.Count, 0, 0);
        }

        BatchesDict.Clear();
        BatchesList.Clear();
    }

    private BatchResources GetBatchResources(int hashCode, LutraTexture texture)
    {
        BatchResources batchResources;

        if (!ResourceDict.TryGetValue(hashCode, out batchResources))
        {
            var buffer = VeldridResources.CreateStructuredBuffer(INITIAL_INSTANCE_BUFFER_SIZE, Instance.SizeInBytes);
            var resourceSet = VeldridResources.Factory.CreateResourceSet(new ResourceSetDescription(
                PerBatchResourceLayout, buffer, texture.TextureView, VeldridResources.GraphicsDevice.PointSampler
            ));

            batchResources = new BatchResources { InstancesBuffer = buffer, ResourceSet = resourceSet };

            ResourceDict.Add(hashCode, batchResources);
        }

        return batchResources;
    }

    private static DeviceBuffer CreateSingleQuadVertexBuffer()
    {
        var buffer = VeldridResources.CreateVertexBuffer(4 * VertexPosition.SizeInBytes);

        var vertices = new[]
        {
            new VertexPosition(new Vector2(0,  0)),
            new VertexPosition(new Vector2(1,  0)),
            new VertexPosition(new Vector2(0,  1)),
            new VertexPosition(new Vector2(1,  1))
        };

        VeldridResources.GraphicsDevice.UpdateBuffer(buffer, 0, vertices);

        return buffer;
    }

    private struct Instance
    {
        public const uint SizeInBytes = 16u + 64u + 64u;
        public Vector4 Color;
        public Matrix4x4 Source;
        public Matrix4x4 World;
    }

    private class BatchResources
    {
        public DeviceBuffer InstancesBuffer;
        public ResourceSet ResourceSet;
    }

    private class InstanceBatch
    {
        public LutraList<Instance> Instances = new(16);
        public BatchResources Resources;
    }
}
