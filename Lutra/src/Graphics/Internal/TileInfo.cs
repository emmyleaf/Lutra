using System;
using System.Numerics;
using Lutra.Rendering;
using Lutra.Utility;
using Vertex = Lutra.Rendering.VertexPositionColorTexture;

namespace Lutra.Graphics;

[Flags]
public enum TileRotationAndFlips
{
    // Flips
    FlipDiagonal = 1 << 0,
    FlipXAxis = 1 << 1,
    FlipYAxis = 1 << 2,

    // Rotations
    ZERO = 0,
    NINETY = FlipDiagonal | FlipXAxis,
    ONE_EIGHTY = FlipXAxis | FlipYAxis,
    TWO_SEVENTY = FlipDiagonal | FlipYAxis,
}

public static class TileRotationAndFlipsExt
{
    public static void SetFlag(this TileRotationAndFlips enumRef, TileRotationAndFlips flag, bool value)
    {
        if (value)
        {
            enumRef |= flag;
        }
        else
        {
            enumRef &= ~flag;
        }
    }
}

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
    /// The rotation and flips of the tile encoded as bitflags for each of the 3 flip directions.
    /// </summary>
    public TileRotationAndFlips RotationAndFlips;

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

    #region Properties
    /// <summary>
    /// Flips the tile anti-diagonally, equivalent to a 90 degree rotation and a horizontal flip.
    /// Combined with FlipX and FlipY you can rotate the tile any direction.
    /// </summary>
    public bool FlipD
    {
        get => RotationAndFlips.HasFlag(TileRotationAndFlips.FlipDiagonal);
        set => RotationAndFlips.SetFlag(TileRotationAndFlips.FlipDiagonal, value);
    }

    /// <summary>
    /// Flips the tile on the X-axis.
    /// </summary>
    public bool FlipX
    {
        get => RotationAndFlips.HasFlag(TileRotationAndFlips.FlipXAxis);
        set => RotationAndFlips.SetFlag(TileRotationAndFlips.FlipXAxis, value);
    }

    /// <summary>
    /// Flips the tile on the Y-axis.
    /// </summary>
    public bool FlipY
    {
        get => RotationAndFlips.HasFlag(TileRotationAndFlips.FlipYAxis);
        set => RotationAndFlips.SetFlag(TileRotationAndFlips.FlipYAxis, value);
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

        var vert1Pos = new Vector2(X, Y);
        var vert2Pos = new Vector2(X + Width, Y);
        var vert3Pos = new Vector2(X, Y + Height);
        var vert4Pos = new Vector2(X + Width, Y + Height);

        var texUpperLeft = new Vector2(TX / texWidth, TY / texHeight);
        var texUpperRight = new Vector2((TX + Width) / texWidth, TY / texHeight);
        var texLowerLeft = new Vector2(TX / texWidth, (TY + Height) / texHeight);
        var texLowerRight = new Vector2((TX + Width) / texWidth, (TY + Height) / texHeight);

        if (!FlipD)
        {
            if (!FlipX && !FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texUpperLeft),
                    new Vertex(vert2Pos, tileColor, texUpperRight),
                    new Vertex(vert3Pos, tileColor, texLowerLeft),
                    new Vertex(vert4Pos, tileColor, texLowerRight)
                );
            }
            if (FlipX && FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texLowerRight),
                    new Vertex(vert2Pos, tileColor, texLowerLeft),
                    new Vertex(vert3Pos, tileColor, texUpperRight),
                    new Vertex(vert4Pos, tileColor, texUpperLeft)
                );
            }
            if (FlipX & !FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texUpperRight),
                    new Vertex(vert2Pos, tileColor, texUpperLeft),
                    new Vertex(vert3Pos, tileColor, texLowerRight),
                    new Vertex(vert4Pos, tileColor, texLowerLeft)
                );
            }
            if (!FlipX & FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texLowerLeft),
                    new Vertex(vert2Pos, tileColor, texLowerRight),
                    new Vertex(vert3Pos, tileColor, texUpperLeft),
                    new Vertex(vert4Pos, tileColor, texUpperRight)
                );
            }
        }
        else
        { //swaps lower-left corner with upper-right on all the cases
            if (!FlipX && !FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texUpperLeft),
                    new Vertex(vert2Pos, tileColor, texLowerLeft),
                    new Vertex(vert3Pos, tileColor, texUpperRight),
                    new Vertex(vert4Pos, tileColor, texLowerRight)
                );
            }
            if (FlipX && FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texLowerRight),
                    new Vertex(vert2Pos, tileColor, texUpperRight),
                    new Vertex(vert3Pos, tileColor, texLowerLeft),
                    new Vertex(vert4Pos, tileColor, texUpperLeft)
                );
            }
            if (!FlipX & FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texUpperRight),
                    new Vertex(vert2Pos, tileColor, texLowerRight),
                    new Vertex(vert3Pos, tileColor, texUpperLeft),
                    new Vertex(vert4Pos, tileColor, texLowerLeft)
                );
            }
            if (FlipX & !FlipY)
            {
                drawable.Add(
                    new Vertex(vert1Pos, tileColor, texLowerLeft),
                    new Vertex(vert2Pos, tileColor, texUpperLeft),
                    new Vertex(vert3Pos, tileColor, texLowerRight),
                    new Vertex(vert4Pos, tileColor, texUpperRight)
                );
            }
        }
    }

    #endregion
}
