using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;
using Vorcyc.Mathematics.SignalProcessing.Operations;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DSP_module_test;

internal static class SignalPhase2_test
{
    public static bool RunNamed(out string? failure)
    {
        if (!FirFilterApplyToReturnsSignal()) { failure = nameof(FirFilterApplyToReturnsSignal); return false; }
        if (!OperationConvolveReturnsSignal()) { failure = nameof(OperationConvolveReturnsSignal); return false; }
        failure = null;
        return true;
    }

    private static bool FirFilterApplyToReturnsSignal()
    {
        const int length = 128;
        const float rate = 8000f;
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, 100f);

        var kernel = DesignFilter.FirWinLp(15, 0.2f);
        var filtered = new FirFilter(kernel).ApplyTo(signal);

        return filtered.Length > 0 && MathF.Abs(filtered.SamplingRate - rate) < 1e-5f;
    }

    private static bool OperationConvolveReturnsSignal()
    {
        var signal = Signal.Unit(32, 1000f);
        var kernel = Signal.Constant(0.25f, 4, 1000f);
        var result = Operation.Convolve(signal, kernel);

        return result.Length == signal.Length + kernel.Length - 1;
    }
}
