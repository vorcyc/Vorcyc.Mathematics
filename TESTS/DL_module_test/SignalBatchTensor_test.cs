using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DL_module_test;

internal static class SignalBatchTensor_test
{
    public static bool Run()
    {
        if (!VectorRoundTrip()) return false;
        if (!BatchVectorsPreserveOrder()) return false;
        if (!FramesLayout()) return false;
        return true;
    }

    private static Signal MakeSine(int length, float rate, float frequency)
    {
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, frequency, Behaviour.Replace);
        return signal;
    }

    private static bool VectorRoundTrip()
    {
        const float rate = 8000f;
        var original = MakeSine(64, rate, 440f);
        var batch = BatchTensorSignalExtensions.FromSignal(original);
        var restored = batch.ToSignal(0, rate);

        if (restored.Length != original.Length)
        {
            return false;
        }

        for (var i = 0; i < original.Length; i++)
        {
            if (MathF.Abs(original[i] - restored[i]) > 1e-6f)
            {
                return false;
            }
        }

        return true;
    }

    private static bool BatchVectorsPreserveOrder()
    {
        const float rate = 8000f;
        var low = MakeSine(32, rate, 100f);
        var high = MakeSine(32, rate, 900f);
        var batch = BatchTensorSignalExtensions.FromSignalVectors([low, high]);

        if (batch.Batch != 2 || batch.Height != 1 || batch.Width != 1 || batch.Channels != 32)
        {
            return false;
        }

        return NearlyEqual(batch.ToSignal(0, rate)[0], low[0])
            && NearlyEqual(batch.ToSignal(1, rate)[0], high[0]);
    }

    private static bool FramesLayout()
    {
        const float rate = 8000f;
        var signal = MakeSine(128, rate, 200f);
        const int frameSize = 32;
        const int hopSize = 16;
        var batch = BatchTensorSignalExtensions.FromSignalFrames(signal, frameSize, hopSize);

        var expectedFrames = 1 + (signal.Length - frameSize) / hopSize;
        if (batch.Batch != 1 || batch.Height != expectedFrames || batch.Width != frameSize || batch.Channels != 1)
        {
            return false;
        }

        return NearlyEqual(batch[0, 0, 0, 0], signal[0])
            && NearlyEqual(batch[0, 1, 0, 0], signal[hopSize]);
    }

    private static bool NearlyEqual(float a, float b) => MathF.Abs(a - b) < 1e-6f;
}
