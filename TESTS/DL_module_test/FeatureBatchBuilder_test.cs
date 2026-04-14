using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace DL_module_test;

internal static class FeatureBatchBuilder_test
{
    public static bool Run()
    {
        if (!PeriodogramVectorLayout()) return false;
        if (!MfccSequenceLayout()) return false;
        if (!MfccMeanLayout()) return false;
        if (!TimeDomainMeanLayout()) return false;
        return true;
    }

    private static Signal MakeSine(int length, float rate, float frequency)
    {
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, frequency, Behaviour.Replace);
        return signal;
    }

    private static bool PeriodogramVectorLayout()
    {
        const float rate = 8000f;
        var low = MakeSine(512, rate, 200f);
        var high = MakeSine(512, rate, 1500f);
        var stft = new Stft(windowSize: 64, hopSize: 32);

        var batch = FeatureBatchBuilder.FromPeriodograms([low, high], stft);
        if (batch.Batch != 2 || batch.Height != 1 || batch.Width != 1)
        {
            return false;
        }

        var single = FeatureBatchBuilder.FromPeriodogram(low, stft);
        return batch.Channels == single.Channels && batch.Channels > 0;
    }

    private static bool MfccSequenceLayout()
    {
        const float rate = 8000f;
        var signal = MakeSine(1024, rate, 440f);
        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor((int)rate);

        var batch = FeatureBatchBuilder.FromExtractor(signal, mfcc);
        return batch.Batch == 1
            && batch.Height > 0
            && batch.Width == mfcc.FeatureCount
            && batch.Channels == 1;
    }

    private static bool MfccMeanLayout()
    {
        const float rate = 8000f;
        var signal = MakeSine(1024, rate, 440f);
        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor((int)rate);

        var batch = FeatureBatchBuilder.FromExtractorMean(signal, mfcc);
        return batch.Batch == 1
            && batch.Height == 1
            && batch.Width == 1
            && batch.Channels == mfcc.FeatureCount;
    }

    private static bool TimeDomainMeanLayout()
    {
        const float rate = 8000f;
        var signal = MakeSine(1024, rate, 440f);
        var extractor = AudioTrainingSamples.CreateDefaultTimeDomainExtractor((int)rate);

        var batch = FeatureBatchBuilder.FromExtractorMean(signal, extractor);
        return batch.Batch == 1
            && batch.Channels == extractor.FeatureCount
            && batch.Channels == 4;
    }
}
