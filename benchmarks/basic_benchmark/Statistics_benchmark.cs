using BenchmarkDotNet.Attributes;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Statistics;

namespace basic_benchmark;

public class Statistics_benchmark
{

    [Params(500, 5000, 50000)]
    public int N;


    private double[] _values;

    private double[] _values2;

    [GlobalSetup]
    public void Setup()
    {
        _values = new double[N];
        _values2 = new double[N];
    }

    //[Benchmark]
    //public (float Mean, float Median, float Mode, float Variance, float StandardDeviation, float CoefficientOfVariation) normal() => _values.AsSpan().CalculateAllStatistics<float>();

    //更低效 
    //[Benchmark]
    //public (float Mean, float Median, float Mode, float Variance, float StandardDeviation, float CoefficientOfVariation) simd() => _values.AsSpan().CalculateAllStatistics_SIMD();


    [Benchmark]
    public double Covariance_SIMD() => _values.AsSpan().Covariance<double>(_values2);

    [Benchmark]
    public double Covariance_LINQ() => Others2.Covariance(_values, _values2); 
    
    [Benchmark]
    public double Covariance_NORMAL() => Others3.Covariance(_values, _values2);



    [Benchmark]
    public double CorrelationCoefficient_SIMD() => _values.AsSpan().CorrelationCoefficient<double>(_values2);

    [Benchmark]
    public double CorrelationCoefficient_LINQ() => Others2.CorrelationCoefficient(_values, _values2); 
    
    [Benchmark]
    public double CorrelationCoefficient_NORMAL() => Others3.CorrelationCoefficient(_values, _values2);


    [Benchmark]
    public (double slope, double intercept) LinearRegression_SIMD() => _values.AsSpan().LinearRegression<double>(_values2);


    [Benchmark]
    public (double slope,double intercept) LinearRegression_LINQ() => Others2.LinearRegression(_values, _values2); 
    
    [Benchmark]
    public (double slope,double intercept) LinearRegression_NORMAL() => Others3.LinearRegression(_values, _values2);


}
