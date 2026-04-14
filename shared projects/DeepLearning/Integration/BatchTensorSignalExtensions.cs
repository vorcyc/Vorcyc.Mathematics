using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.DeepLearning.Integration;

/// <summary>
/// Converts between DSP <see cref="Signal"/> values and <see cref="BatchTensor{float}"/> batches.
/// Layout conventions:
/// <list type="bullet">
///   <item>Vector batch: N×1×1×L (one waveform per batch item, length L in channels).</item>
///   <item>Framed batch: N×F×L×1 (F frames of length L per item; N is usually 1).</item>
/// </list>
/// </summary>
public static class BatchTensorSignalExtensions
{
    /// <summary>
    /// Packs equal-length signals into an N×1×1×L batch tensor.
    /// </summary>
    public static BatchTensor<float> FromSignalVectors(IReadOnlyList<Signal> signals)
    {
        ArgumentNullException.ThrowIfNull(signals);
        if (signals.Count == 0)
        {
            throw new ArgumentException("At least one signal is required.", nameof(signals));
        }

        ValidateUniformSignals(signals, out var length, out var samplingRate);

        var batch = new BatchTensor<float>(signals.Count, 1, 1, length);
        for (var n = 0; n < signals.Count; n++)
        {
            var samples = signals[n].Samples;
            for (var i = 0; i < length; i++)
            {
                batch[n, 0, 0, i] = samples[i];
            }
        }

        return batch;
    }

    /// <summary>
    /// Packs a single signal as a 1×1×1×L batch tensor.
    /// </summary>
    public static BatchTensor<float> FromSignal(Signal signal)
        => FromSignalVectors([signal]);

    /// <summary>
    /// Extracts one waveform from an N×1×1×L batch tensor.
    /// </summary>
    public static Signal ToSignal(this BatchTensor<float> batch, int batchIndex, float samplingRate)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (batch.Height != 1 || batch.Width != 1)
        {
            throw new ArgumentException("Expected vector layout N×1×1×L.", nameof(batch));
        }

        if (batchIndex < 0 || batchIndex >= batch.Batch)
        {
            throw new ArgumentOutOfRangeException(nameof(batchIndex));
        }

        var output = new float[batch.Channels];
        for (var c = 0; c < batch.Channels; c++)
        {
            output[c] = batch[batchIndex, 0, 0, c];
        }

        return Signal.FromCopy(output, samplingRate);
    }

    /// <summary>
    /// Slices a signal into frames and packs them as 1×F×L×1 (frames along height).
    /// </summary>
    public static BatchTensor<float> FromSignalFrames(Signal signal, int frameSize, int hopSize)
    {
        ArgumentNullException.ThrowIfNull(signal);
        if (frameSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameSize));
        }

        if (hopSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hopSize));
        }

        if (signal.Length < frameSize)
        {
            throw new ArgumentException("Signal is shorter than frame size.", nameof(signal));
        }

        var frameCount = 1 + (signal.Length - frameSize) / hopSize;
        var batch = new BatchTensor<float>(1, frameCount, frameSize, 1);
        var samples = signal.Samples;

        for (var f = 0; f < frameCount; f++)
        {
            var start = f * hopSize;
            for (var w = 0; w < frameSize; w++)
            {
                batch[0, f, w, 0] = samples[start + w];
            }
        }

        return batch;
    }

    private static void ValidateUniformSignals(IReadOnlyList<Signal> signals, out int length, out float samplingRate)
    {
        length = signals[0].Length;
        samplingRate = signals[0].SamplingRate;

        for (var i = 1; i < signals.Count; i++)
        {
            if (signals[i].Length != length)
            {
                throw new ArgumentException("All signals must have the same length.", nameof(signals));
            }

            if (MathF.Abs(signals[i].SamplingRate - samplingRate) > 1e-3f)
            {
                throw new ArgumentException("All signals must share the same sampling rate.", nameof(signals));
            }
        }
    }
}
