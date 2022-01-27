using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Silk.NET.OpenAL;

namespace Lutra.Audio.OpenAL;

/// <summary>
/// Class used to play music streamed from an IO Stream.
/// The Stream should contain Ogg Vorbis audio data.
/// </summary>
public class Music : IDisposable
{
    #region Static Fields

    private const int NUM_BUFFERS = 3;
    private const float SECONDS_PER_BUFFER = 0.2f;
    private static readonly TimeSpan THREAD_SLEEP_TIME = TimeSpan.FromSeconds(0.05);

    private static float globalVolume = 1f;
    private static List<Music> musicList = new List<Music>();

    #endregion

    #region Private Fields

    private bool disposed = false;
    private bool stopping = false;
    private float volume = 1f;

    private readonly int sampleRate;
    private readonly int channels;
    private readonly byte[] tempBuffer;

    private readonly uint[] bufferHandles;
    private uint sourceHandle;

    private readonly VorbisDecoder decoder;
    private readonly Thread decoderThread;

    #endregion

    #region Public Fields

    /// <summary>
    /// The duration of the music in milliseconds.
    /// </summary>
    public readonly int Duration;

    /// <summary>
    /// Determines if the music should loop or not.
    /// </summary>
    public bool Loop;

    #endregion

    #region Constructor

    /// <summary>
    /// Load a music stream from an IO Stream containing Ogg Vorbis audio data.
    /// </summary>
    /// <param name="stream">The IO Stream.</param>
    /// <param name="loop">Determines if the sound should loop (default true).</param>
    public Music(Stream stream, bool loop = true)
    {
        decoder = new VorbisDecoder(stream);

        Duration = (int)decoder.Reader.TotalTime.TotalMilliseconds;
        Loop = loop;

        sampleRate = decoder.Reader.SampleRate;
        channels = decoder.Reader.Channels;

        var byteBufferSize = (int)(channels * VorbisDecoder.BYTES_PER_SAMPLE * sampleRate * SECONDS_PER_BUFFER);
        tempBuffer = new byte[byteBufferSize];

        bufferHandles = API.AL.GenBuffers(NUM_BUFFERS);
        sourceHandle = API.AL.GenSource();

        API.AL.SetSourceProperty(sourceHandle, SourceBoolean.Looping, false);
        API.AL.SetSourceProperty(sourceHandle, SourceBoolean.SourceRelative, false);

        foreach (var bufferHandle in bufferHandles)
        {
            DecodeAndQueue(bufferHandle);
        }

        decoderThread = new Thread(DecodeLoop);
        decoderThread.Start();

        musicList.Add(this);
    }

    #endregion

    #region Static Properties

    /// <summary>
    /// The global volume to play all music at.
    /// </summary>
    public static float GlobalVolume
    {
        get => globalVolume;
        set
        {
            globalVolume = Util.Clamp01(value);
            foreach (var music in musicList)
            {
                API.AL.SetSourceProperty(music.sourceHandle, SourceFloat.Gain, globalVolume * music.volume);
            }
        }
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The local volume to play this music at.
    /// </summary>
    public float Volume
    {
        get => volume;
        set
        {
            volume = Util.Clamp01(value);
            API.AL.SetSourceProperty(sourceHandle, SourceFloat.Gain, globalVolume * volume);
        }
    }

    /// <summary>
    /// Adjust the pitch of the music.  Default value is 1.
    /// </summary>
    public float Pitch
    {
        get
        {
            API.AL.GetSourceProperty(sourceHandle, SourceFloat.Pitch, out var pitch);
            return pitch;
        }
        set => API.AL.SetSourceProperty(sourceHandle, SourceFloat.Pitch, value);
    }

    /// <summary>
    /// The playback offset of the music in milliseconds.
    /// </summary>
    public int Offset
    {
        get => (int)decoder.Reader.TimePosition.TotalMilliseconds;
        set => decoder.Reader.TimePosition = TimeSpan.FromMilliseconds(value);
    }

    /// <summary>
    /// Check if the Music is currently playing.
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            API.AL.GetSourceProperty(sourceHandle, GetSourceInteger.SourceState, out var state);
            return state == (int)SourceState.Playing;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Play the music.
    /// </summary>
    public void Play()
    {
        API.AL.SetSourceProperty(sourceHandle, SourceFloat.Gain, globalVolume * volume);
        API.AL.SourcePlay(sourceHandle);
    }

    /// <summary>
    /// Stop the music!
    /// </summary>
    public void Stop()
    {
        API.AL.SourceStop(sourceHandle);
        stopping = true;
    }

    /// <summary>
    /// Pause the music.
    /// </summary>
    public void Pause()
    {
        API.AL.SourcePause(sourceHandle);
    }

    /// <summary>
    /// Release OpenAL resources.
    /// </summary>
    public void Dispose()
    {
        disposed = true;
        decoderThread.Join();
        API.AL.DeleteSource(sourceHandle);
        API.AL.DeleteBuffers(bufferHandles);
        decoder.Dispose(); // Also disposes the underlying IO.Stream :)
    }

    #endregion

    #region Private Methods

    private void DecodeLoop()
    {
        while (!disposed)
        {
            if (stopping)
            {
                Reinitialize();
                Thread.Sleep(THREAD_SLEEP_TIME);
                continue;
            }

            API.AL.GetSourceProperty(sourceHandle, GetSourceInteger.BuffersProcessed, out var buffersProcessed);

            while (buffersProcessed > 0)
            {
                UnqueueDecodeAndQueue();
                buffersProcessed--;
            }

            Thread.Sleep(THREAD_SLEEP_TIME);
        }
    }

    private unsafe void UnqueueDecodeAndQueue()
    {
        uint bufferHandle;
        API.AL.SourceUnqueueBuffers(sourceHandle, 1, &bufferHandle);
        DecodeAndQueue(bufferHandle);
    }

    private unsafe void DecodeAndQueue(uint bufferHandle)
    {
        var samplesRead = decoder.ReadSamples(tempBuffer);
        var bytesRead = samplesRead * VorbisDecoder.BYTES_PER_SAMPLE;

        if (Loop && decoder.Reader.IsEndOfStream)
        {
            decoder.Reader.SeekTo(0);
        }

        fixed (byte* pointer = tempBuffer)
        {
            API.AL.BufferData(bufferHandle, decoder.BufferFormat, pointer, bytesRead, sampleRate);
        }

        API.AL.SourceQueueBuffers(sourceHandle, 1, &bufferHandle);
    }

    private unsafe void Reinitialize()
    {
        decoder.Reader.SeekTo(0);

        API.AL.DeleteSource(sourceHandle);
        sourceHandle = API.AL.GenSource();

        stopping = false;

        foreach (var bufferHandle in bufferHandles)
        {
            DecodeAndQueue(bufferHandle);
        }
    }

    #endregion
}
