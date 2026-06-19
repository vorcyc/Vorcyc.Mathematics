using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Statistics;

namespace Core_example;

internal static class StatisticsDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 统计归约 (Span + ComputingContext) ---");

        Span<float> values = stackalloc float[256];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = MathF.Sin(i * 0.07f) + 0.5f;
        }

        float sumDefault = values.Sum();
        float sumSimd = values.Sum(ComputingContext.Simd);
        float sumParallel = values.Sum(ComputingContext.Parallel);
        float avg = values.Average(ComputingContext.Normal);
        float std = values.StandardDeviation(ComputingContext.Normal);

        Console.WriteLine($"长度: {values.Length}");
        Console.WriteLine($"Sum: default={sumDefault:F4}, SIMD={sumSimd:F4}, Parallel={sumParallel:F4}");
        Console.WriteLine($"Average={avg:F4}, StdDev={std:F4}");

        float scopedSum;
        using (ComputingScope.Enter(ComputingContext.Simd))
        {
            scopedSum = values.Sum();
        }

        Console.WriteLine($"Scope SIMD Sum: {scopedSum:F4}");
        return 0;
    }
}
