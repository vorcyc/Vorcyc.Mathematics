using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options;
using Vorcyc.Mathematics.SignalProcessing.Features;
using Vorcyc.Mathematics.SignalProcessing.Operations;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace DSP_module_test;

internal static class SignalPhase3_test
{
    public static bool RunNamed(out string? failure)
    {
        if (!ModulatorReturnsSignal()) { failure = nameof(ModulatorReturnsSignal); return false; }
        if (!FrameStatisticsMatchLegacy()) { failure = nameof(FrameStatisticsMatchLegacy); return false; }
        if (!RealFftMagnitudeSpectrumReturnsSignal()) { failure = nameof(RealFftMagnitudeSpectrumReturnsSignal); return false; }
        if (!TimeDomainFeaturesExtractorWorks()) { failure = nameof(TimeDomainFeaturesExtractorWorks); return false; }
        if (!TimeDomainFeaturesSignalPathMatchesArrayPath()) { failure = nameof(TimeDomainFeaturesSignalPathMatchesArrayPath); return false; }
        if (!ConvolverSignalPathWorks()) { failure = nameof(ConvolverSignalPathWorks); return false; }
        if (!ResampleSignalPathWorks()) { failure = nameof(ResampleSignalPathWorks); return false; }
        if (!PitchSignalPathMatchesSpan()) { failure = nameof(PitchSignalPathMatchesSpan); return false; }
        if (!ModulatorDemodulateWorks()) { failure = nameof(ModulatorDemodulateWorks); return false; }
        if (!SpectralSubtractorSignalCtorWorks()) { failure = nameof(SpectralSubtractorSignalCtorWorks); return false; }
        if (!SignalExtensionsUtilitiesWork()) { failure = nameof(SignalExtensionsUtilitiesWork); return false; }
        if (!HarmonicPercussiveSeparatorWorks()) { failure = nameof(HarmonicPercussiveSeparatorWorks); return false; }
        if (!SignalSuperimposeAndCrossfadeWork()) { failure = nameof(SignalSuperimposeAndCrossfadeWork); return false; }
        if (!SignalFadeWorks()) { failure = nameof(SignalFadeWorks); return false; }
        failure = null;
        return true;
    }

    private static bool ModulatorReturnsSignal()
    {
        var carrier = new Signal(64, 8000f);
        carrier.GenerateWave(WaveShape.Sine, 440f);
        var result = Modulator.Amplitude(carrier, 20f, 0.5f);
        return result is Signal && result.Length == carrier.Length;
    }

    private static bool FrameStatisticsMatchLegacy()
    {
        var signal = Signal.Constant(0.5f, 32, 1000f);
        var energy = signal.Energy(0, 16);
        var rms = signal.Rms(0, 16);
        return energy > 0.24f && energy < 0.26f && MathF.Abs(rms - 0.5f) < 1e-5f;
    }

    private static bool RealFftMagnitudeSpectrumReturnsSignal()
    {
        var signal = Signal.Unit(64, 1000f);
        var spectrum = new RealFft(64).MagnitudeSpectrum(signal);
        return spectrum.Length == 33 && spectrum.SamplingRate == signal.SamplingRate;
    }

    private static bool TimeDomainFeaturesExtractorWorks()
    {
        var options = new MultiFeatureOptions
        {
            SamplingRate = 1000,
            FrameDuration = 0.01f,
            HopDuration = 0.005f,
            FeatureList = TimeDomainFeaturesExtractor.FeatureSet
        };

        var extractor = new TimeDomainFeaturesExtractor(options);
        var samples = new float[1000];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = MathF.Sin(2 * MathF.PI * 100f * i / 1000f);
        }

        var vectors = extractor.ComputeFrom(samples);
        return vectors.Count > 0 && vectors[0].Length == 4;
    }

    private static bool TimeDomainFeaturesSignalPathMatchesArrayPath()
    {
        const int rate = 1000;
        var options = new MultiFeatureOptions
        {
            SamplingRate = rate,
            FrameDuration = 0.01f,
            HopDuration = 0.005f,
            FeatureList = TimeDomainFeaturesExtractor.FeatureSet
        };

        var extractor = new TimeDomainFeaturesExtractor(options);
        var signal = new Signal(1000, rate);
        signal.GenerateWave(WaveShape.Sine, 100f);

        var fromSignal = extractor.ComputeFrom(signal);
        var fromArray = extractor.ComputeFrom(signal.Samples.ToArray());

        if (fromSignal.Count != fromArray.Count || fromSignal.Count == 0)
        {
            return false;
        }

        const float tol = 1e-4f;
        for (var i = 0; i < fromSignal.Count; i++)
        {
            for (var j = 0; j < fromSignal[i].Length; j++)
            {
                if (MathF.Abs(fromSignal[i][j] - fromArray[i][j]) > tol)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool ConvolverSignalPathWorks()
    {
        var kernel = Signal.Unit(4, 1000f);
        var signal = Signal.Constant(1f, 8, 1000f);
        var result = new Convolver().Convolve(signal, kernel);
        return result.Length == signal.Length + kernel.Length - 1 && result[0] > 0.9f;
    }

    private static bool ResampleSignalPathWorks()
    {
        var signal = Signal.Constant(1f, 100, 8000f);
        var resampled = Operation.Resample(signal, 16000f);
        return resampled.Length == 200 && MathF.Abs(resampled.SamplingRate - 16000f) < 1e-3f;
    }

    private static bool PitchSignalPathMatchesSpan()
    {
        const int rate = 8000;
        var signal = new Signal(512, rate);
        signal.GenerateWave(WaveShape.Sine, 200f);

        var fromSignal = Pitch.FromYin(signal);
        var fromSpan = Pitch.FromYin(signal.Samples, rate);
        return MathF.Abs(fromSignal - fromSpan) < 1e-3f;
    }

    private static bool ModulatorDemodulateWorks()
    {
        var signal = new Signal(128, 8000f);
        signal.GenerateWave(WaveShape.Sine, 440f);
        var modulated = Modulator.Amplitude(signal, 10f, 0.5f);
        var demodulated = Modulator.DemodulateAmplitude(modulated);
        return demodulated.Length == modulated.Length && demodulated.Rms(0, demodulated.Length) > 0;
    }

    private static bool SpectralSubtractorSignalCtorWorks()
    {
        var noise = Signal.Constant(0.01f, 512, 8000f);
        var subtractor = new SpectralSubtractor(noise, fftSize: 256, hopSize: 64);
        var speech = new Signal(512, 8000f);
        speech.GenerateWave(WaveShape.Sine, 200f);
        var filtered = subtractor.ApplyTo(speech);
        return filtered.Length == speech.Length;
    }

    private static bool SignalExtensionsUtilitiesWork()
    {
        var left = Signal.Constant(1f, 4, 1000f);
        var right = Signal.Constant(2f, 4, 1000f);
        var concat = left.Concatenate(right);
        if (concat.Length != 8 || MathF.Abs(concat[4] - 2f) > 1e-5f)
        {
            return false;
        }

        var delayed = left.Delay(2);
        return delayed.Length == 6 && MathF.Abs(delayed[0]) < 1e-5f && MathF.Abs(delayed[2] - 1f) < 1e-5f;
    }

    private static bool HarmonicPercussiveSeparatorWorks()
    {
        var signal = new Signal(1024, 8000f);
        signal.GenerateWave(WaveShape.Sine, 220f);

        var separator = new HarmonicPercussiveSeparator(fftSize: 256, hopSize: 64);
        var (harmonicSpec, percussiveSpec) = separator.EvaluateSpectrograms(signal);

        return harmonicSpec.Magnitudes.Count > 0
            && percussiveSpec.Magnitudes.Count == harmonicSpec.Magnitudes.Count
            && harmonicSpec.Magnitudes[0].Length == percussiveSpec.Magnitudes[0].Length;
    }

    private static bool SignalSuperimposeAndCrossfadeWork()
    {
        var shortSignal = Signal.Constant(1f, 4, 1000f);
        var longSignal = Signal.Constant(2f, 8, 1000f);
        var sum = shortSignal.Superimpose(longSignal);

        if (sum.Length != 8 || MathF.Abs(sum[0] - 3f) > 1e-5f || MathF.Abs(sum[4] - 2f) > 1e-5f)
        {
            return false;
        }

        var diff = longSignal.Subtract(shortSignal);
        if (diff.Length != 8 || MathF.Abs(diff[0] - 1f) > 1e-5f)
        {
            return false;
        }

        var crossfaded = shortSignal.Crossfade(longSignal, 0.001);
        return crossfaded.Length == shortSignal.Length + longSignal.Length - 1;
    }

    private static bool SignalFadeWorks()
    {
        var signal = Signal.Constant(1f, 100, 1000f);
        signal.FadeIn(0.01);
        signal.FadeOut(0.01);

        return MathF.Abs(signal[0]) < 1e-5f && MathF.Abs(signal[99]) < 1e-5f && signal[50] > 0.9f;
    }
}
