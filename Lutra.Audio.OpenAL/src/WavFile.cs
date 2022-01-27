using System.IO;
using Silk.NET.OpenAL;

namespace Lutra.Audio.OpenAL;

/// <summary>
/// Class containing wav file data. Can be constructed from an IO Stream.
/// The Stream should contain a PCM WAV file, 8 or 16-bit, 1 or 2 channels.
/// Lutra's AssetManager can be used to cache WavFiles.
/// </summary>
public class WavFile
{
    public readonly int SampleRate;
    public readonly int Duration;
    public readonly BufferFormat BufferFormat;
    public readonly byte[] Data;

    private WavFile(int sampleRate, int duration, BufferFormat bufferFormat, byte[] data)
    {
        SampleRate = sampleRate;
        Duration = duration;
        BufferFormat = bufferFormat;
        Data = data;
    }

    public static WavFile FromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        // Parse RIFF header
        var chunkId = new string(reader.ReadChars(4));
        _ = reader.ReadUInt32();
        var chunkFormat = new string(reader.ReadChars(4));

        // Parse fmt subchunk
        var fmtSubchunkID = new string(reader.ReadChars(4));
        _ = reader.ReadUInt32();
        var isPCMFormat = reader.ReadUInt16() == 1u;

        if (chunkId != "RIFF" || chunkFormat != "WAVE" || fmtSubchunkID != "fmt " || !isPCMFormat)
        {
            throw new InvalidDataException("Invalid or missing PCM WAV file chunk!");
        }

        var numChannels = reader.ReadUInt16();
        var sampleRate = (int)reader.ReadUInt32();
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt16();
        var bitsPerSample = reader.ReadUInt16();

        var bytesPerSample = bitsPerSample / 8;

        // Parse data subchunk
        var dataSubchunkID = new string(reader.ReadChars(4));
        var dataSubchunkSize = reader.ReadUInt32();

        if (dataSubchunkID != "data")
        {
            throw new InvalidDataException("Invalid or missing .wav file data chunk!");
        }

        var data = reader.ReadBytes((int)dataSubchunkSize);
        var duration = (data.Length * 1000) / (sampleRate * bytesPerSample * numChannels);

        // lmao dirty hack for truncating higher bit-depth files
        if (bytesPerSample > 2)
        {
            var truncData = new byte[(data.Length * 2) / bytesPerSample];
            for (int i = 0, j = 0; i < data.Length; i += bytesPerSample, j += 2)
            {
                truncData[j] = data[i + bytesPerSample - 2];
                truncData[j + 1] = data[i + bytesPerSample - 1];
            }
            var truncBufferFormat = API.GetBufferFormat(numChannels, 2);
            return new WavFile(sampleRate, duration, truncBufferFormat, truncData);
        }

        var bufferFormat = API.GetBufferFormat(numChannels, bytesPerSample);
        return new WavFile(sampleRate, duration, bufferFormat, data);
    }
}
