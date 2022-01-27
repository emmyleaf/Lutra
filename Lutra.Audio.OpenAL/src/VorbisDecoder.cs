using System;
using System.IO;
using NVorbis;
using Silk.NET.OpenAL;

namespace Lutra.Audio.OpenAL;

internal class VorbisDecoder : IDisposable
{
    internal const int BYTES_PER_SAMPLE = 2;
    internal readonly VorbisReader Reader;
    internal readonly BufferFormat BufferFormat;

    private float[] floatBuffer;
    private int byteBufferSize;

    internal VorbisDecoder(Stream stream)
    {
        Reader = new VorbisReader(stream, true);
        BufferFormat = Reader.Channels switch
        {
            1 => BufferFormat.Mono16,
            2 => BufferFormat.Stereo16,
            _ => throw new NotSupportedException("Only mono or stereo Vorbis streams are supported.")
        };
    }

    internal int ReadSamples(Span<byte> data)
    {
        if (data.Length != byteBufferSize)
        {
            Array.Resize(ref floatBuffer, data.Length / BYTES_PER_SAMPLE);
            byteBufferSize = data.Length;
        }

        var samplesRead = Reader.ReadSamples(floatBuffer, 0, floatBuffer.Length);

        for (int f = 0, b = 0; f < samplesRead || b < byteBufferSize; f += 1, b += 2)
        {
            // Float32 to Int16
            var clampedFloat = Util.Clamp(floatBuffer[f], -1, 1);
            var int16 = (short)(short.MaxValue * clampedFloat);

            // Int16 to 2 bytes
            data[b] = (byte)((int16) & 0xFF);
            data[b + 1] = (byte)((int16) >> 8);
        }

        return samplesRead;
    }

    public void Dispose()
    {
        Reader.Dispose();
    }
}
