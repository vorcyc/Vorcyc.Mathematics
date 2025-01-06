//using BenchmarkDotNet.Attributes;
//using Vorcyc.Mathematics;

//namespace basic_benchmark;

//public class SimpleLinearRegression_benchmark
//{

//    [Params(10000, 100000, 1000000, 10000000, 100000000)]
//    public int N;


//    private PinnableArray<float> x;
//    private PinnableArray<float> y;

//    [GlobalSetup]
//    public void Setup()
//    {
//        x?.Dispose(); y?.Dispose();
//        x = new(N, true);
//        y = new(N, true);
//    }


//    [Benchmark]
//    public (float slope, float intercept, float correlationCoefficient) Normal() =>
//        Vorcyc.Mathematics.MachineLearning.SimpleLinearRegression<float>.<float>(x, y); 
    
//    [Benchmark]
//    public (float slope, float intercept, float correlationCoefficient) SIMD() =>
//        Vorcyc.Mathematics.Statistics.another_SimpleLinearRegression.ComputeParameters(x, y);





//}
