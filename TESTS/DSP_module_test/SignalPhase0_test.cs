using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DSP_module_test;

internal static class SignalPhase0_test
{
    public static bool Run()
    {
        return RunNamed(out _);
    }

    public static bool RunNamed(out string? failure)
    {
        if (!FromCopyRoundTrip()) { failure = nameof(FromCopyRoundTrip); return false; }
        if (!SegmentIsZeroCopy()) { failure = nameof(SegmentIsZeroCopy); return false; }
        if (!CloneRangeMatchesDecouple()) { failure = nameof(CloneRangeMatchesDecouple); return false; }
        if (!IndexerSetInvalidatesCache()) { failure = nameof(IndexerSetInvalidatesCache); return false; }
        if (!OperatorMismatchThrows()) { failure = nameof(OperatorMismatchThrows); return false; }
        if (!FloatSamplingRateDuration()) { failure = nameof(FloatSamplingRateDuration); return false; }
        failure = null;
        return true;
    }

    private static bool FromCopyRoundTrip()
    {
        var source = new float[] { 0.1f, 0.2f, 0.3f, -0.4f };
        var signal = Signal.FromCopy(source, 48000.5f);

        if (signal.Length != source.Length || signal.SamplingRate != 48000.5f)
        {
            return false;
        }

        for (int i = 0; i < source.Length; i++)
        {
            if (signal.Samples[i] != source[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool SegmentIsZeroCopy()
    {
        var signal = new Signal(8, 1000f);
        for (int i = 0; i < signal.Length; i++)
        {
            signal.Samples[i] = i;
        }

        var segment = signal[2, 3];
        if (segment is not SignalSegment view || view.Length != 3)
        {
            return false;
        }

        view.Samples[0] = 99f;
        return signal.Samples[2] == 99f;
    }

    private static bool CloneRangeMatchesDecouple()
    {
        var signal = Signal.Constant(0.5f, 16, 8000f);
        signal.Samples[0] = 1f;
        signal.Samples[15] = -1f;

        if (signal[4, 6] is not SignalSegment segment)
        {
            return false;
        }

        var cloned = signal.CloneRange(4, 6);
        var decoupled = segment.Decouple();

        for (int i = 0; i < 6; i++)
        {
            if (cloned.Samples[i] != segment.Samples[i] || decoupled.Samples[i] != segment.Samples[i])
            {
                return false;
            }
        }

        cloned.Samples[0] = 42f;
        return signal.Samples[4] != 42f;
    }

    private static bool IndexerSetInvalidatesCache()
    {
        var signal = Signal.Constant(1f, 32, 44100f);
        var before = signal.Rms;
        signal[0] = 100f;
        return before != signal.Rms;
    }

    private static bool OperatorMismatchThrows()
    {
        var a = new Signal(4, 1000f);
        var b = new Signal(8, 1000f);

        try
        {
            _ = a + b;
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static bool FloatSamplingRateDuration()
    {
        const float sampleRate = 48000.5f;
        const int length = 48000;
        var signal = new Signal(length, sampleRate);

        if (signal.SamplingRate != sampleRate)
        {
            return false;
        }

        if (MathF.Abs((float)signal.Duration.TotalSeconds - length / sampleRate) > 1e-6f)
        {
            return false;
        }

        var fromDuration = new Signal(TimeSpan.FromSeconds(1), 1000f);
        return fromDuration.Length == 1000 && fromDuration.SamplingRate == 1000f;
    }
}
