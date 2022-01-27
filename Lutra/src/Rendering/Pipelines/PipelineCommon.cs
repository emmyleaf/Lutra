using Veldrid;

namespace Lutra.Rendering.Pipelines;

internal class PipelineCommon
{
    internal readonly DeviceBuffer ProjectionBuffer;
    internal readonly DeviceBuffer ViewBuffer;

    internal readonly ResourceLayout PerFrameResourceLayout;

    internal readonly ResourceSet PerFrameResourceSet;

    internal PipelineCommon()
    {
        ProjectionBuffer = VeldridResources.CreateMatrixUniformBuffer();
        ViewBuffer = VeldridResources.CreateMatrixUniformBuffer();

        PerFrameResourceLayout = VeldridResources.Factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        PerFrameResourceSet = VeldridResources.Factory.CreateResourceSet(
            new ResourceSetDescription(PerFrameResourceLayout, ProjectionBuffer, ViewBuffer)
        );
    }
}
