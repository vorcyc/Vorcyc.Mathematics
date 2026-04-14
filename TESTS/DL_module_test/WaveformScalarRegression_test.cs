using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DL_module_test;

internal static class WaveformScalarRegression_test
{
    public static bool Run()
    {
        const float rate = 8000f;
        const int length = 128;
        float[] amplitudes = [0.25f, 0.5f, 0.75f, 1.0f];

        var signals = new Signal[amplitudes.Length];
        for (var i = 0; i < amplitudes.Length; i++)
        {
            signals[i] = MakeScaledSine(length, rate, 400f, amplitudes[i]);
        }

        var sample = AudioTrainingSamples.WaveformRegression(signals, [.. amplitudes]);

        var model = new BatchSequential<float>(
            new BatchFlattenLayer<float>(),
            new BatchFullyConnectedLayer<float>(length, 12, name: "fc1"),
            new BatchReLUActivation<float>(),
            new BatchFullyConnectedLayer<float>(12, 1, name: "fc2"));

        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(
            model,
            new BatchMeanSquaredErrorLoss<float>(),
            new AdamOptimizer<float>(0.03f),
            [sample],
            epochs: 5000);

        var probe = MakeScaledSine(length, rate, 400f, 0.6f);
        var probeBatch = BatchTensorSignalExtensions.FromSignal(probe);
        var prediction = model.Forward(probeBatch, training: false)[0, 0, 0, 0];

        return MathF.Abs(prediction - 0.6f) < 0.15f;
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
