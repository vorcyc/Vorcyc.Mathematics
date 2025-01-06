using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Experimental.Signals;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace DSP_module_test;

internal class Program
{
    static void Main(string[] args)
    {


        //var a = new float[100];
        //a.Fill(1, 1f);
        //var b = a.Copy();

        //Vorcyc.Mathematics.SignalProcessing.Windowing.WindowApplier.Hamming(a);
        //a.PrintLine( ConsoleColor.Green);


        //Vorcyc.Mathematics.SignalProcessing.Windowing.WindowApplier.Hamming2(b);
        //b.PrintLine(ConsoleColor.Blue);

        var s = new Signal(10, 100);
        s.Samples.Fill(1, 1f);
        Console.WriteLine(s);


        var fd = s.TransformToFrequencyDomain();

        

         FastFourierTransform.Inverse(fd.Result);

        fd.Result.PrintLine();
    }
}
