using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Instanced Graphic that renders a static image.
/// </summary>
public class InstancedImage : Graphic
{
    #region Private Fields

    private int dropshadowDistanceX, dropshadowDistanceY;
    private Color dropshadowColor = Color.Black;

    #endregion

    #region Public Properties

    /// <summary>
    /// The dropshadow colour of the InstancedImage. Default is black.
    /// </summary>
    public Color DropshadowColor
    {
        get => dropshadowColor;

        set
        {
            dropshadowColor = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// The distance of the drop shadow in the X direction.
    /// </summary>
    public int DropshadowDistanceX
    {
        get => dropshadowDistanceX;
        set
        {
            dropshadowDistanceX = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// The distance of the drop shadow in the Y direction.
    /// </summary>
    public int DropshadowDistanceY
    {
        get => dropshadowDistanceY;
        set
        {
            dropshadowDistanceY = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// Set both DropshadowDistanceX and DropshadowDistanceY.
    /// </summary>
    public int Dropshadow
    {
        set
        {
            dropshadowDistanceX = value;
            dropshadowDistanceY = value;
            NeedsUpdate = true;
        }
    }

    #endregion

    #region Constructors

    public InstancedImage(string filepath) : this(AssetManager.GetTexture(filepath)) { }

    public InstancedImage(LutraTexture texture)
    {
        Texture = texture;
        Width = (int)texture.Width;
        Height = (int)texture.Height;
        TextureRegion = new RectInt(0, 0, (int)texture.Width, (int)texture.Height);
    }

    protected InstancedImage() { }

    #endregion

    protected internal override void Render()
    {
        // draw dropshadow
        if (dropshadowDistanceX != 0 || dropshadowDistanceY != 0)
        {
            var dropshadowRect = new RectFloat(dropshadowDistanceX, dropshadowDistanceY, Width, Height);
            Draw.Instanced(Texture, Layer, WorldMatrix, dropshadowRect, TextureRegion, dropshadowColor);
        }

        var destRect = new RectFloat(0, 0, Width, Height);
        Draw.Instanced(Texture, Layer, WorldMatrix, destRect, TextureRegion, Color);
    }
}
