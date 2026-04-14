using System.Numerics;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class SignalNeuralNetworkFitter
{
    internal static SignalFitResult<T> Fit<T>(
        IReadOnlyList<Signal> signals,
        Span<T> yData,
        SignalNeuralNetworkOptions? signalOptions,
        int epochs,
        int hiddenNodes,
        T? learningRate,
        TrainingProgressHandler<T>? trainingProgressCallback,
        NeuralNetworkTrainingOptions? trainingOptions)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(signals);
        if (signals.Count == 0)
        {
            throw new ArgumentException("At least one signal is required.", nameof(signals));
        }

        if (signals.Count != yData.Length)
        {
            throw new ArgumentException("signals and yData must have the same length.");
        }

        var options = signalOptions ?? new SignalNeuralNetworkOptions();
        var featureRows = BuildFeatureRows(signals, options);
        var xData = new DataRow<T>[featureRows.Length];

        for (var i = 0; i < featureRows.Length; i++)
        {
            xData[i] = new DataRow<T>(Array.ConvertAll(featureRows[i], v => T.CreateTruncating(v)));
        }

        var multi = NeuralNetworkFitter.Fit_MultiColumn(
            xData,
            yData,
            epochs,
            hiddenNodes,
            learningRate,
            trainingProgressCallback,
            trainingOptions);

        return new SignalFitResult<T>(
            signal =>
            {
                var vector = ExtractFeatureVector(signal, options);
                var row = new DataRow<T>(Array.ConvertAll(vector, v => T.CreateTruncating(v)));
                return multi.Predict!(row);
            },
            multi.Parameters,
            multi.MeanSquaredError);
    }

    private static float[][] BuildFeatureRows(IReadOnlyList<Signal> signals, SignalNeuralNetworkOptions options)
    {
        var rows = new float[signals.Count][];
        var featureLength = -1;

        for (var i = 0; i < signals.Count; i++)
        {
            var vector = ExtractFeatureVector(signals[i], options);
            if (featureLength < 0)
            {
                featureLength = vector.Length;
            }
            else if (vector.Length != featureLength)
            {
                throw new ArgumentException("All signals must produce feature vectors of the same length.");
            }

            rows[i] = vector;
        }

        return rows;
    }

    private static float[] ExtractFeatureVector(Signal signal, SignalNeuralNetworkOptions options)
    {
        var batch = options.FeatureMode switch
        {
            SignalNeuralNetworkFeatureMode.Waveform => BatchTensorSignalExtensions.FromSignal(signal),
            SignalNeuralNetworkFeatureMode.Periodogram => FeatureBatchBuilder.FromPeriodogram(
                signal,
                options.Stft ?? throw new ArgumentException("Stft is required for periodogram features.")),
            SignalNeuralNetworkFeatureMode.FeatureMean => FeatureBatchBuilder.FromExtractorMean(
                signal,
                options.FeatureExtractor ?? throw new ArgumentException("FeatureExtractor is required for feature-mean mode.")),
            _ => throw new ArgumentOutOfRangeException(nameof(options))
        };

        if (batch.Height != 1 || batch.Width != 1)
        {
            throw new InvalidOperationException("Expected vector layout N×1×1×F.");
        }

        var features = new float[batch.Channels];
        for (var c = 0; c < batch.Channels; c++)
        {
            features[c] = batch[0, 0, 0, c];
        }

        return features;
    }
}
