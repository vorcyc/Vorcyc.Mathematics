using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DeepLearning_example;

internal static class AudioClassifyDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 音频 MFCC 分类 (AudioTrainingSamples + FitBatchSequential) ---");

        const float rate = 8000f;
        const int length = 1024;
        const int featureCount = 13;

        var lowTone = MakeSine(length, rate, 180f);
        var highTone = MakeSine(length, rate, 1400f);
        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor((int)rate, featureCount);

        var sample = AudioTrainingSamples.FeatureMeanClassification(
            [lowTone, highTone],
            classIndices: [0, 1],
            numClasses: 2,
            extractor: mfcc);

        Console.WriteLine($"批输入形状: N={sample.Input.Batch}, F={sample.Input.Width}");
        Console.WriteLine($"特征: 低频正弦 → 类 0, 高频正弦 → 类 1");

        var model = new BatchSequential<float>(
            new BatchFullyConnectedLayer<float>(featureCount, 16),
            new BatchReLUActivation<float>(),
            new BatchFullyConnectedLayer<float>(16, 2));

        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(
            model,
            new BatchCategoricalCrossEntropyLoss<float>(),
            new AdamOptimizer<float>(0.05f),
            [sample],
            epochs: 2000,
            computingContext: ComputingContext.Parallel,
            onEpochEnd: (epoch, loss) =>
            {
                if (epoch is 1000 or 2000)
                {
                    Console.WriteLine($"  epoch {epoch,4}: loss = {float.CreateTruncating(loss):F4}");
                }
            });

        var logits = model.Forward(sample.Input, training: false);
        int predLow = logits[0, 0, 0, 0] > logits[0, 0, 0, 1] ? 0 : 1;
        int predHigh = logits[1, 0, 0, 0] > logits[1, 0, 0, 1] ? 0 : 1;
        Console.WriteLine($"低频预测类: {predLow} (期望 0), 高频预测类: {predHigh} (期望 1)");
        return 0;
    }

    private static Signal MakeSine(int length, float rate, float frequency)
    {
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, frequency, Behaviour.Replace);
        return signal;
    }
}
