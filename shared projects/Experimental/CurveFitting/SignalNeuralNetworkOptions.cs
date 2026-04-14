using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Base;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

/// <summary>
/// Feature extraction mode for <see cref="CurveFitter{T}.NeuralNetwork(Signal, Span{T}, ...)"/>.
/// </summary>
public enum SignalNeuralNetworkFeatureMode
{
    /// <summary>Raw samples packed as N×1×1×L.</summary>
    Waveform,

    /// <summary>Welch-style averaged periodogram (N×1×1×F).</summary>
    Periodogram,

    /// <summary>Time-averaged frame features from a <see cref="FeatureExtractor"/>.</summary>
    FeatureMean
}

/// <summary>
/// DSP feature options for signal-based neural network regression.
/// </summary>
public sealed class SignalNeuralNetworkOptions
{
    /// <summary>Gets or sets how each <see cref="Signal"/> is encoded.</summary>
    public SignalNeuralNetworkFeatureMode FeatureMode { get; init; } = SignalNeuralNetworkFeatureMode.Periodogram;

    /// <summary>STFT used when <see cref="FeatureMode"/> is <see cref="SignalNeuralNetworkFeatureMode.Periodogram"/>.</summary>
    public Stft? Stft { get; init; }

    /// <summary>Extractor used when <see cref="FeatureMode"/> is <see cref="SignalNeuralNetworkFeatureMode.FeatureMean"/>.</summary>
    public FeatureExtractor? FeatureExtractor { get; init; }
}
