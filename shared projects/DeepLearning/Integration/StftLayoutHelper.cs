namespace Vorcyc.Mathematics.DeepLearning.Integration;

/// <summary>
/// Frame-count helpers aligned with <see cref="Vorcyc.Mathematics.SignalProcessing.Transforms.Stft.Spectrogram"/>.
/// </summary>
internal static class StftLayoutHelper
{
    internal static int FrameCount(int sampleLength, int windowSize, int hopSize)
    {
        if (sampleLength < windowSize)
        {
            return 0;
        }

        var completeFrames = (sampleLength - windowSize) / hopSize + 1;
        return completeFrames + 1;
    }

    internal static int FrequencyBinCount(int fftSize) => fftSize / 2 + 1;
}
