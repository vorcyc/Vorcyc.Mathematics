using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DL_module_test;

internal static class MfccMean_training_test
{
    public static bool Run()
    {
        const float rate = 8000f;
        const int length = 1024;
        const int featureCount = 13;

        var low = MakeSine(length, rate, 180f);
        var high = MakeSine(length, rate, 1400f);
        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor((int)rate, featureCount);

        var sample = AudioTrainingSamples.FeatureMeanClassification(
            [low, high],
            classIndices: [0, 1],
            numClasses: 2,
            extractor: mfcc);

        var model = new BatchSequential<float>(
            new BatchFullyConnectedLayer<float>(featureCount, 16, name: "fc1"),
            new BatchReLUActivation<float>(),
            new BatchFullyConnectedLayer<float>(16, 2, name: "fc2"));

        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(
            model,
            new BatchCategoricalCrossEntropyLoss<float>(),
            new AdamOptimizer<float>(0.05f),
            [sample],
            epochs: 3000);

        var logits = model.Forward(sample.Input, training: false);
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
