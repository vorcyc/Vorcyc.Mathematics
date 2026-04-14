using Audio_example.Io;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Serialization;
using Vorcyc.Mathematics.DeepLearning.Training;
namespace Audio_example.Pipeline;

internal static class PitchClassifierTrainer
{
    public static BatchSequential<float> CreateModel()
        => new(
            new BatchFullyConnectedLayer<float>(PitchClassifierConfig.FeatureCount, PitchClassifierConfig.HiddenUnits, "fc1"),
            new BatchReLUActivation<float>(),
            new BatchFullyConnectedLayer<float>(PitchClassifierConfig.HiddenUnits, PitchClassifierConfig.NumClasses, "fc2"));

    public static int Train(string dataRoot, string modelPath, int epochs)
    {
        var dataset = LabeledWavDataset.LoadFromFolders(dataRoot);
        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor(
            PitchClassifierConfig.TargetSampleRate,
            PitchClassifierConfig.FeatureCount);

        var sample = AudioTrainingSamples.FeatureMeanClassification(
            dataset.Signals,
            dataset.Labels,
            PitchClassifierConfig.NumClasses,
            mfcc);

        Console.WriteLine($"--- 训练 MFCC 音高分类器 ---");
        Console.WriteLine($"样本: {dataset.Signals.Count} 个 WAV (NAudio 读取 → MFCC 均值特征)");
        foreach (var path in dataset.FilePaths)
        {
            Console.WriteLine($"  {path}");
        }

        var model = CreateModel();
        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(
            model,
            new BatchCategoricalCrossEntropyLoss<float>(),
            new AdamOptimizer<float>(0.05f),
            [sample],
            epochs: epochs,
            computingContext: ComputingContext.Parallel,
            onEpochEnd: (epoch, loss) =>
            {
                if (epoch == epochs || epoch % Math.Max(1, epochs / 4) == 0)
                {
                    Console.WriteLine($"  epoch {epoch,4}: loss = {float.CreateTruncating(loss):F4}");
                }
            });

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(modelPath))!);
        ModelSerializer.SaveToFile(model, modelPath);
        Console.WriteLine($"模型已保存: {Path.GetFullPath(modelPath)}");

        int correct = 0;
        var logits = model.Forward(sample.Input, training: false);
        for (int n = 0; n < dataset.Labels.Count; n++)
        {
            int predicted = logits[n, 0, 0, 0] > logits[n, 0, 0, 1] ? 0 : 1;
            if (predicted == dataset.Labels[n])
            {
                correct++;
            }
        }

        Console.WriteLine($"训练集准确率: {correct}/{dataset.Labels.Count}");
        return correct == dataset.Labels.Count ? 0 : 1;
    }

    public static int Predict(string wavPath, string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException(
                $"Model not found: {modelPath}. Run `train` first or specify --model.",
                modelPath);
        }

        using var signal = WavBridge.ReadAsSignal(wavPath, PitchClassifierConfig.TargetSampleRate);
        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor(
            PitchClassifierConfig.TargetSampleRate,
            PitchClassifierConfig.FeatureCount);

        var input = FeatureBatchBuilder.FromExtractorMean(signal, mfcc);
        var model = CreateModel();
        ModelSerializer.LoadFromFile(model, modelPath);

        var logits = model.Forward(input, training: false);
        int classIndex = logits[0, 0, 0, 0] > logits[0, 0, 0, 1] ? 0 : 1;
        float confidence = SoftmaxConfidence(logits, classIndex);

        var spectrum = signal.TransformToFrequencyDomain();
        Console.WriteLine($"--- 推理 ---");
        Console.WriteLine($"文件: {Path.GetFullPath(wavPath)}");
        Console.WriteLine($"FFT 主频: {spectrum.Frequency:F1} Hz");
        Console.WriteLine($"预测类别: {PitchClassifierConfig.ClassNames[classIndex]} (置信度 {confidence:P1})");
        return 0;
    }

    static float SoftmaxConfidence(BatchTensor<float> logits, int classIndex)
    {
        float max = float.NegativeInfinity;
        for (int c = 0; c < PitchClassifierConfig.NumClasses; c++)
        {
            max = MathF.Max(max, logits[0, 0, 0, c]);
        }

        float sum = 0f;
        for (int c = 0; c < PitchClassifierConfig.NumClasses; c++)
        {
            sum += MathF.Exp(logits[0, 0, 0, c] - max);
        }

        return MathF.Exp(logits[0, 0, 0, classIndex] - max) / sum;
    }
}
