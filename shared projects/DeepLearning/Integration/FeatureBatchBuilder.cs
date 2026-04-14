using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.DeepLearning.Integration;

/// <summary>
/// Packs DSP feature extractor outputs into <see cref="BatchTensor{float}"/> layouts.
/// </summary>
public static class FeatureBatchBuilder
{
    /// <summary>
    /// Welch-style averaged periodogram as 1×1×1×F.
    /// </summary>
    public static BatchTensor<float> FromPeriodogram(Signal signal, Stft stft)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(stft);

        var spectrum = stft.AveragePeriodogram(signal);
        return FromVectors([spectrum]);
    }

    /// <summary>
    /// Stacks periodogram vectors into N×1×1×F.
    /// </summary>
    public static BatchTensor<float> FromPeriodograms(IReadOnlyList<Signal> signals, Stft stft)
    {
        ArgumentNullException.ThrowIfNull(signals);
        ArgumentNullException.ThrowIfNull(stft);

        if (signals.Count == 0)
        {
            throw new ArgumentException("At least one signal is required.", nameof(signals));
        }

        ValidateUniformSignals(signals);

        var vectors = new float[signals.Count][];
        for (var n = 0; n < signals.Count; n++)
        {
            vectors[n] = stft.AveragePeriodogram(signals[n]);
        }

        return FromVectors(vectors);
    }

    /// <summary>
    /// Packs per-frame feature rows into 1×T×F×1.
    /// </summary>
    public static BatchTensor<float> FromExtractor(Signal signal, FeatureExtractor extractor)
        => FromExtractors([signal], extractor);

    /// <summary>
    /// Stacks frame features from multiple signals into N×T×F×1.
    /// </summary>
    public static BatchTensor<float> FromExtractors(IReadOnlyList<Signal> signals, FeatureExtractor extractor)
    {
        ArgumentNullException.ThrowIfNull(signals);
        ArgumentNullException.ThrowIfNull(extractor);

        if (signals.Count == 0)
        {
            throw new ArgumentException("At least one signal is required.", nameof(signals));
        }

        ValidateUniformSignals(signals);

        var framesPerSignal = new List<float[]>[signals.Count];
        for (var n = 0; n < signals.Count; n++)
        {
            framesPerSignal[n] = extractor.ComputeFrom(signals[n]);
        }

        return FromFrameSequences(framesPerSignal);
    }

    /// <summary>
    /// Time-averages frame features into 1×1×1×F.
    /// </summary>
    public static BatchTensor<float> FromExtractorMean(Signal signal, FeatureExtractor extractor)
        => FromExtractorMeans([signal], extractor);

    /// <summary>
    /// Time-averages frame features per signal into N×1×1×F.
    /// </summary>
    public static BatchTensor<float> FromExtractorMeans(IReadOnlyList<Signal> signals, FeatureExtractor extractor)
    {
        ArgumentNullException.ThrowIfNull(signals);
        ArgumentNullException.ThrowIfNull(extractor);

        if (signals.Count == 0)
        {
            throw new ArgumentException("At least one signal is required.", nameof(signals));
        }

        ValidateUniformSignals(signals);

        var vectors = new float[signals.Count][];
        for (var n = 0; n < signals.Count; n++)
        {
            vectors[n] = MeanFrameFeatures(extractor.ComputeFrom(signals[n]));
        }

        return FromVectors(vectors);
    }

    /// <summary>
    /// Packs equal-length feature vectors into N×1×1×F.
    /// </summary>
    public static BatchTensor<float> FromVectors(IReadOnlyList<float[]> vectors)
    {
        ArgumentNullException.ThrowIfNull(vectors);
        if (vectors.Count == 0)
        {
            throw new ArgumentException("At least one vector is required.", nameof(vectors));
        }

        var featureCount = vectors[0].Length;
        var batch = new BatchTensor<float>(vectors.Count, 1, 1, featureCount);

        for (var n = 0; n < vectors.Count; n++)
        {
            var vector = vectors[n];
            if (vector.Length != featureCount)
            {
                throw new ArgumentException("All feature vectors must have the same length.", nameof(vectors));
            }

            for (var f = 0; f < featureCount; f++)
            {
                batch[n, 0, 0, f] = vector[f];
            }
        }

        return batch;
    }

    /// <summary>
    /// Packs frame rows into N×T×F×1.
    /// </summary>
    public static BatchTensor<float> FromFrameSequences(IReadOnlyList<IReadOnlyList<float[]>> frameSequences)
    {
        ArgumentNullException.ThrowIfNull(frameSequences);
        if (frameSequences.Count == 0)
        {
            throw new ArgumentException("At least one frame sequence is required.", nameof(frameSequences));
        }

        var frameCount = frameSequences[0].Count;
        var featureCount = frameSequences[0][0].Length;
        var batch = new BatchTensor<float>(frameSequences.Count, frameCount, featureCount, 1);

        for (var n = 0; n < frameSequences.Count; n++)
        {
            CopyFramesIntoBatch(batch, frameSequences[n], n, featureCount);
        }

        return batch;
    }

    /// <summary>
    /// Packs one frame sequence into 1×T×F×1 (alias for spectrogram-style layouts).
    /// </summary>
    public static BatchTensor<float> FromFeatureFrames(IReadOnlyList<float[]> frames)
        => SpectrogramBatchBuilder.FromSpectrogramFrames(frames, batchSize: 1);

    private static float[] MeanFrameFeatures(IReadOnlyList<float[]> frames)
    {
        if (frames.Count == 0)
        {
            throw new ArgumentException("Extractor returned no frames.");
        }

        var featureCount = frames[0].Length;
        var mean = new float[featureCount];

        for (var t = 0; t < frames.Count; t++)
        {
            var frame = frames[t];
            if (frame.Length != featureCount)
            {
                throw new InvalidOperationException("Inconsistent feature vector length.");
            }

            for (var f = 0; f < featureCount; f++)
            {
                mean[f] += frame[f];
            }
        }

        var inv = 1f / frames.Count;
        for (var f = 0; f < featureCount; f++)
        {
            mean[f] *= inv;
        }

        return mean;
    }

    private static void CopyFramesIntoBatch(
        BatchTensor<float> batch,
        IReadOnlyList<float[]> frames,
        int batchIndex,
        int featureCount)
    {
        if (frames.Count != batch.Height)
        {
            throw new ArgumentException("Frame count does not match batch height.");
        }

        for (var t = 0; t < frames.Count; t++)
        {
            var frame = frames[t];
            if (frame.Length != featureCount)
            {
                throw new ArgumentException("All frames must have the same feature length.");
            }

            for (var f = 0; f < featureCount; f++)
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
                throw new ArgumentException("All signals must have the same length.", nameof(signals));
            }

            if (MathF.Abs(signals[i].SamplingRate - rate) > 1e-3f)
            {
                throw new ArgumentException("All signals must share the same sampling rate.", nameof(signals));
            }
        }
    }
}
