using core_module_test;
using System.Drawing;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Experimental.Signals;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.LanguageExtension;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;
using Vorcyc.Mathematics.Helpers;
using Vorcyc.Mathematics.LinearAlgebra;

//SimpleLinearRegression_test.go();


//ExtremeValueFinder_test.go();

//FFT.new_realOnlyFFT();

//Statistics_test.go();

//CuFFT_test.my();


//signal_test.go();

//var s1 = new Signal(100, 8000);
//var s2 = new Signal(100, 8000);

//s1.UnderlayingArray.Fill(1);
//s2.UnderlayingArray.Fill(2);

//var s3 = s1 * s2;

//Console.WriteLine(s3);


Matrix<int> m = new Matrix<int>(3, 3);
for (int x = 0; x < m.Rows; x++)
{
    for (int y = 0; y < m.Columns; y++)
    {
        m[x, y] = x + y;
    }
}


m.QRDecomposition(out var q, out var r);

Console.WriteLine(  q);
Console.WriteLine(  r);