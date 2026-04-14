using Vorcyc.Mathematics.SignalProcessing.Effects;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

namespace DSP_module_test;

internal static class SignalGenerators_test
{
    public static bool RunNamed(out string? failure)
    {
        if (!SineMatchesExtension()) { failure = nameof(SineMatchesExtension); return false; }
        if (!ChirpWrapsAtLength()) { failure = nameof(ChirpWrapsAtLength); return false; }
        if (!TremoloAcceptsCustomLfo()) { failure = nameof(TremoloAcceptsCustomLfo); return false; }
        if (!VibratoLfoFrequencyTuning()) { failure = nameof(VibratoLfoFrequencyTuning); return false; }
        failure = null;
        return true;
    }

    private static bool SineMatchesExtension()
    {
        const int length = 256;
        const float rate = 8000f;
        const float freq = 100f;

        var ext = new Signal(length, rate);
        ext.GenerateWave(WaveShape.Sine, freq, Behaviour.Replace);

        var osc = new SineOscillator { SamplingRate = rate, Frequency = freq, Min = -1, Max = 1 };
        var gen = osc.ToSignal(length, rate);

        for (var i = 0; i < length; i++)
        {
            if (MathF.Abs(ext[i] - gen[i]) > 1e-4f)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ChirpWrapsAtLength()
    {
        const int length = 64;
        const float rate = 44100f;

        var chirp = new ChirpOscillator
        {
            SamplingRate = rate,
            Length = length,
            StartFrequency = 100f,
            EndFrequency = 1000f
        };

        var first = new float[length];
        for (var i = 0; i < length; i++)
        {
            first[i] = chirp.NextSample();
        }

        var second = new float[length];
        for (var i = 0; i < length; i++)
        {
            second[i] = chirp.NextSample();
        }

        for (var i = 0; i < length; i++)
        {
            if (MathF.Abs(first[i] - second[i]) > 1e-5f)
            {
                return false;
            }
        }

        return true;
    }

    private static bool TremoloAcceptsCustomLfo()
    {
        var lfo = new ChirpOscillator
        {
            SamplingRate = 44100f,
            Length = 32,
            StartFrequency = 1f,
            EndFrequency = 2f,
            Min = 0f,
            Max = 1f
        };

        var effect = new TremoloEffect(lfo, depth: 0.5f);
        effect.Process(1f);
        effect.Reset();
        return true;
    }

    private static bool VibratoLfoFrequencyTuning()
    {
        const int rate = 44100;
        var effect = new VibratoEffect(rate, lfoFrequency: 2f);

        if (effect.Lfo is not IAmplitudeOscillator osc || MathF.Abs(osc.Frequency - 2f) > 1e-5f)
        {
            return false;
        }

        effect.LfoFrequency = 5f;
        return effect.Lfo is IAmplitudeOscillator tuned && MathF.Abs(tuned.Frequency - 5f) < 1e-5f;
    }
}
