using System.Collections.Generic;
using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Graphic that is used for an animated sprite sheet.
/// </summary>
/// <typeparam name="TAnimType">The type to be used as keys for the animation dictionary.</typeparam>
public class Spritemap<TAnimType> : Image
{
    #region Public Fields

    /// <summary>
    /// The playback speed of all animations.  
    /// </summary>
    public float Speed = 1f;

    /// <summary>
    /// Whether or not we need to specify packed texture regions for frames
    /// </summary>
    public bool PackedTexture = false;

    public List<RectInt> PackedRegions = new List<RectInt>();

    #endregion

    #region Public Properties

    /// <summary>
    /// The total number of frames on the sprite sheet.
    /// </summary>
    public int Frames { get; private set; }

    /// <summary>
    /// The total number of columns on the sprite sheet.
    /// </summary>
    public int Columns { get; private set; }

    /// <summary>
    /// The total number of rows on the spirte sheet.
    /// </summary>
    public int Rows { get; private set; }

    /// <summary>
    /// Determines if the sprite is playing animations.
    /// </summary>
    public bool Active { get; private set; }

    /// <summary>
    /// Determines if the sprite is advancing its current animation.
    /// </summary>
    public bool Paused { get; private set; }

    /// <summary>
    /// The dictionary of stored animations.
    /// </summary>
    public Dictionary<TAnimType, Anim> Anims { get; private set; }

    /// <summary>
    /// The animation currently playing.
    /// </summary>
    public TAnimType CurrentAnim { get; protected set; }

    /// <summary>
    /// The current frame of the animation on the sprite sheet.
    /// </summary>
    public int CurrentFrame
    {
        get => Anims[CurrentAnim].CurrentFrame;
        set => SetFrame(value);
    }

    /// <summary>
    /// The current frame index of the animation, from 0 to frame count - 1.
    /// </summary>
    public int CurrentFrameIndex
    {
        get => Anims[CurrentAnim].CurrentFrameIndex;
        set => Anims[CurrentAnim].CurrentFrameIndex = value;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Spritemap from a file path.
    /// </summary>
    /// <param name="source">The file path to a texture to use for the sprite sheet.</param>
    /// <param name="width">The width of the animation.</param>
    /// <param name="height">The height of the animation.</param>
    public Spritemap(string source, int width, int height) : base(source)
    {
        Initialize(width, height);
    }

    /// <summary>
    /// Create a new Spritemap from a Texture.
    /// </summary>
    /// <param name="texture">The Texture to use for the sprite sheet.</param>
    /// <param name="width">The width of a cell on the sprite sheet.</param>
    /// <param name="height">The height of a cell on the sprite sheet.</param>
    public Spritemap(LutraTexture texture, int width, int height) : base(texture)
    {
        Initialize(width, height);
    }

    #endregion

    #region Private Methods

    private void Initialize(int width, int height, bool resetAnims = true)
    {
        if (resetAnims)
        {
            Anims = new();
        }

        Width = width;
        Height = height;

        Columns = (int)Math.Ceiling((float)TextureRegion.Width / Width);
        Rows = (int)Math.Ceiling((float)TextureRegion.Height / Height);

        Frames = Columns * Rows;

        UpdateTextureRegion(0);

        OnRender += UpdateBeforeRender;
    }

    private void UpdateSprite()
    {
        UpdateTextureRegion((int)Util.Clamp(Anims[CurrentAnim].CurrentFrame, 0, Frames));
    }

    private void UpdateTextureRegion(int frame)
    {
        if (PackedTexture)
        {
            TextureRegion = PackedRegions[frame];
            NeedsUpdate = true;
            return;
        }

        var top = (frame / Columns) * Height;
        var left = (frame % Columns) * Width;

        var newRegion = new RectInt(left, top, Width, Height);

        if (TextureRegion != newRegion)
        {
            NeedsUpdate = true;
            TextureRegion = newRegion;
        }
    }

    #endregion

    #region Indexers

    public Anim this[TAnimType anim] => GetAnimation(anim);

    #endregion

    #region Public Methods

    /// <summary>
    /// Add an animation to the list of Anims.
    /// </summary>
    /// <param name="a">The key to reference this animation.</param>
    /// <param name="anim">The anim value.</param>
    public void Add(TAnimType a, Anim anim)
    {
        Anims.Add(a, anim);
        CurrentAnim = a;
    }

    /// <summary>
    /// Adds an animation using a string for frames and a single value for frame delay.
    /// </summary>
    /// <param name="a">The key to store the animation with.</param>
    /// <param name="frames">The frames of the animation from the sprite sheet.  Example: "0,3,7-11,2,5"</param>
    /// <param name="framedelays">The delay between advancing to the next frame.</param>
    /// <returns>The added animation.</returns>
    public Anim Add(TAnimType a, string frames, float framedelays)
    {
        var anim = new Anim(frames, framedelays.ToString());
        Add(a, anim);
        return anim;
    }

    /// <summary>
    /// Add an animation using a string for frames and a string for framedelays.
    /// </summary>
    /// <param name="a">The key to store the animation with.</param>
    /// <param name="frames">The frames of the animation from the sprite sheet.  Example: "0,3,7-11,2,5"</param>
    /// <param name="framedelays">The duration of time to show each frame.  Example: "10,10,5,5,50"</param>
    /// <returns>The added animation.</returns>
    public Anim Add(TAnimType a, string frames, string framedelays)
    {
        var anim = new Anim(frames, framedelays);
        Add(a, anim);
        return anim;
    }

    /// <summary>
    /// Add an animation to the sprite.
    /// </summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="frames">An array of the frames to display.</param>
    /// <param name="frameDelay">An array of durations for each frame.</param>
    /// <returns>The added animation.</returns>
    public Anim Add(TAnimType a, int[] frames, float[] frameDelay = null)
    {
        var anim = new Anim(frames, frameDelay);
        Add(a, anim);
        return anim;
    }

    /// <summary>
    /// Adds an animation using an array for frames and a single value for frame delay.
    /// </summary>
    /// <param name="a">The key to store the animation with.</param>
    /// <param name="frames">The frames of the animation from the sprite sheet.  Example: "0,3,7-11,2,5"</param>
    /// <param name="framedelays">The delay between advancing to the next frame.</param>
    /// <returns>The added animation.</returns>
    public Anim Add(TAnimType a, int[] frames, float framedelays)
    {
        var anim = new Anim(frames, new float[] { framedelays });
        Add(a, anim);
        return anim;
    }

    /// <summary>
    /// Updates the animation. The sprite will not animate without this.
    /// Added to this Graphic's OnRender delegate when constructed.
    /// </summary>
    public void UpdateBeforeRender()
    {
        if (!Active) return;
        if (!Anims.ContainsKey(CurrentAnim)) return;
        if (Paused) return;

        Anims[CurrentAnim].Update(Speed);

        UpdateSprite();
    }

    /// <summary>
    /// Play the desired animation.
    /// </summary>
    /// <param name="a">The animation to play.</param>
    /// <param name="forceReset">Resets the animation back to the start before playing even if this is the same animation that was already playing.</param>
    public void Play(TAnimType a, bool forceReset = true)
    {
        Active = true;
        var pastAnim = CurrentAnim;
        CurrentAnim = a;
        if (Anims[CurrentAnim] != Anims[pastAnim] || forceReset)
        {
            Anims[CurrentAnim].Reset();
        }
        Anims[CurrentAnim].Active = true;
        UpdateSprite();
    }

    /// <summary>
    /// Get the animation with a specific key.
    /// </summary>
    /// <param name="a">The key to search with.</param>
    /// <returns>The animation found.</returns>
    public Anim GetAnimation(TAnimType a)
    {
        if (!Anims.ContainsKey(a)) return null;
        return Anims[a];
    }

    /// <summary>
    /// Pause the playback of the animation.
    /// </summary>
    public void Pause()
    {
        Paused = true;
    }

    /// <summary>
    /// Resume the animation from the current position.
    /// </summary>
    public void Resume()
    {
        Paused = false;
    }

    /// <summary>
    /// Stop playback.  This will reset the animation to the first frame.
    /// </summary>
    public void Stop()
    {
        Active = false;
        Anims[CurrentAnim].Stop();
    }

    /// <summary>
    /// Set the current animation to a specific frame.
    /// </summary>
    /// <param name="frame">The frame in terms of the animation.</param>
    public void SetFrame(int frame)
    {
        if (!Active) return;

        Anims[CurrentAnim].CurrentFrameIndex = frame;
        Anims[CurrentAnim].Reset();
    }

    /// <summary>
    /// Set the current animation to a specific frame and pause.
    /// </summary>
    /// <param name="frame">The frame in terms of the animation.</param>
    public void FreezeFrame(int frame)
    {
        if (!Active) return;

        Paused = true;
        Anims[CurrentAnim].CurrentFrameIndex = frame;

        UpdateTextureRegion(Anims[CurrentAnim].CurrentFrame);
    }

    /// <summary>
    /// Set the sprite to a frame on the sprite sheet itself.
    /// This will disable the current animation!
    /// </summary>
    /// <param name="frame">The global frame in terms of the sprite sheet.</param>
    public void SetGlobalFrame(int frame)
    {
        Active = false;
        UpdateTextureRegion(frame);
    }

    /// <summary>
    /// Resets the current animation back to the first frame.
    /// </summary>
    public void Reset()
    {
        Anims[CurrentAnim].Reset();
    }

    /// <summary>
    /// Clear the list of animations.
    /// </summary>
    public void Clear()
    {
        Anims.Clear();
    }

    #endregion
}
