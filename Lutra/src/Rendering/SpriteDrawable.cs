using System.Collections.Generic;
using System.Numerics;
using Lutra.Utility;
using Vertex = Lutra.Rendering.VertexPositionColorTexture;

namespace Lutra.Rendering;

public readonly record struct SpriteParams(LutraTexture Texture, Matrix4x4 WorldMatrix);

public class SpriteDrawable(LutraTexture texture, Matrix4x4 worldMatrix)
{
    public SpriteParams Params = new(texture, worldMatrix);
    public List<Vertex> Vertices = [];

    public void Reinitialise(LutraTexture texture, Matrix4x4 worldMatrix)
    {
        Params = new(texture, worldMatrix);
        Vertices.Clear();
    }

    public void Add(Vertex TopLeft, Vertex TopRight, Vertex BottomLeft, Vertex BottomRight)
    {
        Vertices.Add(TopLeft);
        Vertices.Add(TopRight);
        Vertices.Add(BottomLeft);
        Vertices.Add(BottomRight);
    }

    public void AddInstance(Vector2 position, RectInt sourceRect, Color color)
    {
        var texWidth = (float)Params.Texture.Width;
        var texHeight = (float)Params.Texture.Height;

        var u1 = sourceRect.Left / texWidth;
        var v1 = sourceRect.Top / texHeight;
        var u2 = sourceRect.Right / texWidth;
        var v2 = sourceRect.Bottom / texHeight;

        Vertices.Add(new Vertex(position, color, new Vector2(u1, v1)));
        Vertices.Add(new Vertex(new Vector2(position.X + sourceRect.Width, position.Y), color, new Vector2(u2, v1)));
        Vertices.Add(new Vertex(new Vector2(position.X, position.Y + sourceRect.Height), color, new Vector2(u1, v2)));
        Vertices.Add(new Vertex(new Vector2(position.X + sourceRect.Width, position.Y + sourceRect.Height), color, new Vector2(u2, v2)));
    }

    // If ever another method was used by Surface, we would need to duplicate this OpenGL flipping logic!
    public void AddInstance(RectFloat destRect, RectInt sourceRect, Color color, FlipState flipState = FlipState.Normal)
    {
        var openGLRenderTarget = VeldridResources.IsOpenGL && Params.Texture.IsRenderTarget;
        var texWidth = (float)Params.Texture.Width;
        var texHeight = (float)Params.Texture.Height;
        var flippedX = flipState.HasFlag(FlipState.FlippedX);
        var flippedY = flipState.HasFlag(FlipState.FlippedY) ^ openGLRenderTarget;

        var u1 = (flippedX ? sourceRect.Right : sourceRect.Left) / texWidth;
        var v1 = (flippedY ? sourceRect.Bottom : sourceRect.Top) / texHeight;
        var u2 = (flippedX ? sourceRect.Left : sourceRect.Right) / texWidth;
        var v2 = (flippedY ? sourceRect.Top : sourceRect.Bottom) / texHeight;

        Vertices.Add(new Vertex(destRect.TopLeft, color, new Vector2(u1, v1)));
        Vertices.Add(new Vertex(destRect.TopRight, color, new Vector2(u2, v1)));
        Vertices.Add(new Vertex(destRect.BottomLeft, color, new Vector2(u1, v2)));
        Vertices.Add(new Vertex(destRect.BottomRight, color, new Vector2(u2, v2)));
    }

    public void AddInstance(float x1, float y1, float x2, float y2, float u1, float v1, float u2, float v2, Color color)
    {
        var texWidth = (float)Params.Texture.Width;
        var texHeight = (float)Params.Texture.Height;

        u1 /= texWidth;
        v1 /= texHeight;
        u2 /= texWidth;
        v2 /= texHeight;

        Vertices.Add(new Vertex(new Vector2(x1, y1), color, new Vector2(u1, v1)));
        Vertices.Add(new Vertex(new Vector2(x2, y1), color, new Vector2(u2, v1)));
        Vertices.Add(new Vertex(new Vector2(x1, y2), color, new Vector2(u1, v2)));
        Vertices.Add(new Vertex(new Vector2(x2, y2), color, new Vector2(u2, v2)));
    }
}
