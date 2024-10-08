using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Internal class for managing characters in RichText.
/// </summary>
/// <remarks>
/// Creates a new RichTextCharacter.
/// </remarks>
/// <param name="character">The character.</param>
/// <param name="charOffset">The character offset for animation.</param>
internal class RichTextCharacter(char character, int charOffset = 0)
{
    #region Private Fields

    float finalShakeX;
    float finalShakeY;
    float finalSinX;
    float finalSinY;

    float activeScaleX = 1;
    float activeScaleY = 1;
    float activeAngle;
    float activeX;
    float activeY;
    float activeSineAmpX;
    float activeSineAmpY;
    float activeSineRateX;
    float activeSineRateY;
    float activeSineOffsetX;
    float activeSineOffsetY;
    float activeShakeX;
    float activeShakeY;
    float activeShadowX;
    float activeShadowY;
    float activeOutlineThickness;
    float activeOffsetAmount;
    Color activeColor = Color.White;
    Color activeColor0 = Color.White;
    Color activeColor1 = Color.White;
    Color activeColor2 = Color.White;
    Color activeColor3 = Color.White;
    Color activeShadowColor = Color.White;
    readonly Color activeOutlineColor = Color.White;

    #endregion

    #region Public Fields

    /// <summary>
    /// The character.
    /// </summary>
    public char Character = character;

    /// <summary>
    /// Timer used for animation.
    /// </summary>
    public float Timer;

    /// <summary>
    /// The sine wave offset for this specific character.
    /// </summary>
    public float CharOffset = charOffset;

    /// <summary>
    /// Determines if the character is bold.  Not supported yet.
    /// </summary>
    public bool Bold = false;

    #endregion

    #region Public Properties

    /// <summary>
    /// The Color of the character.
    /// </summary>
    public Color Color
    {
        get => (color * activeColor).AlphaMultiply(FadeAmount);
        set => activeColor = value;
    }

    public float FadeAmount
    {
        get
        {
            bool shouldDelay = delay > 0.0f;
            float fadeAmt = 1.0f;

            if (shouldDelay)
            {
                // MODE 0
                if (DelayTimer < delay * (index + 1))
                {
                    fadeAmt = 0.0f;
                }

                // MODE 1 & 2
                if (DelayTimer > delay * (index))
                {
                    fadeAmt = Util.ScaleClamp(DelayTimer, delay * (index), delay * (index + 1), 0.0f, 1.0f);
                }
            }

            return fadeAmt;
        }
    }

    /// <summary>
    /// The Color of the top left corner.
    /// </summary>
    public Color Color0
    {
        get => (color0 * activeColor0).AlphaMultiply(FadeAmount);
        set => activeColor0 = value;
    }

    /// <summary>
    /// The Color of the top left corner.
    /// </summary>
    public Color Color1
    {
        get => (color1 * activeColor1).AlphaMultiply(FadeAmount);
        set => activeColor1 = value;
    }

    /// <summary>
    /// The Color of the top left corner.
    /// </summary>
    public Color Color2
    {
        get => (color2 * activeColor2).AlphaMultiply(FadeAmount);
        set => activeColor2 = value;
    }

    /// <summary>
    /// The Color of the top left corner.
    /// </summary>
    public Color Color3
    {
        get => (color3 * activeColor3).AlphaMultiply(FadeAmount);
        set => activeColor3 = value;
    }

    /// <summary>
    /// The Color of the shadow.
    /// </summary>
    public Color ShadowColor
    {
        get => (shadowColor * activeShadowColor).AlphaMultiply(FadeAmount);
        set => activeShadowColor = value;
    }

    /// <summary>
    /// The Color of the outline.
    /// </summary>
    public Color OutlineColor
    {
        get => (outlineColor * activeShadowColor).AlphaMultiply(FadeAmount);
        set => activeShadowColor = value;
    }

    /// <summary>
    /// The offset amount for each character.
    /// </summary>
    public float OffsetAmount
    {
        get => offsetAmount + activeOffsetAmount;
        set => activeOffsetAmount = value;
    }

    /// <summary>
    /// The outline thickness.
    /// </summary>
    public float OutlineThickness
    {
        get => outlineThickness + activeOutlineThickness;
        set => activeOutlineThickness = value;
    }

    /// <summary>
    /// The horizontal sine wave offset.
    /// </summary>
    public float SineOffsetX
    {
        get => sineOffsetX + activeSineOffsetX;
        set => activeSineOffsetX = value;
    }

    /// <summary>
    /// The vertical sine wave offset.
    /// </summary>
    public float SineOffsetY
    {
        get => sineOffsetY + activeSineOffsetY;
        set => activeSineOffsetY = value;
    }

    /// <summary>
    /// The X position of the shadow.
    /// </summary>
    public float ShadowX
    {
        get => shadowX + activeShadowX;
        set => activeShadowX = value;
    }

    /// <summary>
    /// The Y position of the shadow.
    /// </summary>
    public float ShadowY
    {
        get => shadowY + activeShadowY;
        set => activeShadowY = value;
    }

    /// <summary>
    /// The horizontal sine wave rate.
    /// </summary>
    public float SineRateX
    {
        get => sineRateX + activeSineRateX;
        set => activeSineRateX = value;
    }

    /// <summary>
    /// The vertical sine wave rate.
    /// </summary>
    public float SineRateY
    {
        get => sineRateY + activeSineRateY;
        set => activeSineRateY = value;
    }

    /// <summary>
    /// The amount of horizontal shake.
    /// </summary>
    public float ShakeX
    {
        get => shakeX + activeShakeX;
        set => activeShakeX = value;
    }

    /// <summary>
    /// The amount of vertical shake.
    /// </summary>
    public float ShakeY
    {
        get => shakeY + activeShakeY;
        set => activeShakeY = value;
    }

    /// <summary>
    /// The horizontal sine wave amplitude.
    /// </summary>
    public float SineAmpX
    {
        get => sineAmpX + activeSineAmpX;
        set => activeSineAmpX = value;
    }

    /// <summary>
    /// The vertical sine wave amplitude.
    /// </summary>
    public float SineAmpY
    {
        get => sineAmpY + activeSineAmpY;
        set => activeSineAmpY = value;
    }

    /// <summary>
    /// The X scale of the character.
    /// </summary>
    public float ScaleX
    {
        get => scaleX * activeScaleX;
        set => activeScaleX = value;
    }

    /// <summary>
    /// The Y scale of the character.
    /// </summary>
    public float ScaleY
    {
        get => scaleY * activeScaleY;
        set => activeScaleY = value;
    }

    /// <summary>
    /// The angle of the character.
    /// </summary>
    public float Angle
    {
        get => angle + activeAngle;
        set => activeAngle = value;
    }

    /// <summary>
    /// The X position offset of the character.
    /// </summary>
    public float X
    {
        get => activeX;
        set => activeX = value;
    }

    /// <summary>
    /// The Y position offset of the character.
    /// </summary>
    public float Y
    {
        get => activeY;
        set => activeY = value;
    }

    /// <summary>
    /// The final horizontal offset position of the character when rendered.
    /// </summary>
    public float OffsetX => finalShakeX + finalSinX + X;

    /// <summary>
    /// The final vertical offset position of the character when rendered.
    /// </summary>
    public float OffsetY => finalShakeY + finalSinY + ((1.0f - FadeAmount) * -8.0f) + Y;

    public Action<char> OnSpeak;

    #endregion
    #region Constructors

    #endregion

    #region Public Methods

    /// <summary>
    /// Update the character.
    /// </summary>
    public void Update()
    {
        // Always measure time in sixtieth seconds for text effects!
        Timer += Game.Instance.DeltaTime * (Game.Instance.MeasureTimeInSixtiethSeconds ? 1 : 60);
        DelayTimer += Game.Instance.DeltaTime * (Game.Instance.MeasureTimeInSixtiethSeconds ? 1 : 60);

        if (!Spoken && delay > 0f && DelayTimer > delay * (index))
        {
            OnSpeak?.Invoke(Character);
            Spoken = true;
        }

        finalShakeX = Rand.Float(-ShakeX, ShakeX);
        finalShakeY = Rand.Float(-ShakeY, ShakeY);
        finalSinX = Util.SinScale((Timer + SineOffsetX - CharOffset * OffsetAmount) * SineRateX, -SineAmpX, SineAmpX);
        finalSinY = Util.SinScale((Timer + SineOffsetY - CharOffset * OffsetAmount) * SineRateY, -SineAmpY, SineAmpY);
    }

    #endregion

    #region Internal

    internal float scaleX = 1;
    internal float scaleY = 1;
    internal float angle;
    internal float sineAmpX;
    internal float sineAmpY;
    internal float sineRateX = 1;
    internal float sineRateY = 1;
    internal float sineOffsetX;
    internal float sineOffsetY;
    internal float shakeX;
    internal float shakeY;
    internal float shadowX;
    internal float shadowY;
    internal float outlineThickness;
    internal float offsetAmount = 10;
    internal Color color = Color.White;
    internal Color color0 = Color.White;
    internal Color color1 = Color.White;
    internal Color color2 = Color.White;
    internal Color color3 = Color.White;
    internal Color shadowColor = Color.Black;
    internal Color outlineColor = Color.White;
    internal float delay = 0.0f;
    internal float DelayTimer = 0.0f;
    internal int index = 0;
    internal bool Spoken;

    #endregion
}
