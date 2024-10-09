using System.Numerics;
using Veldrid;

namespace Lutra.Rendering;

public struct VertexPosition(Vector2 position)
{
    public const byte SizeInBytes = 8;
    public const byte ElementCount = 1;

    public readonly Vector2 Position = position;
    public readonly static VertexLayoutDescription LayoutDescription = new(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );
}
