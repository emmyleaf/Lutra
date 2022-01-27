using System.Numerics;
using Lutra.Graphics;
using Lutra.Rendering.Pipelines;
using Lutra.Utility;
using Veldrid;

namespace Lutra.Rendering;

public static class Draw
{
    #region Static Fields

    internal static CommandList CommandList;
    internal static PipelineCommon PipelineCommon;
    internal static InstancedRenderPipeline InstancedRenderPipeline;
    internal static SpriteRenderPipeline SpriteRenderPipeline;
    internal static SurfaceRenderPipeline SurfaceRenderPipeline;

    #endregion

    #region Lifecycle Methods

    public static void Initialize()
    {
        CommandList = VeldridResources.Factory.CreateCommandList();
        PipelineCommon = new();
        InstancedRenderPipeline = new(PipelineCommon);
        SpriteRenderPipeline = new(PipelineCommon);
        SurfaceRenderPipeline = new();
    }

    public static void Begin(Color clearColor, Surface surface)
    {
        CommandList.Begin();
        CommandList.SetFramebuffer(surface.Framebuffer);
        CommandList.ClearColorTarget(0u, clearColor);
        CommandList.UpdateBuffer(PipelineCommon.ProjectionBuffer, 0u, Game.Instance.CameraManager.Projection);
        CommandList.UpdateBuffer(PipelineCommon.ViewBuffer, 0u, Game.Instance.CameraManager.View);
    }

    public static void End()
    {
        CommandList.End();

        if (VeldridResources.Sdl2Window.Exists)
        {
            VeldridResources.GraphicsDevice.SubmitCommands(CommandList);
            VeldridResources.DEBUG_ImGuiRender();
            VeldridResources.GraphicsDevice.SwapBuffers();
        }
    }

    #endregion

    #region Draw Methods

    public static void Instanced(
        LutraTexture texture,
        int layer,
        Matrix4x4 graphicMatrix,
        RectFloat destRect,
        RectInt sourceRect,
        Color color
    )
    {
        var texWidth = (float)texture.Width;
        var texHeight = (float)texture.Height;

        var source = Matrix4x4.CreateScale(sourceRect.Width / texWidth, sourceRect.Height / texHeight, 1) *
            Matrix4x4.CreateTranslation(sourceRect.X / texWidth, sourceRect.Y / texHeight, 0);

        var world = Matrix4x4.CreateScale(destRect.Width, destRect.Height, 1) *
            graphicMatrix *
            Matrix4x4.CreateTranslation(destRect.X, destRect.Y, 0);

        InstancedRenderPipeline.Add(texture, layer, color.ToVector4(), source, world);
    }

    public static void SpriteDrawable(SpriteDrawable spriteDrawable)
    {
        InstancedRenderPipeline.Flush();

        var vertData = spriteDrawable.Vertices.ToArray();
        var vertCount = (uint)spriteDrawable.Vertices.Count;
        var quadCount = vertCount / 4u;

        SpriteRenderPipeline.UpdateVertexBuffer(ref vertData, 0, vertCount);
        SpriteRenderPipeline.DrawSprites(spriteDrawable.Params, 0, quadCount);
    }

    public static void SpriteDrawableShaders(SpriteDrawable spriteDrawable, SpriteShader spriteShader)
    {
        InstancedRenderPipeline.Flush();

        var vertData = spriteDrawable.Vertices.ToArray();
        var vertCount = (uint)spriteDrawable.Vertices.Count;
        var quadCount = vertCount / 4u;

        SpriteRenderPipeline.UpdateVertexBuffer(ref vertData, 0, vertCount);
        SpriteRenderPipeline.DrawShaderSprites(spriteDrawable.Params, 0, quadCount, spriteShader);
    }

    public static void SurfaceToWindow(Surface surface, Window window, Color letterBoxColor)
    {
        InstancedRenderPipeline.Flush();

        CommandList.SetFramebuffer(VeldridResources.GraphicsDevice.SwapchainFramebuffer);
        CommandList.ClearColorTarget(0u, letterBoxColor);

        var bounds = window.SurfaceBounds;

        VertexPositionTexture[] vertices;

        // weird hacks needed here, but this seems to work for all Windows-testable backends for now

        var flipSurfaceUVs = VeldridResources.GraphicsDevice.BackendType == Veldrid.GraphicsBackend.OpenGL
            || VeldridResources.GraphicsDevice.BackendType == Veldrid.GraphicsBackend.OpenGLES;

        if (VeldridResources.GraphicsDevice.IsClipSpaceYInverted)
        {
            vertices = new[] {
                new VertexPositionTexture(bounds.TopLeft, new Vector2(0, 0)),
                new VertexPositionTexture(bounds.TopRight, new Vector2(1, 0)),
                new VertexPositionTexture(bounds.BottomLeft, new Vector2(0, 1)),
                new VertexPositionTexture(bounds.BottomRight, new Vector2(1, 1)),
            };
        }
        else if (flipSurfaceUVs)
        {
            vertices = new[] {
                new VertexPositionTexture(bounds.BottomLeft, new Vector2(0, 1)),
                new VertexPositionTexture(bounds.BottomRight, new Vector2(1, 1)),
                new VertexPositionTexture(bounds.TopLeft, new Vector2(0, 0)),
                new VertexPositionTexture(bounds.TopRight, new Vector2(1, 0)),
            };
        }
        else
        {
            vertices = new[] {
                new VertexPositionTexture(bounds.BottomLeft, new Vector2(0, 0)),
                new VertexPositionTexture(bounds.BottomRight, new Vector2(1, 0)),
                new VertexPositionTexture(bounds.TopLeft, new Vector2(0, 1)),
                new VertexPositionTexture(bounds.TopRight, new Vector2(1, 1)),
            };
        }

        SurfaceRenderPipeline.DrawSurface(surface.Texture, vertices);
    }

    #endregion
}
