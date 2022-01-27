using System.Numerics;
using Lutra.Rendering;
using Lutra.Utility;
using Vertex = Lutra.Rendering.VertexPositionColorTexture;

namespace Lutra.Graphics;

/// <summary>
/// Class containing all the info to describe a tile in a Tilemap.
/// </summary>
public class TileInfo
{
    #region Public Fields

    /// <summary>
    /// The X position of the tile.
    /// </summary>
    public int X;

    /// <summary>
    /// The Y position of the tile.
    /// </summary>
    public int Y;

    /// <summary>
    /// The X position of the source texture to render the tile from.
    /// </summary>
    public int TX;

    /// <summary>
    /// The Y position of the source texture to render the tile from.
    /// </summary>
    public int TY;

    /// <summary>
    /// The width of the tile.
    /// </summary>
    public int Width;

    /// <summary>
    /// The height of the tile.
    /// </summary>
    public int Height;

    /// <summary>
    /// Flipped tile options.
    /// </summary>
    public bool FlipX;
    public bool FlipY;

    /// <summary>
    /// Flips the tile anti-diagonally, equivalent to a 90 degree rotation and a horizontal flip.
    /// Combined with FlipX and FlipY you can rotate the tile any direction.
    /// </summary>
    public bool FlipD;

    /// <summary>
    /// The color of the tile, or the color to tint the texture.
    /// </summary>
    public Color Color;

    /// <summary>
    /// The alpha of the tile.
    /// </summary>
    public float Alpha
    {
        get { return Color.A; }
        set { Color = Color.WithAlpha(value); }
    }

    #endregion

    #region Constructors

    public TileInfo(int x, int y, int tx, int ty, int width, int height, Color color)
    {
        X = x;
        Y = y;
        TX = tx;
        TY = ty;
        Width = width;
        Height = height;
        Color = color;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the index of the tile on the source Texture of a Tilemap.
    /// </summary>
    /// <param name="tilemap">The Tilemap that uses the Texture to be tested against.</param>
    /// <returns>The index of the tile on the Tilemap's Texture.</returns>
    public int GetIndex(Tilemap tilemap)
    {
        return Util.OneDee((int)tilemap.Texture.Width / Width, TX / Width, TY / Height);
    }

    #endregion

    #region Internal

    internal Color tilemapColor;

    internal void AddVertices(SpriteDrawable drawable)
    {
        var texWidth = (float)drawable.Params.Texture.Width;
        var texHeight = (float)drawable.Params.Texture.Height;
        var tileColor = Color * tilemapColor;

        Vertex CreateVertex(int x, int y, int tx, int ty)
        {
            var u = (float)(TX + tx) / texWidth;
            var v = (float)(TY + ty) / texHeight;
            return new Vertex(new Vector2(X + x, Y + y), tileColor, new Vector2(u, v));
        }

        if (!FlipD)
        {
            if (!FlipX && !FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, 0, 0), //upper-left
                    CreateVertex(Width, 0, Width, 0), //upper-right
                    CreateVertex(0, Height, 0, Height), //lower-left
                    CreateVertex(Width, Height, Width, Height) //lower-right
                );
            }
            if (FlipX && FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, Width, Height),
                    CreateVertex(Width, 0, 0, Height),
                    CreateVertex(0, Height, Width, 0),
                    CreateVertex(Width, Height, 0, 0)
                );
            }
            if (FlipX & !FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, Width, 0),
                    CreateVertex(Width, 0, 0, 0),
                    CreateVertex(0, Height, Width, Height),
                    CreateVertex(Width, Height, 0, Height)
                );
            }
            if (!FlipX & FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, 0, Height),
                    CreateVertex(Width, 0, Width, Height),
                    CreateVertex(0, Height, 0, 0),
                    CreateVertex(Width, Height, Width, 0)
                );
            }
        }
        else
        { //swaps lower-left corner with upper-right on all the cases
            if (!FlipX && !FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, 0, 0), //upper-left
                    CreateVertex(0, Height, Width, 0), //upper-right
                    CreateVertex(Width, 0, 0, Height), //lower-left
                    CreateVertex(Width, Height, Width, Height) //lower-right
                );
            }
            if (FlipX && FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, Width, Height),
                    CreateVertex(0, Height, 0, Height),
                    CreateVertex(Width, 0, Width, 0),
                    CreateVertex(Width, Height, 0, 0)
                );
            }
            if (!FlipX & FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, Width, 0),
                    CreateVertex(0, Height, 0, 0),
                    CreateVertex(Width, 0, Width, Height),
                    CreateVertex(Width, Height, 0, Height)
                );
            }
            if (FlipX & !FlipY)
            {
                drawable.Add(
                    CreateVertex(0, 0, 0, Height),
                    CreateVertex(0, Height, Width, Height),
                    CreateVertex(Width, 0, 0, 0),
                    CreateVertex(Width, Height, Width, 0)
                );
            }
        }
    }

    #endregion
}
