using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Operations;
using Vorcyc.Mathematics.SignalProcessing.Operations.Tsm;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.Effects;

/// <summary>
/// Represents offline Pitch Shift audio effect
/// based on one of the available TSM algorithms and linear interpolation.
/// <see cref="PitchShiftEffect"/> does not implement online processing (method <see cref="Process(float)"/>).
/// </summary>
public class PitchShiftEffect : AudioEffect
{
    public float Shift { get; set; }
    public TsmAlgorithm Tsm { get; set; }
    public int WindowSize { get; set; }
    public int HopSize { get; set; }

    public PitchShiftEffect(float shift,
                            int windowSize = 1024,
                            int hopSize = 128,
                            TsmAlgorithm tsm = TsmAlgorithm.PhaseVocoderPhaseLocking)
    {
        Shift = shift;
        WindowSize = windowSize;
        HopSize = hopSize;
        Tsm = tsm;
    }

    public override Signal ApplyTo(Signal signal, FilteringMethod method = FilteringMethod.Auto)
    {
        var stretched = Operation.TimeStretch(signal, Shift, WindowSize, HopSize, algorithm: Tsm);

        var resampled = InterpolateStretched(stretched, signal.Length, out _, out _);

        for (var i = 0; i < resampled.Length; i++)
        {
            resampled[i] = signal[i] * Dry + resampled[i] * Wet;
        }

        return Signal.FromCopy(resampled, signal.SamplingRate);
    }

    public override void Apply(Signal signal, FilteringMethod method = FilteringMethod.Auto)
    {
        var stretched = Operation.TimeStretch(signal, Shift, WindowSize, HopSize, algorithm: Tsm);

        var resampled = InterpolateStretched(stretched, signal.Length, out _, out _);

        var samples = signal.Samples;
        for (var i = 0; i < resampled.Length; i++)
        {
            samples[i] = samples[i] * Dry + resampled[i] * Wet;
        }

        signal.NotifySamplesModified();
    }

    public override float Process(float sample) => throw new NotImplementedException();

    public override void Reset() { }

    private float[] InterpolateStretched(Signal stretched, int outputLength, out float[] x, out float[] xresampled)
    {
        x = new float[stretched.Length];
        for (var i = 0; i < x.Length; i++)
        {
            x[i] = i;
        }

        xresampled = new float[outputLength];
        for (var i = 0; i < outputLength; i++)
        {
            xresampled[i] = Shift * i;
        }

        var resampled = new float[outputLength];
        VMath.InterpolateLinear(x, stretched.Samples, xresampled, resampled);
        return resampled;
    }
}
