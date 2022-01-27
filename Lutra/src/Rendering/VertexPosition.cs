using System.Numerics;
using Veldrid;

namespace Lutra.Rendering;

public struct VertexPosition
{
    public const byte SizeInBytes = 8;
    public const byte ElementCount = 1;

    public readonly Vector2 Position;

    public VertexPosition(Vector2 position)
    {
        Position = position;
    }

    public readonly static VertexLayoutDescription LayoutDescription = new VertexLayoutDescription(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );
}
