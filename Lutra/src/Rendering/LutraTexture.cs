#nullable enable

using System.IO;
using Veldrid;
using Veldrid.ImageSharp;

namespace Lutra.Rendering;

public class LutraTexture
{
    #region Protected Properties

    protected TextureView? _textureView;

    #endregion

    #region Public Properties

    public Texture Texture { get; protected set; }

    public uint Width => Texture.Width;
    public uint Height => Texture.Height;

    public TextureView TextureView
    {
        get
        {
            if (_textureView == null)
            {
                _textureView = VeldridResources.Factory.CreateTextureView(Texture);
            }

            return _textureView;
        }
    }

    public bool IsRenderTarget => (Texture.Usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget;

    public static explicit operator Texture(LutraTexture texture) => texture.Texture;
    public static explicit operator TextureView(LutraTexture texture) => texture.TextureView;

    #endregion

    #region Constructors

    public LutraTexture(uint textureSize)
    {
        Texture = VeldridResources.CreateSquareTexture(textureSize);
    }

    public LutraTexture(Stream fileStream)
    {
        var imageSharpTexture = new ImageSharpTexture(fileStream, false);
        Texture = VeldridResources.CreateTexture(imageSharpTexture);
    }

    public LutraTexture(ImageSharpTexture imageSharpTexture)
    {
        Texture = VeldridResources.CreateTexture(imageSharpTexture);
    }

    public LutraTexture(LutraTexture texture)
    {
        Texture = VeldridResources.CloneTexture((Texture)texture);
    }

    internal LutraTexture(Texture texture)
    {
        Texture = texture;
    }

    #endregion
}
