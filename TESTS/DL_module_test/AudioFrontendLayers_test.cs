using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Integration.Frontends;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DL_module_test;

internal static class AudioFrontendLayers_test
{
    public static bool Run()
    {
        if (!PreEmphasisPreservesShape()) return false;
        if (!MelStackOutputShape()) return false;
        return true;
    }

    private static Signal MakeSine(int length, float rate, float frequency)
    {
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, frequency, Behaviour.Replace);
        return signal;
    }

    private static bool PreEmphasisPreservesShape()
    {
        const float rate = 8000f;
        var input = BatchTensorSignalExtensions.FromSignal(MakeSine(128, rate, 300f));
        var layer = new BatchPreEmphasisLayer(0.97f);
        var output = layer.Forward(input);

        return output.Batch == input.Batch
            && output.Height == 1
            && output.Width == 1
            && output.Channels == input.Channels;
    }

    private static bool MelStackOutputShape()
    {
        const float rate = 8000f;
        const int length = 256;
        const int window = 64;
        const int hop = 32;
        const int melBands = 12;

        var input = BatchTensorSignalExtensions.FromSignal(MakeSine(length, rate, 500f));
        var stack = AudioFrontendLayers.CreateMelSpectrogramStack((int)rate, window, hop, melBands);

        var tensor = input;
        foreach (var layer in stack)
        {
            tensor = layer.Forward(tensor);
        }

        return tensor.Batch == 1
            && tensor.Height > 0
            && tensor.Width == melBands
            && tensor.Channels == 1;
    }
}
