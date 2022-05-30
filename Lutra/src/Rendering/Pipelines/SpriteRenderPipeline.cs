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

    private const string NO_SHADER = "___NO_SHADER___";

    private readonly GraphicsPipelineDescription PipelineDescTemplate;

    private readonly PipelineCommon PipelineCommon;

    private readonly DeviceBuffer WorldBuffer;
    private readonly DeviceBuffer VertexBuffer;
    private readonly DeviceBuffer IndexBuffer;

    private readonly ResourceLayout PerSpriteResourceLayout;

    private readonly Dictionary<int, ResourceSet> PerTextureResourceSets = new();
    private readonly Dictionary<int, ResourceSet> PerShaderResourceSets = new();
    private readonly Dictionary<string, ResourceLayout> ShaderLayouts = new();

    private readonly Dictionary<int, Pipeline> Pipelines = new();

    internal SpriteRenderPipeline(PipelineCommon common)
    {
        PipelineCommon = common;

        PipelineDescTemplate = new GraphicsPipelineDescription();
        PipelineDescTemplate.PrimitiveTopology = PrimitiveTopology.TriangleList;
        PipelineDescTemplate.DepthStencilState = DepthStencilStateDescription.Disabled;
        PipelineDescTemplate.Outputs = VeldridResources.GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        PipelineDescTemplate.RasterizerState = RasterizerStateDescription.Default;

        WorldBuffer = VeldridResources.CreateMatrixUniformBuffer();
        VertexBuffer = VeldridResources.CreateVertexBuffer(MAX_VERTICES * VertexPositionColorTexture.SizeInBytes);
        IndexBuffer = CreateQuadIndexBuffer();

        PerSpriteResourceLayout = VeldridResources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var shaderSet = new ShaderSetDescription(
            new[] { VertexPositionColorTexture.LayoutDescription },
            VeldridResources.CreateShaders(BuiltinShader.SpriteVertexBytes, BuiltinShader.SpriteFragmentBytes)
        );

        var alphaBlendDesc = PipelineDescTemplate;
        alphaBlendDesc.BlendState = BlendStateDescription.SingleAlphaBlend;
        alphaBlendDesc.ResourceLayouts = new[] { PipelineCommon.PerFrameResourceLayout, PerSpriteResourceLayout };
        alphaBlendDesc.ShaderSet = shaderSet;

        var addBlendDesc = alphaBlendDesc;
        addBlendDesc.BlendState = BlendStateDescription.SingleAdditiveBlend;

        Pipelines.Add(
            HashCode.Combine(NO_SHADER, BlendMode.Alpha),
            VeldridResources.Factory.CreateGraphicsPipeline(alphaBlendDesc));
        Pipelines.Add(
            HashCode.Combine(NO_SHADER, BlendMode.Add),
            VeldridResources.Factory.CreateGraphicsPipeline(addBlendDesc));
    }

    internal void CreateShaderPipeline(string shaderName, byte[] fragShaderBytes, ResourceLayoutDescription layoutDesc)
    {
        if (ShaderLayouts.ContainsKey(shaderName)) return;

        var shaderSet = new ShaderSetDescription(
            new[] { VertexPositionColorTexture.LayoutDescription },
            VeldridResources.CreateShaders(BuiltinShader.SpriteVertexBytes, fragShaderBytes)
        );

        var shaderResourceLayout = VeldridResources.Factory.CreateResourceLayout(layoutDesc);

        var alphaBlendDesc = PipelineDescTemplate;
        alphaBlendDesc.BlendState = BlendStateDescription.SingleAlphaBlend;
        alphaBlendDesc.ResourceLayouts = new[] { PipelineCommon.PerFrameResourceLayout, PerSpriteResourceLayout, shaderResourceLayout };
        alphaBlendDesc.ShaderSet = shaderSet;

        var addBlendDesc = alphaBlendDesc;
        addBlendDesc.BlendState = BlendStateDescription.SingleAdditiveBlend;

        Pipelines.Add(
            HashCode.Combine(shaderName, BlendMode.Alpha),
            VeldridResources.Factory.CreateGraphicsPipeline(alphaBlendDesc));
        Pipelines.Add(
            HashCode.Combine(shaderName, BlendMode.Add),
            VeldridResources.Factory.CreateGraphicsPipeline(addBlendDesc));

        ShaderLayouts.Add(shaderName, shaderResourceLayout);
    }

    public void UpdateVertexBuffer(ref VertexPositionColorTexture[] data, uint vertIndex, uint vertAmount)
    {
        uint sizeInBytes = vertAmount * VertexPositionColorTexture.SizeInBytes;
        Draw.CommandList.UpdateBuffer(VertexBuffer, 0u, ref data[vertIndex], sizeInBytes);
    }

    public void DrawSprites(SpriteParams quadInfo, uint indexOffset, uint numQuads, BlendMode blendMode, bool smooth, ShaderData shaderData = null)
    {
        var shaderName = shaderData != null ? shaderData.ShaderName : NO_SHADER;
        var key = HashCode.Combine(shaderName, blendMode);
        var pipeline = Pipelines[key];

        Draw.CommandList.SetPipeline(pipeline);

        Draw.CommandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);
        Draw.CommandList.SetVertexBuffer(0u, VertexBuffer);
        Draw.CommandList.UpdateBuffer(WorldBuffer, 0u, quadInfo.WorldMatrix);

        Draw.CommandList.SetGraphicsResourceSet(0u, PipelineCommon.PerFrameResourceSet);
        Draw.CommandList.SetGraphicsResourceSet(1u, GetPerTextureResourceSet(quadInfo, smooth));

        if (shaderData != null)
        {
            Draw.CommandList.SetGraphicsResourceSet(2u, GetShaderResourceSet(shaderData));
        }

        Draw.CommandList.DrawIndexed(numQuads * 6u, 1u, indexOffset * 6u, 0, 0u);
    }

    private ResourceSet GetPerTextureResourceSet(SpriteParams quadInfo, bool smooth)
    {
        int key = HashCode.Combine(quadInfo.Texture.TextureView, smooth);
        ResourceSet resourceSet;

        if (!PerTextureResourceSets.TryGetValue(key, out resourceSet))
        {
            var sampler = smooth ? VeldridResources.GraphicsDevice.LinearSampler : VeldridResources.GraphicsDevice.PointSampler;
            resourceSet = VeldridResources.Factory.CreateResourceSet(new ResourceSetDescription(
                PerSpriteResourceLayout, WorldBuffer, quadInfo.Texture.TextureView, sampler)
            );
            PerTextureResourceSets[key] = resourceSet;
        }

        return resourceSet;
    }

    private ResourceSet GetShaderResourceSet(ShaderData shaderData)
    {
        int key = HashCode.Combine(shaderData);
        ResourceSet resourceSet;

        if (!PerShaderResourceSets.TryGetValue(key, out resourceSet))
        {
            var desc = new ResourceSetDescription(ShaderLayouts[shaderData.ShaderName], shaderData.Resources);
            resourceSet = VeldridResources.Factory.CreateResourceSet(ref desc);
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
