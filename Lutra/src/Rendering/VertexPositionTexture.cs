using System.Numerics;
using Veldrid;

namespace Lutra.Rendering;

public struct VertexPositionTexture(Vector2 position, Vector2 textureCoords)
{
    public const byte SizeInBytes = 16;
    public const byte ElementCount = 2;

    public readonly Vector2 Position = position;
    public readonly Vector2 TextureCoords = textureCoords;
    public readonly static VertexLayoutDescription LayoutDescription = new(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("TextureCoords", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );
}
