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
        //var input = new float[1024];
        //input.Fill(0, 1f);
        //var output = new ComplexFp32[500];//ArrayPool<ComplexFp32>.Shared.Rent(1024 / 2 + 1);
        //Dft1D.Forward(input, output);
        //output.PrintLine();
        //Dft1D.Inverse(output, input);
        //input.PrintLine();
        //FFTW_ALL.Run();

        var s = new Signal(TimeSpan.FromSeconds(1), 1000);
        Console.WriteLine(s.Length);
        Console.WriteLine(s.Duration);
        var seg = s[0, 500];
        Console.WriteLine(seg.Value.Length);
        var freq = seg.Value.TransformToFrequencyDomain();
        Console.WriteLine(freq.Offset);
        Console.WriteLine(freq.ActualLength);
        Console.WriteLine(freq.TransformLength);
        Console.WriteLine("----------");
        Console.WriteLine(500.NextPowerOf2());
    }
}
