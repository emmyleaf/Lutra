using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Class used for animations in Spritemap.
/// </summary>
public partial class Anim
{
    #region Static Methods

    /// <summary>
    /// Returns an array of numbers from min to max.  Useful for passing in arguments for long animations.
    /// </summary>
    /// <param name="min">The start of the animation (includes this number.)</param>
    /// <param name="max">The end of the animation (includes this number.)</param>
    /// <returns>The array of ints representing an animation.</returns>
    public static int[] FramesRange(int min, int max)
    {
        int[] f = new int[max - min + 1];
        for (int i = min; i <= max; i++)
        {
            f[i - min] = i;
        }
        return f;
    }

    /// <summary>
    /// Creates an array of frames from a string expression. The expression must be similar to the following format:
    /// "0,3,7-11,2,5"
    /// Whitespace is permitted, and commas are optional.
    /// <param name="input">A string formatted as above, describing the frames to generate.</param>
    /// </summary>
    public static int[] ParseFrames(string input)
    {
        // Make sure the pattern matches, and alert the user if it doesn't
        var parse = SyntaxCheck().Match(input);
        if (!parse.Success)
            throw new Exception(string.Format("Invalid format: {0}", input));

        // Get all numbers/ranges in the input string.
        var frames = new List<int>();
        foreach (Match match in GetMatches().Matches(input).Cast<Match>())
        {
            var range = GetRange().Match(match.Value);
            if (range.Success)
            {
                int from = int.Parse(range.Groups[1].Value);
                int to = int.Parse(range.Groups[2].Value);

                // Support ascending and descending ranges
                if (from < to)
                {
                    while (from <= to)
                        frames.Add(from++);
                }
                else
                {
                    while (from >= to)
                        frames.Add(from--);
                }
            }
            else
            {
                frames.Add(int.Parse(match.Value));
            }
        }

        return [.. frames];
    }

    public static List<float> FrameDelaysForAnimAndEase(Anim anim, Easer easingFunc, int overTime, float holdFinalFrames)
    {
        var output = new List<float>();
        var numFrames = anim.Frames.Count;

        var timeSlices = new List<float>();

        for (int i = 0; i < numFrames; i++)
        {
            float percentile = i / (float)(numFrames - 1);
            float timeSlice = overTime * easingFunc(percentile);
            timeSlices.Add(timeSlice);
        }

        for (int i = 0; i < timeSlices.Count; i++)
        {
            if (i == timeSlices.Count - 1)
            {
                output.Add(holdFinalFrames);
            }
            else
            {
                var delay = timeSlices[i + 1] - timeSlices[i];
                output.Add(delay);
            }
        }

        return output;
    }

    #endregion

    #region Private Static Fields

    // Matches strings containing only numbers and number ranges, separated by single commas and/or whitespace
    [GeneratedRegex(@"^(?:\d+\s*-\s*\d+|\d\s*,?\s*)*$")]
    private static partial Regex SyntaxCheck();

    // Extracts a number or number range from a string
    [GeneratedRegex(@"((?:\d+\s*-\s*\d+)|(?:\d+))")]
    private static partial Regex GetMatches();

    // Extracts two numbers from a string containing a range
    [GeneratedRegex(@"(\d+)-(\d+)")]
    private static partial Regex GetRange();

    #endregion

    #region Private Fields

    bool pingPong;
    int loopBack = 0;
    float timer;
    int direction;
    int repeatsCounted = 0;
    bool isComplete = false;
    int currentFrame;

    #endregion

    #region Public Fields

    /// <summary>
    /// An action to run when the animation finishes playing.
    /// </summary>
    public Action OnComplete = delegate { };

    /// <summary>
    /// An action that is called when the Anim switches to a new frame.
    /// </summary>
    public Action OnNewFrame = delegate { };

    /// <summary>
    /// Determines if the animation is active (playing.)
    /// </summary>
    public bool Active;

    #endregion

    #region Public Properties

    /// <summary>
    /// The overall playback speed of the animation.
    /// </summary>
    public float PlaybackSpeed { get; private set; }

    /// <summary>
    /// The repeat count of the animation.
    /// </summary>
    public int RepeatCount { get; private set; }

    /// <summary>
    /// The frames used in the animation.
    /// </summary>
    public List<int> Frames { get; private set; }

    /// <summary>
    /// The frame delays used in the animation.
    /// </summary>
    public List<float> FrameDelays { get; private set; }

    /// <summary>
    /// The total number of frames in this animation.
    /// </summary>
    public int FrameCount => Frames.Count;

    /// <summary>
    /// The current frame of the animation.
    /// </summary>
    public int CurrentFrame => Frames[currentFrame];

    /// <summary>
    /// The current frame index of the animation.
    /// </summary>
    public int CurrentFrameIndex
    {
        get => currentFrame;
        set => currentFrame = value;
    }

    /// <summary>
    /// The total duration of the animation.
    /// </summary>
    public float TotalDuration
    {
        get
        {
            float delay = 0;
            foreach (float d in FrameDelays)
            {
                delay += d;
            }
            return delay;
        }
    }

    public int RepeatsCounted => repeatsCounted;

    public bool IsComplete => isComplete;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new Anim with an array of ints for frames, and an array of floats for frameDelays.
    /// </summary>
    /// <param name="frames">The frames from the sprite sheet to display.</param>
    /// <param name="frameDelays">The time that each frame should be displayed.</param>
    public Anim(int[] frames, float[] frameDelays = null)
    {
        Initialize(frames, frameDelays);
    }

    /// <summary>
    /// Creates a new Anim with a string of ints for frames, and a string of floats for frameDelays.
    /// </summary>
    /// <param name="frames">A string of frames separated by a delim character.  Example: "0,1,2-7,9,11"</param>
    /// <param name="frameDelays">A string of floats separated by a delim character.  Example: "0.5f,1,0.5f,1"</param>
    /// <param name="delim">The string of characters to parse the string by.  Default is ","</param>
    public Anim(string frames, string frameDelays)
    {
        string[] frameDelaysParts = frameDelays.Replace(" ", "").Split(",");
        float[] framedelaysfloat = new float[frameDelaysParts.Length];

        for (int i = 0; i < frameDelaysParts.Length; i++)
        {
            framedelaysfloat[i] = float.Parse(frameDelaysParts[i]);
        }

        Initialize(ParseFrames(frames), framedelaysfloat);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Determines how many times this animation loops.  -1 for infinite.
    /// </summary>
    /// <param name="times">How many times the animation should repeat.</param>
    /// <returns>The anim object.</returns>
    public Anim Repeat(int times = -1)
    {
        RepeatCount = times;
        return this;
    }

    /// <summary>
    /// Disables repeating.  Animations default to repeat on.
    /// </summary>
    /// <returns>The anim object.</returns>
    public Anim NoRepeat()
    {
        RepeatCount = 0;
        return this;
    }

    /// <summary>
    /// Determines if the animation will repeat by going back and forth between the start and end.
    /// </summary>
    /// <param name="pingpong">True for yes, false for no no no.</param>
    /// <returns>The anim object.</returns>
    public Anim PingPong(bool pingpong = true)
    {
        pingPong = pingpong;
        return this;
    }

    /// <summary>
    /// Determines the playback speed of the animation.  1 = 1 frame.
    /// </summary>
    /// <param name="speed">The new speed.</param>
    /// <returns>The anim object.</returns>
    public Anim Speed(float speed)
    {
        PlaybackSpeed = speed;
        return this;
    }

    /// <summary>
    /// Determines which frame the animation will loop back to when it repeats.
    /// </summary>
    /// <param name="frame">The frame to loop back to (from 0 to frame count - 1)</param>
    /// <returns>The anim object.</returns>
    public Anim LoopBackTo(int frame = 0)
    {
        loopBack = frame;
        return this;
    }

    /// <summary>
    /// Stops the animation and returns it to the first frame.
    /// </summary>
    /// <returns>The anim object.</returns>
    public Anim Stop()
    {
        Active = false;
        currentFrame = 0;
        return this;
    }

    /// <summary>
    /// Resets the animation back to frame 0 but does not stop it.
    /// </summary>
    /// <returns>The anim object.</returns>
    public Anim Reset()
    {
        currentFrame = 0;
        timer = FrameDelays[currentFrame];
        repeatsCounted = 0;
        isComplete = false;
        return this;
    }

    /// <summary>
    /// Updates the Anim object.  Handled by the Spritemap usually.  If this doesn't run the animation will not play.
    /// </summary>
    /// <param name="t">The time scale.</param>
    public virtual void Update(float t = 1f)
    {
        if (Active)
        {
            timer -= PlaybackSpeed * (Game.Instance.DeltaTime) * t;

            if (timer <= 0)
            {
                AdvanceFrame();
            }
        }
    }

    #endregion

    #region Private Methods

    private void Initialize(int[] frames, float[] frameDelays = null)
    {
        Frames = [];
        FrameDelays = [];

        for (int i = 0; i < frames.Length; i++)
        {
            Frames.Add(frames[i]);
            if (frameDelays != null)
            {
                if (i >= frameDelays.Length)
                {
                    FrameDelays.Add(frameDelays[i % frameDelays.Length]);
                }
                else
                {
                    FrameDelays.Add(frameDelays[i]);
                }
            }
            else
            {
                FrameDelays.Add(1);
            }
        }

        RepeatCount = -1;
        timer = FrameDelays[0];
        direction = 1;
        PlaybackSpeed = 1;

        Active = true;
    }

    private void AdvanceFrame()
    {
        currentFrame += direction;

        if (currentFrame == Frames.Count)
        {
            if (repeatsCounted < RepeatCount || RepeatCount < 0)
            {
                repeatsCounted++;
                if (pingPong)
                {
                    direction *= -1;
                    currentFrame = Frames.Count - 2;
                }
                else
                {
                    currentFrame = loopBack;
                }
            }
            else
            {
                if (pingPong)
                {
                    direction *= -1;
                    currentFrame = Frames.Count - 2;
                }
                else
                {
                    OnCompleteInternal();
                    Stop();
                    currentFrame = Frames.Count - 1;
                }
            }
        }

        if (currentFrame < loopBack)
        {
            if (pingPong)
            {
                if (repeatsCounted < RepeatCount || RepeatCount < 0)
                {
                    repeatsCounted++;
                    direction *= -1;
                    currentFrame = loopBack + 1;
                }
                else
                {
                    OnCompleteInternal();
                    Stop();
                }
            }
        }

        timer = FrameDelays[currentFrame];

        OnNewFrame();
    }

    private void OnCompleteInternal()
    {
        isComplete = true;
        OnComplete();
    }

    #endregion
}
