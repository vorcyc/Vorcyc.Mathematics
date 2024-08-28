using BenchmarkDotNet.Attributes;
using Vorcyc.Mathematics;

namespace basic_benchmark;

public class ExtremeValueFinder_benchmark
{

    [Params(10000, 100000, 1000000, 10000000, 100000000)]
    public int N;


    //public float[] _array;
    private PinnableArray<float> _array;

    [GlobalSetup]
    public void Setup()
    {
        //_array = new float[N];
        //for (int i = 0; i < _array.Length; i++)
        //{
        //    _array[i] = Random.Shared.NextSingle();
        //}
        _array?.Dispose();
        _array = new(N, true);
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
        Vorcyc.Mathematics.Statistics.old_extremeValueFinder.FindExtremeValue_Vector128(_array);

    [Benchmark]
    public (float max, float min) Vector256_p() =>
        Vorcyc.Mathematics.Statistics.old_extremeValueFinder.FindExtremeValue_Vector256(_array);

}
