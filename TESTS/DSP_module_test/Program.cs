using System.Buffers;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Extensions.FFTW;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Numerics;

namespace DSP_module_test;

internal class Program
{
    static void Main(string[] args)
    {


        var input = new float[1024];
        input.Fill(0, 1f);
        var output = new ComplexFp32[500];//ArrayPool<ComplexFp32>.Shared.Rent(1024 / 2 + 1);
        Dft1D.Forward(input, output);
        output.PrintLine();
        Dft1D.Inverse(output, input);
        input.PrintLine();


        FFTW_ALL.Run();



        //var results = WindowApplier.CompareAllWindowPairs(1024);
        //foreach (var result in results)
        //{
        //    if (result.Equal)
        //        result.PrintLine(ConsoleColor.Green);
        //    else
        //        result.PrintLine(ConsoleColor.Red);

        //}







    }
}
