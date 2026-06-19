using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Statistics;

namespace Core_example;

internal static class ComputingContextDemo
{
    public static int Run()
    {
        Console.WriteLine("--- ComputingContext 解析优先级 ---");

        Span<float> data = stackalloc float[64];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = i + 1;
        }

        float explicitParallel = data.Sum(ComputingContext.Parallel);
        Console.WriteLine($"显式 Parallel Sum: {explicitParallel:F1}");

        float scoped;
        using (ComputingScope.Enter(ComputingContext.Simd))
        {
            scoped = data.Sum(context: null);
        }

        Console.WriteLine($"Scope SIMD + context:null Sum: {scoped:F1}");

        using (ComputingScope.Enter(ComputingContext.Parallel))
        {
            float scopeWins = data.Sum(ComputingContext.Normal);
            Console.WriteLine($"Scope Parallel 内显式 Normal 优先: {scopeWins:F1}");
        }

        Console.WriteLine($"Resolve(null) → Default: {ComputingContext.Resolve(null).CpuMode}");
        Console.WriteLine($"ParallelReductionThreshold: {ComputingContextExecution.ParallelReductionThreshold:N0}");
        return 0;
    }
}
