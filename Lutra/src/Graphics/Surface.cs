using Lutra.Rendering;
using Veldrid;

namespace Lutra.Graphics;

public class Surface
{
    public readonly float Width;
    public readonly float Height;
    public readonly LutraTexture Texture;
    public readonly Framebuffer Framebuffer;

    #region Constructors

    public Surface(uint width, uint height)
    {
        Width = width;
        Height = height;
        Texture = new LutraTexture(VeldridResources.CreateSurfaceTexture(width, height));
        Framebuffer = VeldridResources.CreateFramebuffer(Texture);
    }

    #endregion
}
