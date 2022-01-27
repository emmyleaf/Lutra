using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Internal class for managing characters in RichText.
/// </summary>
internal class RichTextCharacter
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
    Color activeOutlineColor = Color.White;

    #endregion

    #region Public Fields

    /// <summary>
    /// The character.
    /// </summary>
    public char Character;

    /// <summary>
    /// Timer used for animation.
    /// </summary>
    public float Timer;

    /// <summary>
    /// The sine wave offset for this specific character.
    /// </summary>
    public float CharOffset;

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
        get { return (color * activeColor).AlphaMultiply(FadeAmount); }
        set { activeColor = value; }
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
        get { return (color0 * activeColor0).AlphaMultiply(FadeAmount); }
        set { activeColor0 = value; }
    }

    /// <summary>
    /// The Color of the top left corner.
    /// </summary>
    public Color Color1
    {
        get { return (color1 * activeColor1).AlphaMultiply(FadeAmount); }
        set { activeColor1 = value; }
    }

    /// <summary>
    /// The Color of the top left corner.
    /// </summary>
    public Color Color2
    {
        get { return (color2 * activeColor2).AlphaMultiply(FadeAmount); }
        set { activeColor2 = value; }
    }

    /// <summary>
    /// The Color of the top left corner.
    /// </summary>
    public Color Color3
    {
        get { return (color3 * activeColor3).AlphaMultiply(FadeAmount); }
        set { activeColor3 = value; }
    }

    /// <summary>
    /// The Color of the shadow.
    /// </summary>
    public Color ShadowColor
    {
        get { return (shadowColor * activeShadowColor).AlphaMultiply(FadeAmount); }
        set { activeShadowColor = value; }
    }

    /// <summary>
    /// The Color of the outline.
    /// </summary>
    public Color OutlineColor
    {
        get { return (outlineColor * activeShadowColor).AlphaMultiply(FadeAmount); }
        set { activeShadowColor = value; }
    }

    /// <summary>
    /// The offset amount for each character.
    /// </summary>
    public float OffsetAmount
    {
        get { return offsetAmount + activeOffsetAmount; }
        set { activeOffsetAmount = value; }
    }

    /// <summary>
    /// The outline thickness.
    /// </summary>
    public float OutlineThickness
    {
        get { return outlineThickness + activeOutlineThickness; }
        set { activeOutlineThickness = value; }
    }

    /// <summary>
    /// The horizontal sine wave offset.
    /// </summary>
    public float SineOffsetX
    {
        get { return sineOffsetX + activeSineOffsetX; }
        set { activeSineOffsetX = value; }
    }

    /// <summary>
    /// The vertical sine wave offset.
    /// </summary>
    public float SineOffsetY
    {
        get { return sineOffsetY + activeSineOffsetY; }
        set { activeSineOffsetY = value; }
    }

    /// <summary>
    /// The X position of the shadow.
    /// </summary>
    public float ShadowX
    {
        get { return shadowX + activeShadowX; }
        set { activeShadowX = value; }
    }

    /// <summary>
    /// The Y position of the shadow.
    /// </summary>
    public float ShadowY
    {
        get { return shadowY + activeShadowY; }
        set { activeShadowY = value; }
    }

    /// <summary>
    /// The horizontal sine wave rate.
    /// </summary>
    public float SineRateX
    {
        get { return sineRateX + activeSineRateX; }
        set { activeSineRateX = value; }
    }

    /// <summary>
    /// The vertical sine wave rate.
    /// </summary>
    public float SineRateY
    {
        get { return sineRateY + activeSineRateY; }
        set { activeSineRateY = value; }
    }

    /// <summary>
    /// The amount of horizontal shake.
    /// </summary>
    public float ShakeX
    {
        get { return shakeX + activeShakeX; }
        set { activeShakeX = value; }
    }

    /// <summary>
    /// The amount of vertical shake.
    /// </summary>
    public float ShakeY
    {
        get { return shakeY + activeShakeY; }
        set { activeShakeY = value; }
    }

    /// <summary>
    /// The horizontal sine wave amplitude.
    /// </summary>
    public float SineAmpX
    {
        get { return sineAmpX + activeSineAmpX; }
        set { activeSineAmpX = value; }
    }

    /// <summary>
    /// The vertical sine wave amplitude.
    /// </summary>
    public float SineAmpY
    {
        get { return sineAmpY + activeSineAmpY; }
        set { activeSineAmpY = value; }
    }

    /// <summary>
    /// The X scale of the character.
    /// </summary>
    public float ScaleX
    {
        get { return scaleX * activeScaleX; }
        set { activeScaleX = value; }
    }

    /// <summary>
    /// The Y scale of the character.
    /// </summary>
    public float ScaleY
    {
        get { return scaleY * activeScaleY; }
        set { activeScaleY = value; }
    }

    /// <summary>
    /// The angle of the character.
    /// </summary>
    public float Angle
    {
        get { return angle + activeAngle; }
        set { activeAngle = value; }
    }

    /// <summary>
    /// The X position offset of the character.
    /// </summary>
    public float X
    {
        get { return activeX; }
        set { activeX = value; }
    }

    /// <summary>
    /// The Y position offset of the character.
    /// </summary>
    public float Y
    {
        get { return activeY; }
        set { activeY = value; }
    }

    /// <summary>
    /// The final horizontal offset position of the character when rendered.
    /// </summary>
    public float OffsetX
    {
        get
        {
            return finalShakeX + finalSinX + X;
        }
    }

    /// <summary>
    /// The final vertical offset position of the character when rendered.
    /// </summary>
    public float OffsetY
    {
        get
        {
            return finalShakeY + finalSinY + ((1.0f - FadeAmount) * -8.0f) + Y;
        }
    }

    public Action<char> OnSpeak;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new RichTextCharacter.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <param name="charOffset">The character offset for animation.</param>
    public RichTextCharacter(char character, int charOffset = 0)
    {
        Character = character;
        CharOffset = charOffset;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update the character.
    /// </summary>
    public void Update()
    {
        Timer += Game.Instance.DeltaTime;
        DelayTimer += Game.Instance.DeltaTime;

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
