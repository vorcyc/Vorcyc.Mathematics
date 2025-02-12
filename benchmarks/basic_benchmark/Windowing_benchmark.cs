using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics;

namespace basic_benchmark;

//public class Windowing_benchmark
//{


//    [Params(500, 5000, 10000, 50000, 100000, 500000, 5000000)]
//    public int N;


//    private float[] _values;

//    [GlobalSetup]
//    public void Setup()
//    {
//        _values = new float[N];
//    }

//    [Benchmark]
//    public void hamming_naive() => Vorcyc.Mathematics.SignalProcessing.Windowing.Hamming(_values);

//    [Benchmark]
//    public void hamming_ManagedPointer() => Vorcyc.Mathematics.SignalProcessing.Windowing.Windowing2.Hamming2(_values);







//}
