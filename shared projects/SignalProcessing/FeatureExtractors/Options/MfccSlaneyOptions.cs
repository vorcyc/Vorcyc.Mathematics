using System.Runtime.Serialization;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options;

[DataContract]
public class MfccSlaneyOptions : MfccOptions
{
    public MfccSlaneyOptions(int samplingRate,
                             int featureCount,
                             float frameDuration,
                             float lowFrequency = 0,
                             float highFrequency = 0,
                             int filterbankSize = 40,
                             int fftSize = 0,
                             bool normalize = true)
    {
        var frameSize = (int)(frameDuration * samplingRate);
        fftSize = fftSize > frameSize ? fftSize :frameSize.NextPowerOf2();

        FilterBank = FilterBanks.MelBankSlaney(filterbankSize, fftSize, samplingRate, lowFrequency, highFrequency, normalize);
        FilterBankSize = filterbankSize;
        FeatureCount = featureCount;
        FftSize = fftSize;
        SamplingRate = samplingRate;
        LowFrequency = lowFrequency;
        HighFrequency = highFrequency;
        NonLinearity = NonLinearityType.LogE;
    }
}
