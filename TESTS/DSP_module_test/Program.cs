using System.Buffers;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.Extensions.FFTW;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Numerics;
namespace DSP_module_test;
internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(SignalPhase0_test.RunNamed(out var failure)
            ? "SignalPhase0: PASS"
            : $"SignalPhase0: FAIL ({failure})");
        Console.WriteLine(SignalGenerators_test.RunNamed(out failure)
            ? "SignalGenerators: PASS"
            : $"SignalGenerators: FAIL ({failure})");
        Console.WriteLine(SignalPhase2_test.RunNamed(out failure)
            ? "SignalPhase2: PASS"
            : $"SignalPhase2: FAIL ({failure})");
        Console.WriteLine(SignalPhase3_test.RunNamed(out failure)
            ? "SignalPhase3: PASS"
            : $"SignalPhase3: FAIL ({failure})");
        Console.WriteLine(FftKernel_test.RunNamed(out failure)
            ? "FftKernel: PASS"
            : $"FftKernel: FAIL ({failure})");
        Console.WriteLine(FftKernel_test.RunNamed64(out failure)
            ? "FftKernel64: PASS"
            : $"FftKernel64: FAIL ({failure})");
        Console.WriteLine(FftKernel_test.RunNamedSpan(out failure)
            ? "FftKernelSpan: PASS"
            : $"FftKernelSpan: FAIL ({failure})");
        Console.WriteLine(Stft_test.RunNamed(out failure)
            ? "Stft: PASS"
            : $"Stft: FAIL ({failure})");
        FftKernel_bench.Run();
    }
}
