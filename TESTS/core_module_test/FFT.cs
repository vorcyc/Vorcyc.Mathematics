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
        FastFourierTransformNormal.Forward(input, output);


        foreach (var x in output)
            Console.WriteLine(x);

        Console.WriteLine("----------------");

        FastFourierTransformNormal.Inverse(output);

        foreach (var x in output)
            Console.WriteLine(x);
    }
}
