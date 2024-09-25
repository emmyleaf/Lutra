using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Graphic that renders a static image.
/// Can be repeated/flipped in X and/or Y directions.
/// </summary>
public class Image : SpriteGraphic
{
    #region Private Fields

    private bool repeatX, repeatY, flipX, flipY;
    private float dropshadowDistanceX, dropshadowDistanceY;
    private Color dropshadowColor = Color.Black;

    #endregion

    #region Public Properties

    /// <summary>
    /// Determines if the image should be rendered multiple times horizontally.
    /// If set to true, this also sets Dynamic to true. If Dynamic is false, the repetitions will not be updated with scrolling.
    /// </summary>
    public bool RepeatX
    {
        get => repeatX;
        set
        {
            repeatX = value;
            Dynamic |= value;
        }
    }

    /// <summary>
    /// Determines if the image should be rendered multiple times vertically.
    /// If set to true, this also sets Dynamic to true. If Dynamic is false, the repetitions will not be updated with scrolling.
    /// </summary>
    public bool RepeatY
    {
        get => repeatY;
        set
        {
            repeatY = value;
            Dynamic |= value;
        }
    }

    /// <summary>
    /// Set both RepeatX and RepeatY.
    /// If set to true, this also sets Dynamic to true. If Dynamic is false, the repetitions will not be updated with scrolling.
    /// </summary>
    public bool Repeat
    {
        set
        {
            repeatX = value;
            repeatY = value;
            Dynamic |= value;
        }
    }

    /// <summary>
    /// Flip the texture coordinates on the X axis every time the image is repeated horizontally.
    /// Only applies when RepeatX is true.
    /// </summary>
    public bool RepeatFlipX;

    /// <summary>
    /// Flip the texture coordinates on the Y axis every time the image is repeated vertically.
    /// Only applies when RepeatY is true.
    /// </summary>
    public bool RepeatFlipY;

    /// <summary>
    /// Flip the texture coordinates on the X axis.
    /// </summary>
    public bool FlippedX
    {
        get => flipX;
        set
        {
            flipX = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// Flip the texture coordinates on the Y axis.
    /// </summary>
    public bool FlippedY
    {
        get => flipY;
        set
        {
            flipY = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// The dropshadow colour of the Image. Default is black.
    /// </summary>
    public Color DropshadowColor
    {
        get
        {
            return dropshadowColor;
        }

        set
        {
            dropshadowColor = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// The distance of the drop shadow in the X direction.
    /// </summary>
    public float DropshadowDistanceX
    {
        get
        {
            return dropshadowDistanceX;
        }
        set
        {
            dropshadowDistanceX = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// The distance of the drop shadow in the Y direction.
    /// </summary>
    public float DropshadowDistanceY
    {
        get
        {
            return dropshadowDistanceY;
        }
        set
        {
            dropshadowDistanceY = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// Set both DropshadowDistanceX and DropshadowDistanceY.
    /// </summary>
    public float Dropshadow
    {
        set
        {
            dropshadowDistanceX = value;
            dropshadowDistanceY = value;
            NeedsUpdate = true;
        }
    }

    /// <summary>
    /// If true, the dropshadow distance will be applied before transformation.
    /// Otherwise, the transform will be applied first.
    /// </summary>
    public bool DropshadowSticky = false;

    #endregion

    #region Constructors

    public Image(string filepath) : this(AssetManager.GetTexture(filepath)) { }

    public Image(LutraTexture texture)
    {
        SetTexture(texture);
        Width = (int)texture.Width;
        Height = (int)texture.Height;
    }

    protected Image() { }

    #endregion

    protected override void UpdateDrawable()
    {
        InitializeDrawable(Texture, WorldMatrix);

        var flipState = GetFlipState();

        // If we don't repeat, we can draw once and exit early
        if (!repeatX && !repeatY)
        {
            DrawOne(0, 0, flipState);
            return;
        }

        var camera = Game.Instance.CameraManager.ActiveCamera;
        var repeatLeft = camera.Left - (camera.Width / camera.Scale);
        var repeatTop = camera.Top - (camera.Height / camera.Scale);
        var repeatRight = camera.Right + (camera.Width / camera.Scale);
        var repeatBottom = camera.Bottom + (camera.Height / camera.Scale);

        // We take the transform off again when we draw
        var initialX = Transform.X - Transform.OriginX;
        var initialY = Transform.Y - Transform.OriginY;

        if (ScrollX != 1)
        {
            initialX += camera.Left * (1 - ScrollX);
        }
        if (ScrollY != 1)
        {
            initialY += camera.Top * (1 - ScrollY);
        }

        var drawX = initialX;
        var drawY = initialY;

        // Drawing repeating images in the X direction is handled by this local function
        void DrawRepeatX()
        {
            while (drawX > repeatLeft)
            {
                drawX -= Width;
                if (RepeatFlipX) flipState ^= FlipState.FlippedX;
            }

            while (drawX < repeatRight)
            {
                DrawOne(drawX - initialX, drawY - initialY, flipState);

                drawX += Width;
                if (RepeatFlipX) flipState ^= FlipState.FlippedX;
            }
        }

        // Handle each combination of repeating in X and Y directions
        if (repeatX && !repeatY)
        {
            DrawRepeatX();
        }
        else if (repeatY)
        {
            while (drawY > repeatTop)
            {
                drawY -= Height;
                if (RepeatFlipY) flipState ^= FlipState.FlippedY;
            }

            while (drawY < repeatBottom)
            {
                if (repeatX)
                {
                    DrawRepeatX();
                }
                else
                {
                    DrawOne(drawX - initialX, drawY - initialY, flipState);
                }

                drawY += Height;
                if (RepeatFlipY) flipState ^= FlipState.FlippedY;
            }
        }
    }

    private void DrawOne(float drawX, float drawY, FlipState flipState)
    {
        // draw dropshadow
        if (dropshadowDistanceX != 0 || dropshadowDistanceY != 0)
        {
            // TODO: Handle DropshadowSticky

            var dropshadowRect = new RectFloat(drawX + dropshadowDistanceX, drawY + dropshadowDistanceY, Width, Height);
            SpriteDrawable.AddInstance(dropshadowRect, TextureRegion, dropshadowColor, flipState);
        }

        var destRect = new RectFloat(drawX, drawY, Width, Height);
        SpriteDrawable.AddInstance(destRect, TextureRegion, Color, flipState);
    }

    private FlipState GetFlipState()
    {
        if (flipX)
        {
            if (flipY) return FlipState.FlippedXY;
            return FlipState.FlippedX;
        }

        if (flipY) return FlipState.FlippedY;
        return FlipState.Normal;
    }
}
