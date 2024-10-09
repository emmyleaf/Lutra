using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Graphic that renders a static shape.
/// </summary>
public class Shape : SpriteGraphic
{
    private static LutraTexture OnePxWhite;

    private int radius, rectWidth, rectHeight;
    private Color outlineColor = Color.None;
    private float outlineThickness;

    /// <summary>
    /// The amount of points to use when rendering a circle shape.
    /// </summary>
    public static int CirclePointCount = 24;

    /// <summary>
    /// The outline color of the Shape.
    /// </summary>
    public Color OutlineColor
    {
        get => outlineColor;
        set
        {
            outlineColor = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// The outline thickness of the Shape.
    /// </summary>
    public float OutlineThickness
    {
        get => outlineThickness;
        set
        {
            outlineThickness = value;
            NeedsUpdate = true;
        }
    }

    #region Static Methods

    /// <summary>
    /// Creates a rectangle.
    /// </summary>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <param name="color">The color of the rectangle.</param>
    /// <returns>A new shape containing the rectangle.</returns>
    static public Shape CreateRectangle(int width, int height, Color color)
    {
        Shape shape = new(width, height);
        shape.rectWidth = width;
        shape.rectHeight = height;
        shape.Color = color;

        return shape;
    }

    /// <summary>
    /// Creates a rectangle the size of the active Game window.
    /// </summary>
    /// <param name="color">The color of the rectangle.</param>
    /// <returns>A new shape containing the rectangle.</returns>
    static public Shape CreateRectangle(Color color)
    {
        int w = Game.Instance.Width;
        int h = Game.Instance.Height;
        return Shape.CreateRectangle(w, h, color);
    }

    /// <summary>
    /// Creates a simple black rectangle the size of the active Game window.
    /// </summary>
    /// <returns>A new shape containing the rectangle.</returns>
    static public Shape CreateRectangle()
    {
        return Shape.CreateRectangle(Color.Black);
    }

    /// <summary>
    /// Creates a rectangle.
    /// </summary>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <returns>A new shape containing the rectangle.</returns>
    static public Shape CreateRectangle(int width, int height)
    {
        return CreateRectangle(width, height, Color.White);
    }

    /// <summary>
    /// Creates a rectangle.
    /// </summary>
    /// <param name="size">The width and height of the rectangle.</param>
    /// <returns>A new shape containing the rectangle.</returns>
    static public Shape CreateRectangle(int size)
    {
        return CreateRectangle(size, size);
    }

    /// <summary>
    /// Creates a rectangle.
    /// </summary>
    /// <param name="size">The width and height of the rectangle.</param>
    /// <param name="color">The color of the rectangle.</param>
    /// <returns>A new shape containing the rectangle.</returns>
    static public Shape CreateRectangle(int size, Color color)
    {
        return CreateRectangle(size, size, color);
    }

    /// <summary>
    /// Create a circle.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <returns>A new shape containing the circle.</returns>
    static public Shape CreateCircle(int radius, Color color)
    {
        Shape shape = new(radius * 2, radius * 2);
        shape.radius = radius;
        shape.Color = color;

        return shape;
    }

    /// <summary>
    /// Create a white circle.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>A new shape containing the circle.</returns>
    static public Shape CreateCircle(int radius)
    {
        return CreateCircle(radius, Color.White);
    }

    #endregion

    private Shape(int width, int height)
    {
        if (OnePxWhite == null)
        {
            OnePxWhite = new LutraTexture(1u);
            var white = new byte[] { 255, 255, 255, 255 };
            VeldridResources.UpdateTexture(OnePxWhite.Texture, white, new RectInt(0, 0, 1, 1));
        }

        Width = width;
        Height = height;
        Texture = OnePxWhite;
        TextureRegion = new RectInt(0, 0, 1, 1);
    }

    protected override void UpdateDrawable()
    {
        InitializeDrawable(Texture, WorldMatrix);

        if (radius != 0)
        {
            throw new NotImplementedException(); // TODO: Circles?!
        }
        else
        {
            SpriteDrawable.AddInstance(0, 0, rectWidth, rectHeight, 0, 0, 1, 1, Color);
        }
    }
}
