using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.DeepLearning.Integration;

/// <summary>
/// Builds N×T×F×1 batch tensors from STFT magnitude spectrograms
/// (T = time frames, F = frequency bins).
/// </summary>
public static class SpectrogramBatchBuilder
{
    /// <summary>
    /// Converts one signal's magnitude spectrogram to 1×T×F×1.
    /// </summary>
    public static BatchTensor<float> FromSignal(Signal signal, Stft stft, bool normalize = true)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(stft);

        var frames = stft.Spectrogram(signal, normalize);
        return FromSpectrogramFrames(frames, batchSize: 1);
    }

    /// <summary>
    /// Stacks magnitude spectrograms from multiple equal-length signals along the batch axis.
    /// </summary>
    public static BatchTensor<float> FromSignals(IReadOnlyList<Signal> signals, Stft stft, bool normalize = true)
    {
        ArgumentNullException.ThrowIfNull(signals);
        ArgumentNullException.ThrowIfNull(stft);

        if (signals.Count == 0)
        {
            throw new ArgumentException("At least one signal is required.", nameof(signals));
        }

        ValidateUniformSignals(signals);

        var reference = stft.Spectrogram(signals[0], normalize);
        var batch = FromSpectrogramFrames(reference, signals.Count);

        for (var n = 0; n < signals.Count; n++)
        {
            var frames = stft.Spectrogram(signals[n], normalize);
            CopyFramesIntoBatch(batch, frames, n);
        }

        return batch;
    }

    /// <summary>
    /// Packs STFT magnitude frames into N×T×F×1.
    /// </summary>
    public static BatchTensor<float> FromSpectrogramFrames(IReadOnlyList<float[]> frames, int batchSize = 1)
    {
        ArgumentNullException.ThrowIfNull(frames);
        if (frames.Count == 0)
        {
            throw new ArgumentException("At least one frame is required.", nameof(frames));
        }

        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        var freqBins = frames[0].Length;
        var batch = new BatchTensor<float>(batchSize, frames.Count, freqBins, 1);
        CopyFramesIntoBatch(batch, frames, batchIndex: 0);
        return batch;
    }

    private static void CopyFramesIntoBatch(BatchTensor<float> batch, IReadOnlyList<float[]> frames, int batchIndex)
    {
        if (batchIndex < 0 || batchIndex >= batch.Batch)
        {
            throw new ArgumentOutOfRangeException(nameof(batchIndex));
        }

        if (frames.Count != batch.Height)
        {
            throw new ArgumentException("Frame count does not match batch height.", nameof(frames));
        }

        var freqBins = batch.Width;
        for (var t = 0; t < frames.Count; t++)
        {
            var frame = frames[t];
            if (frame.Length != freqBins)
            {
                throw new ArgumentException("All spectrogram frames must have the same length.", nameof(frames));
            }

            for (var f = 0; f < freqBins; f++)
            {
                batch[batchIndex, t, f, 0] = frame[f];
            }
        }
    }

    private static void ValidateUniformSignals(IReadOnlyList<Signal> signals)
    {
        var length = signals[0].Length;
        var rate = signals[0].SamplingRate;

        for (var i = 1; i < signals.Count; i++)
        {
            if (signals[i].Length != length)
            {
                throw new ArgumentException("All signals must have the same length for batched spectrograms.", nameof(signals));
            }

            if (MathF.Abs(signals[i].SamplingRate - rate) > 1e-3f)
            {
                throw new ArgumentException("All signals must share the same sampling rate.", nameof(signals));
            }
        }
    }
}
