using System.Collections.Generic;
using System.Numerics;
using Lutra.Rendering.Shaders;
using Lutra.Utility.Collections;
using Veldrid;

namespace Lutra.Rendering.Pipelines;

public class InstancedRenderPipeline
{
    public const uint MAX_INSTANCES = 2048u;
    public const uint INSTANCE_BUFFER_SIZE = MAX_INSTANCES * Instance.SizeInBytes;

    private readonly PipelineCommon PipelineCommon;

    private readonly DeviceBuffer VertexBuffer;
    private readonly DeviceBuffer InstanceBuffer;

    private readonly ResourceLayout PerBatchResourceLayout;
    private readonly Pipeline Pipeline;

    private readonly Dictionary<int, ResourceSet> ResourceDict = new();
    private readonly Dictionary<int, InstanceBatch> BatchesDict = new();
    private readonly List<InstanceBatch> BatchesList = new();

    internal InstancedRenderPipeline(PipelineCommon common)
    {
        PipelineCommon = common;

        VertexBuffer = CreateSingleQuadVertexBuffer();
        InstanceBuffer = VeldridResources.CreateStructuredBuffer(INSTANCE_BUFFER_SIZE, Instance.SizeInBytes);

        PerBatchResourceLayout = VeldridResources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("InstancesBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var shaderSet = new ShaderSetDescription(
            new[] { VertexPosition.LayoutDescription },
            VeldridResources.CreateShaders(BuiltinShader.InstancedVertexBytes, BuiltinShader.SpriteFragmentBytes)
        );

        var description = new GraphicsPipelineDescription();
        description.BlendState = BlendStateDescription.SINGLE_ALPHA_BLEND;
        description.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        description.ResourceLayouts = new[] { PipelineCommon.PerFrameResourceLayout, PerBatchResourceLayout };
        description.ResourceBindingModel = ResourceBindingModel.Improved;
        description.ShaderSet = shaderSet;
        description.Outputs = VeldridResources.GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        description.DepthStencilState = DepthStencilStateDescription.DISABLED;
        description.RasterizerState = RasterizerStateDescription.DEFAULT;

        Pipeline = VeldridResources.Factory.CreateGraphicsPipeline(description);
    }

    /// <summary>
    /// Add an instance to be drawn at the next Flush.
    /// A flush can be triggered if any resource set already contains the maximum number of instances.
    /// </summary>
    public void Add(LutraTexture texture, int layer, Vector4 color, Matrix4x4 source, Matrix4x4 world, CommandList commandList)
    {
        var hashCode = HashCode.Combine(texture.TextureView, layer);
        InstanceBatch batch;

        if (!BatchesDict.TryGetValue(hashCode, out batch))
        {
            batch = new InstanceBatch() { ResourceSet = GetBatchResourceSet(hashCode, texture.TextureView) };

            BatchesDict.Add(hashCode, batch);
            BatchesList.Add(batch);
        }

        if (batch.Instances.Count == MAX_INSTANCES)
        {
            Flush(commandList);
        }

        batch.Instances.Add(new Instance { Color = color, Source = source, World = world });
    }

    /// <summary>
    /// Flush current contents to the given CommandList.
    /// </summary>
    public void Flush(CommandList commandList)
    {
        if (BatchesList.IsEmpty()) return;

        commandList.SetPipeline(Pipeline);
        commandList.SetVertexBuffer(0u, VertexBuffer);
        commandList.SetGraphicsResourceSet(0, PipelineCommon.PerFrameResourceSet);

        foreach (var batch in BatchesList)
        {
            commandList.UpdateBuffer(InstanceBuffer, 0, batch.Instances.GetReadOnlySpan());
            commandList.SetGraphicsResourceSet(1, batch.ResourceSet);
            commandList.Draw(4, (uint)batch.Instances.Count, 0, 0);
        }

        BatchesDict.Clear();
        BatchesList.Clear();
    }

    private ResourceSet GetBatchResourceSet(int hashCode, TextureView textureView)
    {
        ResourceSet batchResourceSet;

        if (!ResourceDict.TryGetValue(hashCode, out batchResourceSet))
        {
            batchResourceSet = VeldridResources.Factory.CreateResourceSet(new ResourceSetDescription(
                PerBatchResourceLayout, InstanceBuffer, textureView, VeldridResources.GraphicsDevice.PointSampler
            ));

            ResourceDict.Add(hashCode, batchResourceSet);
        }

        return batchResourceSet;
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

    private class InstanceBatch
    {
        public LutraList<Instance> Instances = new(16);
        public ResourceSet ResourceSet;
    }
}
