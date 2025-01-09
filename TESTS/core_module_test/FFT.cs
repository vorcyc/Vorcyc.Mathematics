using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace core_module_test;

internal class FFT
{


    public static void go()
    {


        ReadOnlySpan<float> input = [-1, 2, -3, 4, -5, 6, -7, 8, -9, 10, -11, 12, -13, 14, -15, 16];
        Span<ComplexFp32> output = new ComplexFp32[input.Length];
        FastFourierTransform.Forward(input, output);


        foreach (var x in output)
            Console.WriteLine(x);

        Console.WriteLine("----------------");

        FastFourierTransform.Inverse(output);

        foreach (var x in output)
            Console.WriteLine(x);
    }


    public static void new_realOnlyFFT()
    {

        var realonly = new RealOnlyFFT_Fp32(16);

        ReadOnlySpan<float> input = [-1, 2, -3, 4, -5, 6, -7, 8, -9, 10, -11, 12, -13, 14, -15, 16];

        var output = realonly.Forward(input);

        output.PrintLine();

        Console.WriteLine(new string('-', 10));

        var out2 = realonly.InverseNorm(output);

        out2.PrintLine();


       var mag= realonly.MagnitudeSpectrum (input);
        mag.PrintLine();

    }
}
