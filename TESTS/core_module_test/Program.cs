using core_module_test;
using System.Drawing;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.Statistics;


//SimpleLinearRegression_test.go();


//ExtremeValueFinder_test.go();

//FFT.new_realOnlyFFT();

//Statistics_test.go();

//CuFFT_test.my();

PinnableArray<float>.Option.UseLeasingMode = true;


PinnableArray<float> a = new(1000);
a.FillWithRandomNumber();

Console.WriteLine(a.ToString());

Console.WriteLine(  a.Values.Length);
Console.WriteLine(  a.Length);