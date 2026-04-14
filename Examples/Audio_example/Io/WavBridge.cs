using NAudio.Wave;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Audio_example.Io;

/// <summary>
/// NAudio WAV I/O ↔ Vorcyc <see cref="Signal"/> bridge.
/// </summary>
internal static class WavBridge
{
    public static Signal ReadAsSignal(string path, int? targetSampleRate = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("WAV file not found.", path);
        }

        using var reader = new AudioFileReader(path);
        int sourceRate = reader.WaveFormat.SampleRate;
        float[] samples = ReadAllSamples(reader);

        var signal = Signal.FromCopy(samples, sourceRate);
        int rate = targetSampleRate ?? sourceRate;
        if (rate != sourceRate)
        {
            signal = signal.Resample(rate);
        }

        return signal;
    }

    public static void WritePcm16(string path, Signal signal)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(signal);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        var format = new WaveFormat((int)signal.SamplingRate, 16, 1);
        using var writer = new WaveFileWriter(path, format);

        var samples = signal.Samples;
        var buffer = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            float clamped = Math.Clamp(samples[i], -1f, 1f);
            short pcm = (short)Math.Round(clamped * short.MaxValue);
            buffer[i * 2] = (byte)(pcm & 0xFF);
            buffer[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
        }

        writer.Write(buffer, 0, buffer.Length);
    }

    public static WavFileInfo Probe(string path)
    {
        using var reader = new AudioFileReader(path);
        return new WavFileInfo(
            Path.GetFileName(path),
            reader.WaveFormat.SampleRate,
            reader.WaveFormat.Channels,
            reader.WaveFormat.BitsPerSample,
            reader.TotalTime);
    }

    static float[] ReadAllSamples(ISampleProvider provider)
    {
        var list = new List<float>(capacity: 16_384);
        var buffer = new float[8192];
        int read;
        while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
        {
            list.AddRange(buffer.AsSpan(0, read).ToArray());
        }

        return list.ToArray();
    }
}

internal readonly record struct WavFileInfo(
    string FileName,
    int SampleRate,
    int Channels,
    int BitsPerSample,
    TimeSpan Duration);
