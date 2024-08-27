using BenchmarkDotNet.Attributes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace basic_benchmark;

public class ExtremeValueFinder_benchmark
{

    [Params(10000, 100000, 1000000, 10000000, 100000000)]
    public int N;


    public float[] _array;

    [GlobalSetup]
    public void Setup()
    {
        _array = new float[N];
        //for (int i = 0; i < _array.Length; i++)
        //{
        //    _array[i] = Random.Shared.NextSingle();
        //}
    }

    [Benchmark]
    public (float max, float min) Normal() =>
        Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue(_array);


    [Benchmark]
    public (float max, float min) Vector128() =>
        Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector128(_array);

    [Benchmark]
    public (float max, float min) Vector256() =>
        Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector256(_array);

    [Benchmark]
    public (float max, float min) Vector512() =>
        Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector512(_array);


    [Benchmark]
    public (float max, float min) Vector128_p() =>
        another_extremeValueFinder.FindExtremeValue_Vector128(_array);

    [Benchmark]
    public (float max, float min) Vector256_p() =>
        another_extremeValueFinder.FindExtremeValue_Vector256(_array);

}
