using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

namespace core_module_test;

internal class signal_test
{


    public static void go()
    {

        //var s = new Signal(100, 8000);
        ////var seg = s[10, 50];
        ////seg.GenerateWave(WaveShape.Square, 10);


        ////for (int i = 0; i < s.Length; i++)
        ////{
        ////    Console.WriteLine(s.Samples[i]);
        ////}

        //s.GenerateWave(WaveShape.Sine, 80);
        ////for (int i = 0; i < s.Length; i++)
        ////{
        ////    Console.WriteLine(s.Samples[i]);
        ////}

        //var newSignal = s.Resample(4000);
        //for (int i = 0; i < newSignal.Length; i++)
        //{
        //    Console.WriteLine(newSignal.Samples[i]);
        //}


        var builder = new SineBuilder();
        var signal = builder.SampledAt(100).OfLength(100).Build();
        //foreach (var i in signal.Samples)
        //    Console.WriteLine(  i);


        var newSignal = Vorcyc.Mathematics.SignalProcessing.Operations.Operation.Resample(signal, 200);
        foreach (var item in newSignal.Samples)
            Console.WriteLine(item);
    }

}
