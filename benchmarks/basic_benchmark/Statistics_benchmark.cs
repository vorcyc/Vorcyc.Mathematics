using BenchmarkDotNet.Attributes;
using Vorcyc.Mathematics;

namespace basic_benchmark;

public class Statistics_benchmark
{

    [Params(500, 5000, 50000, 500000, 5000000)]
    public int N;


    private float[] _values;

    [GlobalSetup]
    public void Setup()
    {
        _values = new float[N];
    }

    [Benchmark]
    public float normal() => _values.AsSpan().Average<float>();

    [Benchmark]
    public float simd() => _values.AsSpan().Average();



}
