using System.Collections.Generic;
using Lutra.Rendering.Shaders;
using Veldrid;

namespace Lutra.Rendering.Pipelines;

// TODO: Replace this with just reusing the SpriteRenderPipeline
// That way we can simplify the Surface render logic, enable multiple Surfaces, and have custom shaders on Surfaces.
public class SurfaceRenderPipeline : IDisposable
{
    private readonly DeviceBuffer VertexBuffer;
    private readonly ResourceLayout SurfaceLayout;
    private readonly Pipeline Pipeline;

    private readonly Dictionary<int, ResourceSet> SurfaceSets = new();

    public SurfaceRenderPipeline()
    {
        VertexBuffer = VeldridResources.CreateVertexBuffer(4u * VertexPositionTexture.SizeInBytes);

        SurfaceLayout = VeldridResources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Surface", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var shaderSet = new ShaderSetDescription(
            new[] { VertexPositionTexture.LayoutDescription },
            VeldridResources.CreateShaders(BuiltinShader.SurfaceVertexBytes, BuiltinShader.SurfaceFragmentBytes)
        );

        var description = new GraphicsPipelineDescription();
        description.BlendState = BlendStateDescription.SINGLE_DISABLED;
        description.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        description.ResourceLayouts = new[] { SurfaceLayout };
        description.ResourceBindingModel = ResourceBindingModel.Improved;
        description.ShaderSet = shaderSet;
        description.Outputs = VeldridResources.GraphicsDevice.SwapchainFramebuffer.OutputDescription;
        description.DepthStencilState = DepthStencilStateDescription.DISABLED;
        description.RasterizerState = RasterizerStateDescription.DEFAULT;

        Pipeline = VeldridResources.Factory.CreateGraphicsPipeline(description);
    }

    private ResourceSet GetSurfaceSet(TextureView textureView)
    {
        int key = HashCode.Combine(textureView);
        ResourceSet resourceSet;

        if (!SurfaceSets.TryGetValue(key, out resourceSet))
        {
            resourceSet = VeldridResources.Factory.CreateResourceSet(
                new ResourceSetDescription(SurfaceLayout, textureView, VeldridResources.GraphicsDevice.PointSampler)
            );
            SurfaceSets[key] = resourceSet;
        }

        return resourceSet;
    }

    public void DrawSurface(TextureView textureView, VertexPositionTexture[] vertices)
    {
        Draw.CommandList.UpdateBuffer(VertexBuffer, 0u, vertices);
        Draw.CommandList.SetVertexBuffer(0u, VertexBuffer);
        Draw.CommandList.SetPipeline(Pipeline);
        Draw.CommandList.SetGraphicsResourceSet(0u, GetSurfaceSet(textureView));
        Draw.CommandList.Draw(4u);
    }

    public void Dispose()
    {
        VertexBuffer.Dispose();
        SurfaceLayout.Dispose();
        Pipeline.Dispose();
    }
}
