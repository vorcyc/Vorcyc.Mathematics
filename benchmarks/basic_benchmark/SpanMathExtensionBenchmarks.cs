using BenchmarkDotNet.Attributes;
using Vorcyc.Mathematics.Helpers;

namespace basic_benchmark;

public class SpanMathExtensionBenchmarks
{

    private float[] data;
    private int value;
    private float[] data2;


    [Params(500, 5000, 50000, 500000, 5000000)]
    public int N;


    [GlobalSetup]
    public void Setup()
    {
        data = new float[N];
        data2 = new float[N];
        value = 5;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = i;
            data2[i] = i;
        }
    }

    [Benchmark]
    public void AddOriginal()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] += value;
        }
    }

    [Benchmark]
    public void AddSIMD()
    {
        data.AsSpan().Add(value);
    }

    [Benchmark]
    public void AddOriginalSpan()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] += data2[i];
        }
    }

    [Benchmark]
    public void AddSIMDSpan()
    {
        data.AsSpan().Add(data2);
    }




}
