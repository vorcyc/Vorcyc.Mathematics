using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.Experimental.CurveFitting;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace DL_module_test;

internal static class CurveFitterSignalNeuralNetwork_test
{
    public static bool Run()
    {
        const float rate = 8000f;
        const int length = 512;
        float[] amplitudes = [0.3f, 0.45f, 0.6f, 0.75f, 0.9f, 1.0f];
        var signals = new Signal[amplitudes.Length];

        for (var i = 0; i < amplitudes.Length; i++)
        {
            signals[i] = MakeScaledSine(length, rate, 440f, amplitudes[i]);
        }

        var stft = new Stft(windowSize: 64, hopSize: 32);
        var options = new SignalNeuralNetworkOptions
        {
            FeatureMode = SignalNeuralNetworkFeatureMode.Periodogram,
            Stft = stft
        };

        var trainingOptions = new NeuralNetworkTrainingOptions
        {
            RandomSeed = 42,
            OptimizerKind = MlpOptimizerKind.Adam,
            InitialLearningRate = 0.05,
            SchedulerKind = NeuralNetworkSchedulerKind.CosineAnnealing,
            MinimumLearningRate = 0.001
        };

        var result = CurveFitter<float>.NeuralNetwork(
            signals,
            amplitudes,
            signalOptions: options,
            epochs: 6000,
            hiddenNodes: 20,
            trainingOptions: trainingOptions);

        const float probeAmplitude = 0.55f;
        var probe = MakeScaledSine(length, rate, 440f, probeAmplitude);
        var predicted = float.CreateTruncating(result.Predict(probe));
        var mse = float.CreateTruncating(result.MeanSquaredError);

        return mse < 0.02f && MathF.Abs(predicted - probeAmplitude) < 0.12f;
    }

    private static Signal MakeScaledSine(int length, float rate, float frequency, float amplitude)
    {
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, frequency, Behaviour.Replace);
        for (var i = 0; i < signal.Length; i++)
        {
            signal[i] *= amplitude;
        }

        return signal;
    }
}
