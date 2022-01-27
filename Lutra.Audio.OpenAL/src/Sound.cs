using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenAL;

namespace Lutra.Audio.OpenAL;

/// <summary>
/// Class used to play a sound from a WavFile.
/// </summary>
public class Sound : IDisposable
{
    #region Private Fields

    private static float globalVolume = 1;
    private float volume = 1;

    private readonly uint buffer;
    private readonly uint source;

    #endregion

    #region Public Fields

    /// <summary>
    /// The duration of the sound in milliseconds.
    /// </summary>
    public readonly int Duration;

    #endregion

    #region Constructor

    /// <summary>
    /// Load a new sound from a WavFile.
    /// </summary>
    /// <param name="wavFile">The WavFile with sound data.</param>
    /// <param name="loop">Determines if the sound should loop (default false).</param>
    public Sound(WavFile wavFile, bool loop = false)
    {
        buffer = API.AL.GenBuffer();
        source = API.AL.GenSource();

        Duration = wavFile.Duration;

        SetBufferData(wavFile.Data, wavFile.BufferFormat, wavFile.SampleRate);

        API.AL.SetSourceProperty(source, SourceInteger.Buffer, buffer);
        API.AL.SetSourceProperty(source, SourceBoolean.Looping, loop);
        API.AL.SetSourceProperty(source, SourceBoolean.SourceRelative, false);
    }

    /// <summary>
    /// Load a new sound from an IO Stream containing Ogg Vorbis audio data.
    /// </summary>
    /// <param name="stream">The IO Stream.</param>
    /// <param name="loop">Determines if the sound should loop (default false).</param>
    public Sound(Stream stream, bool loop = false)
    {
        buffer = API.AL.GenBuffer();
        source = API.AL.GenSource();

        using var decoder = new VorbisDecoder(stream);

        Duration = (int)decoder.Reader.TotalTime.TotalMilliseconds;

        var bufferFormat = API.GetBufferFormat(decoder.Reader.Channels, VorbisDecoder.BYTES_PER_SAMPLE);
        var byteBufferSize = (int)(VorbisDecoder.BYTES_PER_SAMPLE * decoder.Reader.TotalSamples);
        var byteBuffer = new byte[byteBufferSize];

        decoder.ReadSamples(byteBuffer);

        SetBufferData(byteBuffer, bufferFormat, decoder.Reader.SampleRate);

        API.AL.SetSourceProperty(source, SourceInteger.Buffer, buffer);
        API.AL.SetSourceProperty(source, SourceBoolean.Looping, loop);
        API.AL.SetSourceProperty(source, SourceBoolean.SourceRelative, false);
    }

    private unsafe void SetBufferData(ReadOnlySpan<byte> data, BufferFormat format, int sampleRate)
    {
        fixed (byte* pointer = data)
        {
            API.AL.BufferData(buffer, format, pointer, data.Length, sampleRate);
        }
    }

    #endregion

    #region Static Properties

    /// <summary>
    /// The global volume of all sounds.
    /// </summary>
    public static float GlobalVolume
    {
        get => globalVolume;
        set => globalVolume = Util.Clamp01(value);
    }

    // /// <summary>
    // /// Where the Listener is in 3D space.
    // /// </summary>
    // public static Vector3 ListenerPosition
    // {
    //     set { }
    //     get { }
    // }

    // /// <summary>
    // /// What direction the Listener is pointing. Should be a unit vector.
    // /// </summary>
    // public static Vector3 ListenerDirection
    // {
    //     set { }
    //     get { }
    // }

    #endregion

    #region Public Properties

    /// <summary>
    /// The local volume of this sound.
    /// </summary>
    public float Volume
    {
        get => volume;
        set => volume = Util.Clamp01(value);
    }

    /// <summary>
    /// Adjust the pitch of the sound. Default value is 1.
    /// </summary>
    public float Pitch
    {
        get
        {
            API.AL.GetSourceProperty(source, SourceFloat.Pitch, out var pitch);
            return pitch;
        }
        set => API.AL.SetSourceProperty(source, SourceFloat.Pitch, value);
    }

    // /// <summary>
    // /// The playback offset of the sound in milliseconds.
    // /// </summary>
    // public int Offset
    // {
    //     set { }
    //     get { }
    // }

    /// <summary>
    /// Determines if the sound should loop or not.
    /// </summary>
    public bool Loop
    {
        get
        {
            API.AL.GetSourceProperty(source, SourceBoolean.Looping, out var loop);
            return loop;
        }
        set => API.AL.SetSourceProperty(source, SourceBoolean.Looping, value);
    }

    /// <summary>
    /// Whether or not the sound plays relative to the Listener position.
    /// Only mono sounds are able to be spatial.
    /// </summary>
    public bool RelativeToListener
    {
        get
        {
            API.AL.GetSourceProperty(source, SourceBoolean.SourceRelative, out var relative);
            return relative;
        }
        set => API.AL.SetSourceProperty(source, SourceBoolean.SourceRelative, value);
    }

    /// <summary>
    /// Where the sound is in 3D space.
    /// </summary>
    public Vector3 Position
    {
        get
        {
            API.AL.GetSourceProperty(source, SourceVector3.Position, out var position);
            return position;
        }
        set => API.AL.SetSourceProperty(source, SourceVector3.Position, value);
    }

    /// <summary>
    /// The sound's attenuation factor.
    /// Determines how the sound fades over distance.
    /// </summary>
    public float Attenuation
    {
        get
        {
            API.AL.GetSourceProperty(source, SourceFloat.RolloffFactor, out var attenuation);
            return attenuation;
        }
        set => API.AL.SetSourceProperty(source, SourceFloat.RolloffFactor, value);
    }

    /// <summary>
    /// The minimum distance to hear the sound at max volume.
    /// Past this distance the sound is faded according to it's attenuation.
    /// 0 is an invalid value.
    /// </summary>
    public float MinimumDistance
    {
        get
        {
            API.AL.GetSourceProperty(source, SourceFloat.ReferenceDistance, out var minDistance);
            return minDistance;
        }
        set => API.AL.SetSourceProperty(source, SourceFloat.ReferenceDistance, value);
    }

    /// <summary>
    /// Check if the Sound is currently playing.
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            API.AL.GetSourceProperty(source, GetSourceInteger.SourceState, out var state);
            return state == (int)SourceState.Playing;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Play the sound.
    /// </summary>
    public void Play()
    {
        API.AL.SetSourceProperty(source, SourceFloat.Gain, globalVolume * volume);
        API.AL.SourcePlay(source);
    }

    /// <summary>
    /// Stop the sound.
    /// </summary>
    public void Stop()
    {
        API.AL.SourceStop(source);
    }

    /// <summary>
    /// Pause the sound.
    /// </summary>
    public void Pause()
    {
        API.AL.SourcePause(source);
    }

    /// <summary>
    /// Release OpenAL resources.
    /// </summary>
    public void Dispose()
    {
        API.AL.DeleteSource(source);
        API.AL.DeleteBuffer(buffer);
    }

    #endregion
}
