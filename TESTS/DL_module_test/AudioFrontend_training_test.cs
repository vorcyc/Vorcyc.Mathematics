using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Integration.Frontends;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DL_module_test;

internal static class AudioFrontend_training_test
{
    public static bool Run()
    {
        const float rate = 8000f;
        const int length = 256;
        const int window = 64;
        const int hop = 32;
        const int melBands = 16;

        var low = MakeSine(length, rate, 200f);
        var high = MakeSine(length, rate, 1200f);
        var input = BatchTensorSignalExtensions.FromSignalVectors([low, high]);
        var targets = AudioTrainingSamples.CreateOneHotTargets([0, 1], numClasses: 2);

        var frontend = AudioFrontendLayers.CreateMelSpectrogramStack(
            (int)rate,
            windowSize: window,
            hopSize: hop,
            melBands: melBands);

        var model = new BatchSequential<float>(
            frontend[0],
            frontend[1],
            frontend[2],
            new BatchConvolution2DLayer<float>(1, 8, kernelSize: 3, name: "conv"),
            new BatchReLUActivation<float>(),
            new BatchGlobalAveragePool2DLayer<float>(),
            new BatchFullyConnectedLayer<float>(8, 2, name: "fc"));

        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(
            model,
            new BatchCategoricalCrossEntropyLoss<float>(),
            new AdamOptimizer<float>(0.02f),
            [new BatchLabelSample<float>(input, targets)],
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
