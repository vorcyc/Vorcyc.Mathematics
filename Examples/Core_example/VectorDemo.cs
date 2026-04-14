using Vorcyc.Mathematics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Core_example;

internal static class VectorDemo
{
    public static int Run()
    {
        Console.WriteLine("--- VectorSpan (Dot / Norm / Axpy) ---");

        Span<float> a = stackalloc float[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        Span<float> b = stackalloc float[8] { 8, 7, 6, 5, 4, 3, 2, 1 };
        Span<float> y = stackalloc float[8];

        float dot = VectorSpan.Dot(a, b, ComputingContext.Normal);
        float norm = VectorSpan.Norm(a, ComputingContext.Simd);
        VectorSpan.Axpy(0.5f, a, y, ComputingContext.Normal);

        Console.WriteLine($"Dot(a,b) = {dot:F2}");
        Console.WriteLine($"Norm(a) = {norm:F4}");
        Console.WriteLine($"Axpy 后 y[0..2]: {y[0]:F1}, {y[1]:F1}, {y[2]:F1}");

        var large = new float[100_000];
        for (var i = 0; i < large.Length; i++)
        {
            large[i] = i * 0.001f;
        }

        float largeDot = VectorSpan.Dot(large, large, ComputingContext.Parallel);
        Console.WriteLine($"大向量自点积 (Parallel): {largeDot:F2}");
        return 0;
    }
}
