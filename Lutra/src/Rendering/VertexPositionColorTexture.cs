using System.Numerics;
using Veldrid;

namespace Lutra.Rendering;

public struct VertexPositionColorTexture
{
    public const byte SizeInBytes = 32;
    public const byte ElementCount = 3;

    public readonly Vector2 Position;
    public readonly RgbaFloat Color;
    public readonly Vector2 TextureCoords;

    public VertexPositionColorTexture(Vector2 position, RgbaFloat color, Vector2 textureCoords)
    {
        Position = position;
        Color = color;
        TextureCoords = textureCoords;
    }

    public readonly static VertexLayoutDescription LayoutDescription = new VertexLayoutDescription(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("TextureCoords", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );
}
