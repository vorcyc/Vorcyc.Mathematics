using Vorcyc.Mathematics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Core_example;

internal static class MatrixDemo
{
    public static int Run()
    {
        Console.WriteLine("--- 矩阵乘法 (Matrix.Multiply + ComputingContext) ---");

        const int n = 32;
        var a = new Matrix(n, n);
        var b = new Matrix(n, n);
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                a[i, j] = (i + 1) * 0.01f + j * 0.1f;
                b[i, j] = (j + 1) * 0.02f - i * 0.05f;
            }
        }

        var baseline = a * b;
        var parallel = Matrix.Multiply(a, b, ComputingContext.Parallel);

        float maxDiff = 0f;
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                maxDiff = MathF.Max(maxDiff, MathF.Abs(baseline[i, j] - parallel[i, j]));
            }
        }

        Console.WriteLine($"矩阵规模: {n}×{n}");
        Console.WriteLine($"operator* vs Parallel 最大差: {maxDiff:E3}");
        Console.WriteLine($"示例元素 [0,0]: {baseline[0, 0]:F4} / {parallel[0, 0]:F4}");

        using (ComputingScope.Enter(ComputingContext.Parallel))
        {
            var scoped = Matrix.Multiply(a, b, context: null);
            Console.WriteLine($"ComputingScope Parallel [0,0]: {scoped[0, 0]:F4}");
        }

        return 0;
    }
}
