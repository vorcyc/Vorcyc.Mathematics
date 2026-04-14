using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.DeepLearning.Integration.Frontends;

/// <summary>
/// Factory helpers for common frozen DSP frontends.
/// </summary>
public static class AudioFrontendLayers
{
    /// <summary>
    /// Builds [pre-emphasis → STFT magnitude → mel filterbank] layers.
    /// </summary>
    public static IBatchLayer<float>[] CreateMelSpectrogramStack(
        int samplingRate,
        int windowSize,
        int hopSize,
        int melBands,
        float preEmphasis = 0.97f,
        float lowFrequency = 0f,
        float highFrequency = 0f)
    {
        if (samplingRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(samplingRate));
        }

        var fftSize = windowSize.NextPowerOf2();
        var melEdges = FilterBanks.MelBands(melBands, samplingRate, lowFrequency, highFrequency);
        var filterBank = FilterBanks.Triangular(fftSize, samplingRate, melEdges, mapper: Scale.HerzToMel);

        return
        [
            new BatchPreEmphasisLayer(preEmphasis, name: "preemphasis"),
            new BatchStftMagnitudeLayer(windowSize, hopSize, name: "stft_mag"),
            new BatchMelFilterbankLayer(filterBank, name: "mel")
        ];
    }
}
