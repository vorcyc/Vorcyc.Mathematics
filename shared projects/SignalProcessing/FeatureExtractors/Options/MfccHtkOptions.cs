using System.Runtime.Serialization;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options;

[DataContract]
public class MfccHtkOptions : MfccOptions
{
    public MfccHtkOptions(int samplingRate,
                          int featureCount,
                          float frameDuration,
                          float lowFrequency = 0f,
                          float highFrequency = 0f,
                          int filterbankSize = 24,
                          int fftSize = 0)
    {
        
        var frameSize = (int)(frameDuration * samplingRate);
        fftSize = fftSize > frameSize ? fftSize : frameSize.NextPowerOf2();// MathUtils.NextPowerOfTwo(frameSize);

        var melBands = FilterBanks.MelBands(filterbankSize, samplingRate, lowFrequency, highFrequency);
        FilterBank = FilterBanks.Triangular(fftSize, samplingRate, melBands, null, Scale.HerzToMel);
        FilterBankSize = filterbankSize;
        FeatureCount = featureCount;
        FftSize = fftSize;
        SamplingRate = samplingRate;
        LowFrequency = lowFrequency;
        HighFrequency = highFrequency;
        NonLinearity = NonLinearityType.LogE;
        LogFloor = 1.0f;
    }
}
