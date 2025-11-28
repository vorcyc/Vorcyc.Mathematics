using System;
using System.Collections.Generic;
using System.Text;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Extensions.FFTW.Helpers;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Numerics;

namespace DSP_module_test
{
    internal class FFTW
    {


        public static void _2D_FFTW_Test()
        {

            var input = new float[1024];
            var input2 = new PinnableArray<ComplexFp32>(1024, true);
            var output = new PinnableArray<ComplexFp32>(1024, true);
            input.Fill(0, 1f);
            input2.Fill(ComplexFp32.Zero, new ComplexFp32(1f, 0f));

            Vorcyc.Mathematics.Extensions.FFTW.Dft2D.Forward(input2, output, 32, 32);

            Console.WriteLine("DFT computation completed.");

            output.PrintLine();


            Vorcyc.Mathematics.Extensions.FFTW.Dft2D.Inverse(output, input2, 32, 32);

            Console.WriteLine("Inverse DFT computation completed.");

            input2.ScaleInPlace();

            input2.PrintLine();



        }
    }
}
