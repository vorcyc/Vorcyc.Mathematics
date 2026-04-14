using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Base;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.DeepLearning.Integration;

/// <summary>
/// Builds <see cref="BatchLabelSample{float}"/> instances from DSP <see cref="Signal"/> inputs.
/// </summary>
public static class AudioTrainingSamples
{
    /// <summary>
    /// Packs scalar regression targets into N×1×1×1.
    /// </summary>
    public static BatchTensor<float> CreateScalarTargets(IReadOnlyList<float> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var targets = new BatchTensor<float>(values.Count, 1, 1, 1);
        for (var n = 0; n < values.Count; n++)
        {
            targets[n, 0, 0, 0] = values[n];
        }

        return targets;
    }

    /// <summary>
    /// Builds a waveform → scalar regression sample (N×1×1×L input, N×1×1×1 target).
    /// </summary>
    public static BatchLabelSample<float> WaveformRegression(
        IReadOnlyList<Signal> signals,
        IReadOnlyList<float> targets)
    {
        if (signals.Count != targets.Count)
        {
            throw new ArgumentException("Signals and targets must have the same count.");
        }

        var input = BatchTensorSignalExtensions.FromSignalVectors(signals);
        var target = CreateScalarTargets(targets);
        return new BatchLabelSample<float>(input, target);
    }

    /// <summary>
    /// Creates one-hot classification targets with shape N×1×1×C.
    /// </summary>
    public static BatchTensor<float> CreateOneHotTargets(IReadOnlyList<int> classIndices, int numClasses)
    {
        ArgumentNullException.ThrowIfNull(classIndices);
        if (numClasses <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numClasses));
        }

        var targets = new BatchTensor<float>(classIndices.Count, 1, 1, numClasses);
        for (var n = 0; n < classIndices.Count; n++)
        {
            var label = classIndices[n];
            if (label < 0 || label >= numClasses)
            {
                throw new ArgumentOutOfRangeException(nameof(classIndices), "Class index is out of range.");
            }

            targets[n, 0, 0, label] = 1f;
        }

        return targets;
    }

    /// <summary>
    /// Builds a labeled batch from magnitude spectrograms and class indices.
    /// </summary>
    public static BatchLabelSample<float> SpectrogramClassification(
        IReadOnlyList<Signal> signals,
        IReadOnlyList<int> classIndices,
        int numClasses,
        Stft stft,
        bool normalize = true)
    {
        if (signals.Count != classIndices.Count)
        {
            throw new ArgumentException("Signals and labels must have the same count.");
        }

        var input = SpectrogramBatchBuilder.FromSignals(signals, stft, normalize);
        var target = CreateOneHotTargets(classIndices, numClasses);
        return new BatchLabelSample<float>(input, target);
    }

    /// <summary>
    /// Builds a labeled batch from raw waveform vectors (N×1×1×L).
    /// </summary>
    public static BatchLabelSample<float> WaveformClassification(
        IReadOnlyList<Signal> signals,
        IReadOnlyList<int> classIndices,
        int numClasses)
    {
        if (signals.Count != classIndices.Count)
        {
            throw new ArgumentException("Signals and labels must have the same count.");
        }

        var input = BatchTensorSignalExtensions.FromSignalVectors(signals);
        var target = CreateOneHotTargets(classIndices, numClasses);
        return new BatchLabelSample<float>(input, target);
    }

    /// <summary>
    /// Builds a labeled batch from averaged periodogram vectors (N×1×1×F).
    /// </summary>
    public static BatchLabelSample<float> PeriodogramClassification(
        IReadOnlyList<Signal> signals,
        IReadOnlyList<int> classIndices,
        int numClasses,
        Stft stft)
    {
        if (signals.Count != classIndices.Count)
        {
            throw new ArgumentException("Signals and labels must have the same count.");
        }

        var input = FeatureBatchBuilder.FromPeriodograms(signals, stft);
        var target = CreateOneHotTargets(classIndices, numClasses);
        return new BatchLabelSample<float>(input, target);
    }

    /// <summary>
    /// Builds a labeled batch from time-averaged frame features (N×1×1×F).
    /// </summary>
    public static BatchLabelSample<float> FeatureMeanClassification(
        IReadOnlyList<Signal> signals,
        IReadOnlyList<int> classIndices,
        int numClasses,
        FeatureExtractor extractor)
    {
        if (signals.Count != classIndices.Count)
        {
            throw new ArgumentException("Signals and labels must have the same count.");
        }

        var input = FeatureBatchBuilder.FromExtractorMeans(signals, extractor);
        var target = CreateOneHotTargets(classIndices, numClasses);
        return new BatchLabelSample<float>(input, target);
    }

    /// <summary>
    /// Builds a labeled batch from per-frame MFCC features (N×T×F×1).
    /// </summary>
    public static BatchLabelSample<float> MfccSequenceClassification(
        IReadOnlyList<Signal> signals,
        IReadOnlyList<int> classIndices,
        int numClasses,
        MfccExtractor mfcc)
        => FeatureSequenceClassification(signals, classIndices, numClasses, mfcc);

    /// <summary>
    /// Builds a labeled batch from per-frame extractor features (N×T×F×1).
    /// </summary>
    public static BatchLabelSample<float> FeatureSequenceClassification(
        IReadOnlyList<Signal> signals,
        IReadOnlyList<int> classIndices,
        int numClasses,
        FeatureExtractor extractor)
    {
        if (signals.Count != classIndices.Count)
        {
            throw new ArgumentException("Signals and labels must have the same count.");
        }

        var input = FeatureBatchBuilder.FromExtractors(signals, extractor);
        var target = CreateOneHotTargets(classIndices, numClasses);
        return new BatchLabelSample<float>(input, target);
    }

    /// <summary>
    /// Creates a compact MFCC extractor for 8 kHz integration tests and small models.
    /// </summary>
    public static MfccExtractor CreateDefaultMfccExtractor(int samplingRate, int featureCount = 13)
    {
        return new MfccExtractor(new MfccOptions
        {
            SamplingRate = samplingRate,
            FrameSize = 256,
            HopSize = 128,
            FeatureCount = featureCount,
            FilterBankSize = 24
        });
    }

    /// <summary>
    /// Creates a time-domain feature extractor (energy, rms, zcr, entropy).
    /// </summary>
    public static TimeDomainFeaturesExtractor CreateDefaultTimeDomainExtractor(int samplingRate)
    {
        return new TimeDomainFeaturesExtractor(new MultiFeatureOptions
        {
            SamplingRate = samplingRate,
            FrameSize = 256,
            HopSize = 128,
            FeatureList = TimeDomainFeaturesExtractor.FeatureSet
        });
    }
}
