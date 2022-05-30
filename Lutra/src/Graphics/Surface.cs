using Lutra.Cameras;
using Lutra.Rendering;
using Veldrid;

namespace Lutra.Graphics;

/// <summary>
/// Graphic that represents a render target.
/// </summary>
public class Surface : Image
{
    internal readonly Framebuffer Framebuffer;

    /// <summary>
    /// Determines if the Surface will automatically clear at the start of the next render cycle.
    /// </summary>
    public bool AutoClear = true;

    /// <summary>
    /// Determines if the Surface will use the camera provided by CameraManager.
    /// Currently named as 'SceneCamera' due to backwards compatibility with Otter.
    /// If false, draws to this surface use a centered orthographic projection the size of the surface.
    /// </summary>
    public bool UseSceneCamera = true;

    /// <summary>
    /// The color that the surface will clear with at the start of each render.
    /// </summary>
    public Color ClearColor;

    #region Constructors

    /// <summary>
    /// Constructs a Surface with a specified size and optional clear color.
    /// </summary>
    public Surface(int width, int height, Color? clearColor = null)
    {
        if (width < 0) throw new ArgumentException("Width must be greater than 0.");
        if (height < 0) throw new ArgumentException("Height must be greater than 0.");

        ClearColor = clearColor ?? Color.None;
        Width = width;
        Height = height;
        SetTexture(new LutraTexture(VeldridResources.CreateSurfaceTexture((uint)width, (uint)height)));
        Framebuffer = VeldridResources.CreateFramebuffer((Texture)Texture);
    }

    #endregion

    /// <summary>
    /// Draws a graphic to this surface.
    /// </summary>
    /// <param name="graphic">The Graphic to draw.</param>
    public void Draw(Graphic graphic)
    {
        var temp = Rendering.Draw.Target;
        Rendering.Draw.Target = this;

        if (!UseSceneCamera)
        {
            Game.Instance.CameraManager.PushCamera(new Camera(HalfWidth, HalfHeight, Width, Height));
        }

        graphic.Render();
        graphic.NeedsUpdate = false; // This would only get set back to false in InternalRender(), which we bypass for manual drawing.
        Rendering.Draw.Target = temp;

        if (!UseSceneCamera)
        {
            Game.Instance.CameraManager.PopCamera();
        }
    }

    protected internal override void Render()
    {
        base.Render();

        if (AutoClear)
        {
            var temp = Rendering.Draw.Target;
            Rendering.Draw.Target = this;
            Rendering.Draw.ClearTarget();
            Rendering.Draw.Target = temp;
        }
    }
}
