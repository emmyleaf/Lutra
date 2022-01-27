using System;
using Silk.NET.OpenAL;

namespace Lutra.Audio.OpenAL;

/// <summary>
/// Class containing the OpenAL API, device & context for the Sound and Music classes.
/// </summary>
internal static unsafe class API
{
    internal static readonly ALContext ALC;
    internal static readonly AL AL;

    private static readonly Device* device;
    private static readonly Context* context;

    static API()
    {
        ALC = ALContext.GetApi(true);
        AL = AL.GetApi(true);

        device = ALC.OpenDevice(null);
        context = ALC.CreateContext(device, null);
        ALC.MakeContextCurrent(context);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => Dispose();
    }

    static void Dispose()
    {
        ALC.MakeContextCurrent(0);
        ALC.DestroyContext(context);
        ALC.CloseDevice(device);

        AL.Dispose();
        ALC.Dispose();
    }

    internal static BufferFormat GetBufferFormat(int numChannels, int bytesPerSample)
    {
        if (numChannels == 1)
        {
            if (bytesPerSample == 1) return BufferFormat.Mono8;
            if (bytesPerSample == 2) return BufferFormat.Mono16;
        }

        if (numChannels == 2)
        {
            if (bytesPerSample == 1) return BufferFormat.Stereo8;
            if (bytesPerSample == 2) return BufferFormat.Stereo16;
        }

        throw new NotSupportedException($"Unsupported audio format: {numChannels} channels, {bytesPerSample * 8} bit");
    }
}
