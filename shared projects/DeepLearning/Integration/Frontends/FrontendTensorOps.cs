namespace Vorcyc.Mathematics.DeepLearning.Integration.Frontends;

internal static class FrontendTensorOps
{
    internal static void RequireWaveformLayout(BatchTensor<float> input, string parameterName = "input")
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Height != 1 || input.Width != 1)
        {
            throw new ArgumentException("Expected waveform layout N×1×1×L.", parameterName);
        }
    }

    internal static void RequireSpectralLayout(BatchTensor<float> input, int frequencyBins, string parameterName = "input")
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Width != frequencyBins || input.Channels != 1)
        {
            throw new ArgumentException($"Expected spectral layout N×T×{frequencyBins}×1.", parameterName);
        }
    }

    internal static BatchTensor<float> ZeroGradLike(BatchTensor<float> reference)
        => new(reference.Batch, reference.Height, reference.Width, reference.Channels);
}
