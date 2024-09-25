using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using System.IO;
using Veldrid;

namespace Lutra.Rendering;

public class ImageSharpTexture
{
    /// <summary>
    /// The ImageSharp Image.
    /// </summary>
    public Image<Rgba32> ISImage { get; }

    /// <summary>
    /// The width of the largest image in the chain.
    /// </summary>
    public uint Width => (uint)ISImage.Width;

    /// <summary>
    /// The height of the largest image in the chain.
    /// </summary>
    public uint Height => (uint)ISImage.Height;

    /// <summary>
    /// The pixel format of all images.
    /// </summary>
    public PixelFormat Format { get; }

    /// <summary>
    /// The size of each pixel, in bytes.
    /// </summary>
    public static uint PixelSizeInBytes => sizeof(byte) * 4;

    public ImageSharpTexture(string path) : this(Image.Load<Rgba32>(path), false) { }
    public ImageSharpTexture(string path, bool srgb) : this(Image.Load<Rgba32>(path), srgb) { }
    public ImageSharpTexture(Stream stream) : this(Image.Load<Rgba32>(stream), false) { }
    public ImageSharpTexture(Stream stream, bool srgb) : this(Image.Load<Rgba32>(stream), srgb) { }
    public ImageSharpTexture(Image<Rgba32> image) : this(image, false) { }
    public ImageSharpTexture(Image<Rgba32> image, bool srgb)
    {
        Format = srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm;
        ISImage = image;
    }

    public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory factory)
    {
        return CreateTextureViaUpdate(gd, factory);
    }

    private unsafe Texture CreateTextureViaUpdate(GraphicsDevice gd, ResourceFactory factory)
    {
        Texture tex = factory.CreateTexture(TextureDescription.Texture2D(
            Width, Height, 1, 1, Format, TextureUsage.Sampled));

        if (!ISImage.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixelMem))
        {
            throw new VeldridException("Unable to get image pixelspan.");
        }

        using (MemoryHandle pixelMemHandle = pixelMem.Pin())
        {
            gd.UpdateTexture(
                tex,
                (IntPtr)pixelMemHandle.Pointer,
                PixelSizeInBytes * Width * Height,
                0,
                0,
                0,
                Width,
                Height,
                1,
                0,
                0);
        }

        return tex;
    }
}
