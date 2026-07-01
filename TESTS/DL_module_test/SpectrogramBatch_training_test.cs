using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace DL_module_test;

internal static class SpectrogramBatch_training_test
{
    public static bool Run()
    {
        const float rate = 8000f;
        const int length = 256;
        const int window = 64;
        const int hop = 32;

        var lowTone = MakeSine(length, rate, 200f);
        var highTone = MakeSine(length, rate, 1200f);
        var stft = new Stft(windowSize: window, hopSize: hop);

        var sample = AudioTrainingSamples.SpectrogramClassification(
            [lowTone, highTone],
            classIndices: [0, 1],
            numClasses: 2,
            stft);

        var input = sample.Input;
        var model = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 8, kernelSize: 3, name: "conv"),
            new BatchReLUActivation<float>(),
            new BatchGlobalAveragePool2DLayer<float>(),
            new BatchFullyConnectedLayer<float>(8, 2, name: "fc"));

        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(
            model,
            new BatchCategoricalCrossEntropyLoss<float>(),
            new AdamOptimizer<float>(0.02f),
            [sample],
            epochs: 4000);

        var logits = model.Forward(input, training: false);
        return logits[0, 0, 0, 0] > logits[0, 0, 0, 1]
            && logits[1, 0, 0, 1] > logits[1, 0, 0, 0];
    }

    private static Signal MakeSine(int length, float rate, float frequency)
    {
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, frequency, Behaviour.Replace);
        return signal;
    }
}
