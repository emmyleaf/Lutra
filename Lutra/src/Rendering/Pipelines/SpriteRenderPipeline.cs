using System.Collections.Generic;
using Lutra.Rendering.Shaders;
using Veldrid;

namespace Lutra.Rendering.Pipelines;

// TODO: Eventually this will be replaced by InstancedRenderPipeline for all basic sprites.
// Then this will be renamed and only serve the purpose of creating custom shader pipelines.
public class SpriteRenderPipeline
{
    private const ushort MAX_QUADS = 2048 * 4;
    private const ushort MAX_VERTICES = MAX_QUADS * 4;
    private const ushort MAX_INDICES = MAX_QUADS * 6;

    private readonly PipelineCommon PipelineCommon;

    private readonly DeviceBuffer WorldBuffer;
    private readonly DeviceBuffer VertexBuffer;
    private readonly DeviceBuffer IndexBuffer;

    private readonly ResourceLayout PerSpriteResourceLayout;

    private readonly Dictionary<int, ResourceSet> PerSpriteResourceSets = new();
    private readonly Dictionary<int, ResourceSet> PerShaderResourceSets = new();

    private readonly Pipeline MainPipeline;
    private readonly Dictionary<string, (Pipeline Pipeline, ResourceLayout Layout)> ShaderPipelines = new();

    internal SpriteRenderPipeline(PipelineCommon common)
    {
        PipelineCommon = common;

        WorldBuffer = VeldridResources.CreateMatrixUniformBuffer();
        VertexBuffer = VeldridResources.CreateVertexBuffer(MAX_VERTICES * VertexPositionColorTexture.SizeInBytes);
        IndexBuffer = CreateQuadIndexBuffer();

        PerSpriteResourceLayout = VeldridResources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Sprite", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var shaderSet = new ShaderSetDescription(
            new[] { VertexPositionColorTexture.LayoutDescription },
            VeldridResources.CreateShaders(BuiltinShader.SpriteVertexBytes, BuiltinShader.SpriteFragmentBytes)
        );

        var description = new GraphicsPipelineDescription();
        description.BlendState = BlendStateDescription.SingleAlphaBlend;
        description.PrimitiveTopology = PrimitiveTopology.TriangleList;
        description.ResourceLayouts = new[] { PipelineCommon.PerFrameResourceLayout, PerSpriteResourceLayout };
        description.ResourceBindingModel = ResourceBindingModel.Improved;
        description.ShaderSet = shaderSet;
        description.Outputs = VeldridResources.GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        description.DepthStencilState = DepthStencilStateDescription.Disabled;
        description.RasterizerState = RasterizerStateDescription.Default;

        MainPipeline = VeldridResources.Factory.CreateGraphicsPipeline(description);
    }

    internal void CreateShaderPipeline(string shaderName, byte[] fragShaderBytes, ResourceLayoutDescription layoutDesc)
    {
        if (ShaderPipelines.ContainsKey(shaderName)) return;

        var shaderSet = new ShaderSetDescription(
            new[] { VertexPositionColorTexture.LayoutDescription },
            VeldridResources.CreateShaders(BuiltinShader.SpriteVertexBytes, fragShaderBytes)
        );

        var shaderResourceLayout = VeldridResources.Factory.CreateResourceLayout(layoutDesc);

        var description = new GraphicsPipelineDescription();
        description.BlendState = BlendStateDescription.SingleAlphaBlend;
        description.PrimitiveTopology = PrimitiveTopology.TriangleList;
        description.ResourceLayouts = new[] { PipelineCommon.PerFrameResourceLayout, PerSpriteResourceLayout, shaderResourceLayout };
        description.ResourceBindingModel = ResourceBindingModel.Improved;
        description.ShaderSet = shaderSet;
        description.Outputs = VeldridResources.GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        description.DepthStencilState = DepthStencilStateDescription.Disabled;
        description.RasterizerState = RasterizerStateDescription.Default;

        var shaderPipeline = VeldridResources.Factory.CreateGraphicsPipeline(description);

        ShaderPipelines.Add(shaderName, (shaderPipeline, shaderResourceLayout));
    }

    public void UpdateVertexBuffer(ref VertexPositionColorTexture[] data, uint vertIndex, uint vertAmount)
    {
        uint sizeInBytes = vertAmount * VertexPositionColorTexture.SizeInBytes;
        Draw.CommandList.UpdateBuffer(VertexBuffer, 0u, ref data[vertIndex], sizeInBytes);
    }

    public void DrawSprites(SpriteParams quadInfo, uint indexOffset, uint numQuads)
    {
        Draw.CommandList.SetPipeline(MainPipeline);

        Draw.CommandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);
        Draw.CommandList.SetVertexBuffer(0u, VertexBuffer);
        Draw.CommandList.UpdateBuffer(WorldBuffer, 0u, quadInfo.WorldMatrix);

        Draw.CommandList.SetGraphicsResourceSet(0u, PipelineCommon.PerFrameResourceSet);
        Draw.CommandList.SetGraphicsResourceSet(1u, GetPerSpriteResourceSet(quadInfo));

        Draw.CommandList.DrawIndexed(numQuads * 6u, 1u, indexOffset * 6u, 0, 0u);
    }

    public void DrawShaderSprites(SpriteParams quadInfo, uint indexOffset, uint numQuads, SpriteShader shaderData)
    {
        Draw.CommandList.SetPipeline(ShaderPipelines[shaderData.ShaderName].Pipeline);

        Draw.CommandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);
        Draw.CommandList.SetVertexBuffer(0u, VertexBuffer);
        Draw.CommandList.UpdateBuffer(WorldBuffer, 0u, quadInfo.WorldMatrix);

        Draw.CommandList.SetGraphicsResourceSet(0u, PipelineCommon.PerFrameResourceSet);
        Draw.CommandList.SetGraphicsResourceSet(1u, GetPerSpriteResourceSet(quadInfo));
        Draw.CommandList.SetGraphicsResourceSet(2u, GetShaderResourceSet(shaderData));

        Draw.CommandList.DrawIndexed(numQuads * 6u, 1u, indexOffset * 6u, 0, 0u);
    }

    private ResourceSet GetPerSpriteResourceSet(SpriteParams quadInfo)
    {
        int key = HashCode.Combine(quadInfo.Texture);
        ResourceSet resourceSet;

        if (!PerSpriteResourceSets.TryGetValue(key, out resourceSet))
        {
            resourceSet = VeldridResources.Factory.CreateResourceSet(new ResourceSetDescription(
                PerSpriteResourceLayout, WorldBuffer, quadInfo.Texture.TextureView, VeldridResources.GraphicsDevice.PointSampler)
            );
            PerSpriteResourceSets[key] = resourceSet;
        }

        return resourceSet;
    }

    private ResourceSet GetShaderResourceSet(SpriteShader shaderData)
    {
        int key = HashCode.Combine(shaderData);
        ResourceSet resourceSet;

        if (!PerShaderResourceSets.TryGetValue(key, out resourceSet))
        {
            var layout = ShaderPipelines[shaderData.ShaderName].Layout;
            resourceSet = VeldridResources.Factory.CreateResourceSet(new ResourceSetDescription(
                layout, shaderData.Resources
            ));
            PerShaderResourceSets[key] = resourceSet;
        }

        return resourceSet;
    }

    private static DeviceBuffer CreateQuadIndexBuffer()
    {
        var buffer = VeldridResources.CreateIndexBuffer(MAX_INDICES * sizeof(ushort));
        var indices = new ushort[MAX_INDICES];
        for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
        {
            indices[i] = (ushort)j;
            indices[i + 1] = (ushort)(j + 1);
            indices[i + 2] = (ushort)(j + 3);

            indices[i + 3] = (ushort)(j);
            indices[i + 4] = (ushort)(j + 3);
            indices[i + 5] = (ushort)(j + 2);
        }

        VeldridResources.GraphicsDevice.UpdateBuffer(buffer, 0u, indices);
        return buffer;
    }
}
