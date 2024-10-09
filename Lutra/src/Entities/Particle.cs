using System.Collections.Generic;
using Lutra.Graphics;
using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Entities;

/// <summary>
/// Entity that is a quick way to make a particle.
/// </summary>
public class Particle : Entity
{
    #region Private Fields

    Color color;
    bool hasColor;

    bool useFrameList;

    float speedLen;
    bool hasSpeedLen;

    float speedDir;
    bool hasSpeedDir;

    int frameCount;
    bool hasFrameCount;

    float finalSpeedX;
    bool hasFinalSpeedX;

    float initSpeedX;

    float finalSpeedY;
    bool hasFinalSpeedY;

    float initSpeedY;

    float finalScaleX;
    bool hasFinalScaleX;

    float initScaleX;

    float finalScaleY;
    bool hasFinalScaleY;

    float initScaleY;

    float finalAngle;
    bool hasFinalAngle;

    float initAngle;

    float finalX;
    bool hasFinalX;

    float initX;

    float finalY;
    bool hasFinalY;

    float initY;

    float finalAlpha;
    bool hasFinalAlpha;

    float initAlpha;

    float finalColorR;
    bool hasFinalColorR;

    float initColorR;

    float finalColorG;
    bool hasFinalColorG;

    float initColorG;

    float finalColorB;
    bool hasFinalColorB;

    float initColorB;

    Color finalColor;
    bool hasFinalColor;

    Color initColor;

    float finalSpeedLen;
    bool hasFinalSpeedLen;

    float initSpeedLen;

    float finalSpeedDir;
    bool hasFinalSpeedDir;

    float initSpeedDir;

    float originX;
    float originY;

    bool useOrigin;

    bool useSpeedXY = true;
    bool tweenPosition = false;

    float xpos, ypos;

    float delayTimer = 0;
    float colorLerp;

    float initColorLerp;

    #endregion

    #region Public Static Fields

    public static int ActiveCount = 0;

    #endregion

    #region Public Fields

    /// <summary>
    /// The ease function to use when interpolating values.
    /// </summary>
    public Easer Ease;

    /// <summary>
    /// The ImageSet that the Particle is rendering.
    /// </summary>
    public InstancedImageSet Image;

    /// <summary>
    /// The default life span for all Particle Entities.
    /// </summary>
    public static int DefaultLifeSpan = 60;

    /// <summary>
    /// The initial horizontal speed.
    /// </summary>
    public float SpeedX;

    /// <summary>
    /// The initial vertical speed.
    /// </summary>
    public float SpeedY;

    /// <summary>
    /// The horizontal offset from the spawn position.
    /// </summary>
    public float OffsetX;

    /// <summary>
    /// The vertical offset from the spawn position.
    /// </summary>
    public float OffsetY;

    /// <summary>
    /// The initial X scale of the rendered Image.
    /// </summary>
    public float ScaleX = 1;

    /// <summary>
    /// The initial Y scale fo the rendered Image.
    /// </summary>
    public float ScaleY = 1;

    /// <summary>
    /// The angle of the Image.
    /// </summary>
    public float Angle;

    /// <summary>
    /// The initial alpha of the Image.
    /// </summary>
    public float Alpha = 1;

    /// <summary>
    /// The initial Color R component of the Image.
    /// </summary>
    public float ColorR = 1;

    /// <summary>
    /// The initial Color G component of the Image.
    /// </summary>
    public float ColorG = 1;

    /// <summary>
    /// The initial Color B component of the Image.
    /// </summary>
    public float ColorB = 1;

    /// <summary>
    /// The amount of time that must pass before the Entity is spawned.
    /// </summary>
    public float Delay;

    /// <summary>
    /// How many steps the particle should move by its speed when first created.
    /// </summary>
    public int AdvanceSteps;

    /// <summary>
    /// Determines if the image angle should be locked to the direction of the particle's movement.
    /// </summary>
    public bool MotionAngle;

    /// <summary>
    /// The starting frame in the ImageSet.
    /// </summary>
    public int FrameOffset;

    /// <summary>
    /// How many times to loop the ImageSet animation during the Particle's lifespan.
    /// </summary>
    public int Loops;

    /// <summary>
    /// Determines if the ScaleY will always be locked to the ScaleX.
    /// </summary>
    public bool LockScaleRatio;

    /// <summary>
    /// Determines if the Image will have a centered origin.
    /// </summary>
    public bool CenterOrigin = true;

    /// <summary>
    /// Flip the ImageSet horizontally.
    /// </summary>
    public bool FlipX = false;

    /// <summary>
    /// Flip the ImageSet vertically.
    /// </summary>
    public bool FlipY = false;

    /// <summary>
    /// Determines if the Particle should animate the ImageSet.
    /// </summary>
    public bool Animate;

    // /// <summary>
    // /// The shader to use on the ImageSet.
    // /// </summary>
    // public Shader Shader;

    /// <summary>
    /// The specific frames to display for the ImageSet.  If set it will override the default FrameCount.
    /// </summary>
    public List<int> Frames;

    #endregion

    #region Public Properties

    /// <summary>
    /// The Color of the Particle.  Overrides ColorR, ColorG, ColorB.
    /// </summary>
    public Color Color
    {
        set { color = value; hasColor = true; }
        get => color;
    }

    /// <summary>
    /// The magnitude of the particle's movement.  Overrides SpeedX and SpeedY.
    /// </summary>
    public float SpeedLen
    {
        set { speedLen = value; hasSpeedLen = true; }
        get => speedLen;
    }

    /// <summary>
    /// The direction of the particle's movement.  Overrides SpeedX and SpeedY.
    /// </summary>
    public float SpeedDir
    {
        set { speedDir = value; hasSpeedDir = true; }
        get => speedDir;
    }

    /// <summary>
    /// How many frames are present in the ImageSet for the Particle to animate through.
    /// </summary>
    public int FrameCount
    {
        set { frameCount = value; hasFrameCount = true; }
        get => frameCount;
    }

    /// <summary>
    /// The final horizontal speed of the Particle.
    /// </summary>
    public float FinalSpeedX
    {
        set { finalSpeedX = value; hasFinalSpeedX = true; }
        get => finalSpeedX;
    }

    /// <summary>
    /// The final vertical speed of the Particle.
    /// </summary>
    public float FinalSpeedY
    {
        set { finalSpeedY = value; hasFinalSpeedY = true; }
        get => finalSpeedY;
    }

    /// <summary>
    /// The final X scale of the rendered Image.
    /// </summary>
    public float FinalScaleX
    {
        set { finalScaleX = value; hasFinalScaleX = true; }
        get => finalScaleX;
    }

    /// <summary>
    /// The final Y scale of the rendered Image.
    /// </summary>
    public float FinalScaleY
    {
        set { finalScaleY = value; hasFinalScaleY = true; }
        get => finalScaleY;
    }

    /// <summary>
    /// The final angle of the rendered Image.
    /// </summary>
    public float FinalAngle
    {
        set { finalAngle = value; hasFinalAngle = true; }
        get => finalAngle;
    }

    /// <summary>
    /// The final X position of the Particle. If set this overrides speed values.
    /// </summary>
    public float FinalX
    {
        set { finalX = value; hasFinalX = true; }
        get => finalX;
    }

    /// <summary>
    /// The final Y position of the Particle. If set this overrides speed values.
    /// </summary>
    public float FinalY
    {
        set { finalY = value; hasFinalY = true; }
        get => finalY;
    }

    /// <summary>
    /// The final Alpha of the rendered Image.
    /// </summary>
    public float FinalAlpha
    {
        set { finalAlpha = value; hasFinalAlpha = true; }
        get => finalAlpha;
    }

    /// <summary>
    /// The final Color R component of the rendered Image.
    /// </summary>
    public float FinalColorR
    {
        set { finalColorR = value; hasFinalColorR = true; }
        get => finalColorR;
    }

    /// <summary>
    /// The final Color G component of the rendered Image.
    /// </summary>
    public float FinalColorG
    {
        set { finalColorG = value; hasFinalColorG = true; }
        get => finalColorG;
    }

    /// <summary>
    /// The final Color B component of the rendered Image.
    /// </summary>
    public float FinalColorB
    {
        set { finalColorB = value; hasFinalColorB = true; }
        get => finalColorB;
    }

    /// <summary>
    /// The final Color of the rendered Image.  If set will override the R G B components.
    /// </summary>
    public Color FinalColor
    {
        set { finalColor = value; hasFinalColor = true; }
        get => finalColor;
    }

    /// <summary>
    /// The final speed length of the Particle.  If set will override SpeedX and SpeedY.
    /// </summary>
    public float FinalSpeedLen
    {
        set { finalSpeedLen = value; hasFinalSpeedLen = true; }
        get => finalSpeedLen;
    }

    /// <summary>
    /// The final speed direction of the Particle.  If set will override SpeedX and SpeedY.
    /// </summary>
    public float FinalSpeedDir
    {
        set { finalSpeedDir = value; hasFinalSpeedDir = true; }
        get => finalSpeedDir;
    }

    /// <summary>
    /// The X origin of the rendered Image.
    /// </summary>
    public float OriginX
    {
        get => originX;
        set
        {
            originX = value;
            useOrigin = true;
        }
    }

    /// <summary>
    /// The Y origin of the rendered Image.
    /// </summary>
    public float OriginY
    {
        get => originY;
        set
        {
            originY = value;
            useOrigin = true;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Particle.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <param name="source">The source file path of the ImageSet.</param>
    /// <param name="width">The width of the ImageSet cell.</param>
    /// <param name="height">The height of the ImageSet cell.</param>
    public Particle(float x, float y, string source = null, int width = 0, int height = 0)
        : base(x, y)
    {
        Image = new InstancedImageSet(source, width, height);
    }

    /// <summary>
    /// Create a new Particle.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <param name="texture">The Texture to use for the ImageSet.</param>
    /// <param name="width">The width of the ImageSet cell.</param>
    /// <param name="height">The height of the ImageSet cell.</param>
    public Particle(float x, float y, LutraTexture texture, int width = 0, int height = 0)
        : base(x, y)
    {
        Image = new InstancedImageSet(texture, width, height);
    }

    /// <summary>
    /// Create a blank Particle.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    public Particle(float x, float y)
        : base(x, y) { }

    // /// <summary>
    // /// Create a new Particle.
    // /// </summary>
    // /// <param name="x">The x position.</param>
    // /// <param name="y">The y position.</param>
    // /// <param name="ImageSet">The ImageSet to use for the Particle.</param>
    // public Particle(float x, float y, ImageSet imageSet)
    //     : base(x, y)
    // {
    //     Image = imageSet;
    // }

    #endregion

    #region Public Methods

    /// <summary>
    /// Added to the Scene.
    /// </summary>
    public override void Added()
    {
        base.Added();

        if (Delay == 0) Start();
    }

    /// <summary>
    /// Start the Particle.
    /// </summary>
    public void Start()
    {
        if (LifeSpan == 0)
        {
            LifeSpan = DefaultLifeSpan;
        }

        if (Frames != null)
        {
            useFrameList = true;
            FrameCount = Frames.Count;
        }

        if (FrameCount > 0)
        {
            Animate = true;
        }

        if (hasSpeedLen || hasSpeedDir)
        {
            useSpeedXY = false;
        }
        if (hasFinalX || hasFinalY)
        {
            tweenPosition = true;
        }

        if (!tweenPosition)
        {
            if (useSpeedXY)
            {
                if (!hasFinalSpeedX)
                {
                    FinalSpeedX = SpeedX;
                }
                if (!hasFinalSpeedY)
                {
                    FinalSpeedY = SpeedY;
                }
                SpeedLen = 0;
                FinalSpeedLen = 0;
                SpeedDir = 0;
                FinalSpeedDir = 0;
            }
            else
            {
                FinalSpeedX = 0;
                FinalSpeedY = 0;
                if (!hasFinalSpeedLen)
                {
                    FinalSpeedLen = SpeedLen;
                }
                if (!hasFinalSpeedDir)
                {
                    FinalSpeedDir = SpeedDir;
                }
            }
            FinalX = 0;
            FinalY = 0;
        }
        else
        {
            xpos = X;
            ypos = Y;
            if (!hasFinalX)
            {
                FinalX = X;
            }
            if (!hasFinalY)
            {
                FinalY = Y;
            }
        }
        if (!hasFinalScaleX)
        {
            FinalScaleX = ScaleX;
        }
        if (!hasFinalScaleY)
        {
            FinalScaleY = ScaleY;
        }
        if (!hasFinalAngle)
        {
            FinalAngle = Angle;
        }
        if (!hasFinalAlpha)
        {
            FinalAlpha = Alpha;
        }
        if (!hasFinalColorR)
        {
            FinalColorR = ColorR;
        }
        if (!hasFinalColorG)
        {
            FinalColorG = ColorG;
        }
        if (!hasFinalColorB)
        {
            FinalColorB = ColorB;
        }

        initSpeedX = SpeedX;
        initSpeedY = SpeedY;
        initScaleX = ScaleX;
        initScaleY = ScaleY;
        initAngle = Angle;
        initAlpha = Alpha;
        initColorR = ColorR;
        initColorG = ColorG;
        initColorB = ColorB;
        initSpeedLen = SpeedLen;
        initSpeedDir = SpeedDir;
        initX = X;
        initY = Y;
        initColorLerp = colorLerp;
        initColor = Color;

        X += OffsetX;
        Y += OffsetY;

        AddGraphic(Image);

        if (useOrigin)
        {
            Image.Transform.OriginX = originX;
            Image.Transform.OriginY = originY;
        }
        else
        {
            if (CenterOrigin) Image.CenterOrigin();
        }

        for (var i = 0; i < AdvanceSteps; i++)
        {
            if (!useSpeedXY)
            {
                SpeedX = Util.PolarX(SpeedDir, SpeedLen);
                SpeedY = Util.PolarY(SpeedDir, SpeedLen);
            }
            X += SpeedX;
            Y += SpeedY;
        }

        Ease ??= Utility.Ease.Linear;

        // if (Shader != null)
        // {
        //     Image.Shader = Shader;
        // }

        Image.Visible = false;

        ActiveCount++;
    }

    /// <summary>
    /// Update the Particle.
    /// </summary>
    public override void Update()
    {
        base.Update();

        // Handle delay
        if (delayTimer < Delay)
        {
            delayTimer += Game.Instance.DeltaTime;
            Timer = 0;
            if (delayTimer == Delay)
            {
                Start();
            }
            return;
        }

        // Update values
        float lerp = Ease(Timer / LifeSpan);
        SpeedX = MathHelper.Lerp(initSpeedX, finalSpeedX, lerp);
        SpeedY = MathHelper.Lerp(initSpeedY, finalSpeedY, lerp);
        ScaleX = MathHelper.Lerp(initScaleX, finalScaleX, lerp);
        ScaleY = MathHelper.Lerp(initScaleY, finalScaleY, lerp);
        Angle = MathHelper.Lerp(initAngle, finalAngle, lerp);
        Alpha = MathHelper.Lerp(initAlpha, finalAlpha, lerp);
        ColorR = MathHelper.Lerp(initColorR, finalColorR, lerp);
        ColorG = MathHelper.Lerp(initColorG, finalColorG, lerp);
        ColorB = MathHelper.Lerp(initColorB, finalColorB, lerp);
        SpeedLen = MathHelper.Lerp(initSpeedLen, finalSpeedLen, lerp);
        SpeedDir = MathHelper.Lerp(initSpeedDir, finalSpeedDir, lerp);
        xpos = MathHelper.Lerp(initX, finalX, lerp);
        ypos = MathHelper.Lerp(initY, finalY, lerp);
        colorLerp = MathHelper.Lerp(0, 1, lerp);

        Image.Visible = true;

        // If the positions are not controlled by tweens.
        if (!tweenPosition)
        {
            // if SpeedDir and Len are being used
            if (!useSpeedXY)
            {
                SpeedX = Util.PolarX(SpeedDir, SpeedLen);
                SpeedY = Util.PolarY(SpeedDir, SpeedLen);
            }

            // Control the position with the particle speed.
            X += SpeedX * Scene.Game.DeltaTime;
            Y += SpeedY * Scene.Game.DeltaTime;
        }
        else
        {
            // Control the position with the interpolated/tweened position.
            X = xpos;
            Y = ypos;
        }

        // Set up animation
        if (hasFrameCount)
        {
            _ = FrameOffset + FrameCount;
        }
        else
        {
            _ = Image.Frames;
        }

        // Animate the particle
        if (Animate)
        {
            var playCount = Loops + 1;
            var frameIndex = (int)Util.ScaleClamp(Timer, 0, LifeSpan, 0, FrameCount * playCount);
            frameIndex %= FrameCount;
            if (useFrameList)
            {
                Image.Frame = Frames[frameIndex];
            }
            else
            {
                Image.Frame = frameIndex + FrameOffset;
            }
        }
        else
        {
            Image.Frame = 0;
        }

        // Control the image scale.
        Image.Transform.ScaleX = ScaleX;
        if (LockScaleRatio)
        {
            Image.Transform.ScaleY = ScaleX;
        }
        else
        {
            Image.Transform.ScaleY = ScaleY;
        }

        // Determines if the particle faces the direction of its speed vector.
        if (MotionAngle)
        {
            Image.Transform.Rotation = Util.Angle(SpeedX, SpeedY);
        }
        else
        {
            Image.Transform.Rotation = Angle;
        }

        // Control the blend mode.
        // Image.Blend = Blend;

        // Color the color.
        if (hasColor)
        {
            if (hasFinalColor)
            {
                Image.Color = Util.LerpColor(initColor, finalColor, colorLerp);
            }
            else
            {
                Image.Color = Color;
            }
            Image.Color = Image.Color.AlphaMultiply(Alpha);
        }
        else
        {
            Image.Color = new Color(ColorR, ColorG, ColorB, Alpha);
        }

        // Flip the image. // TODO: oops i already deleted that bit for simplicity
        // Image.FlippedX = FlipX;
        // Image.FlippedY = FlipY;
    }

    /// <summary>
    /// Removed from the Scene.
    /// </summary>
    public override void Removed()
    {
        base.Removed();

        Timer = 0;
        // ClearGraphics();
        ActiveCount--;
    }

    #endregion
}
