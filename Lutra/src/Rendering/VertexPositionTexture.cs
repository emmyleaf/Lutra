using System.Numerics;
using Veldrid;

namespace Lutra.Rendering;

public struct VertexPositionTexture
{
    public const byte SizeInBytes = 16;
    public const byte ElementCount = 2;

    public readonly Vector2 Position;
    public readonly Vector2 TextureCoords;

    public VertexPositionTexture(Vector2 position, Vector2 textureCoords)
    {
        Position = position;
        TextureCoords = textureCoords;
    }

    public readonly static VertexLayoutDescription LayoutDescription = new VertexLayoutDescription(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("TextureCoords", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );
}
