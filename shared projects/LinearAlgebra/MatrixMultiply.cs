namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics;

/// <summary>
/// SIMD-friendly matrix multiplication kernels (row-major storage).
/// </summary>
internal static class MatrixMultiply
{
    private const int TransposeThreshold = 8;
    private const int BlockSize = 32;
    private const int BlockedMultiplyThreshold = 4096;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply<T>(
        ReadOnlySpan<T> a,
        int aRows,
        int aCols,
        ReadOnlySpan<T> b,
        int bRows,
        int bCols,
        Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
        => MultiplyCore(a, aRows, aCols, b, bRows, bCols, result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply<T>(
        T[] a,
        int aRows,
        int aCols,
        T[] b,
        int bRows,
        int bCols,
        T[] result,
        ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        int m = aRows;
        int k = aCols;
        int n = bCols;

        if (k != bRows)
            throw new ArgumentException("矩阵维度不匹配，无法相乘。");

        int problemSize = m * n * k;
        if (ComputingContextExecution.UseParallel(context, problemSize, ComputingContextExecution.ParallelMatrixMultiplyThreshold))
        {
            MultiplyParallel(a, m, k, b, n, result, context);
            return;
        }

        MultiplyCore(a.AsSpan(), aRows, aCols, b.AsSpan(), bRows, bCols, result.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Multiply<T>(
        ReadOnlySpan<T> a,
        int aRows,
        int aCols,
        ReadOnlySpan<T> b,
        int bRows,
        int bCols,
        Span<T> result,
        ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        int problemSize = aRows * aCols * bCols;
        if (ComputingContextExecution.UseParallel(context, problemSize, ComputingContextExecution.ParallelMatrixMultiplyThreshold))
        {
            var resultBuffer = GC.AllocateUninitializedArray<T>(result.Length);
            Multiply(a.ToArray(), aRows, aCols, b.ToArray(), bRows, bCols, resultBuffer, context);
            resultBuffer.AsSpan().CopyTo(result);
            return;
        }

        MultiplyCore(a, aRows, aCols, b, bRows, bCols, result);
    }

    private static void MultiplyCore<T>(
        ReadOnlySpan<T> a,
        int aRows,
        int aCols,
        ReadOnlySpan<T> b,
        int bRows,
        int bCols,
        Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        int m = aRows;
        int k = aCols;
        int n = bCols;

        if (k != bRows)
            throw new ArgumentException("矩阵维度不匹配，无法相乘。");

        int problemSize = m * n * k;
        if (problemSize > BlockedMultiplyThreshold)
        {
            MultiplyBlocked(a, m, k, b, n, result);
            return;
        }

        if (k <= TransposeThreshold && n <= TransposeThreshold)
        {
            MultiplyNaive(a, m, k, b, n, result);
            return;
        }

        var bTransposed = new T[n * k];
        Transpose(b, bRows, bCols, bTransposed);

        for (int i = 0; i < m; i++)
        {
            ReadOnlySpan<T> aRow = a.Slice(i * k, k);
            int resultRow = i * n;
            for (int j = 0; j < n; j++)
                result[resultRow + j] = VectorSpan.Dot(aRow, bTransposed.AsSpan(j * k, k));
        }
    }

    private static void MultiplyParallel<T>(
        T[] a,
        int m,
        int k,
        T[] b,
        int n,
        T[] result,
        ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        var bTransposed = new T[n * k];
        Transpose(b.AsSpan(), k, n, bTransposed);

        long workPerRow = (long)n * k;
        ComputingContextExecution.ForEach(context, 0, m, i =>
        {
            int aRow = i * k;
            int resultRow = i * n;
            for (int j = 0; j < n; j++)
            {
                T sum = T.Zero;
                int bRow = j * k;
                for (int t = 0; t < k; t++)
                {
                    sum += a[aRow + t] * bTransposed[bRow + t];
                }

                result[resultRow + j] = sum;
            }
        }, workPerRow);
    }

    private static void MultiplyBlocked<T>(
        ReadOnlySpan<T> a,
        int m,
        int k,
        ReadOnlySpan<T> b,
        int n,
        Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        result.Clear();

        var bTransposed = new T[n * k];
        Transpose(b, k, n, bTransposed);

        for (int i0 = 0; i0 < m; i0 += BlockSize)
        {
            int iMax = Math.Min(i0 + BlockSize, m);
            for (int j0 = 0; j0 < n; j0 += BlockSize)
            {
                int jMax = Math.Min(j0 + BlockSize, n);
                for (int k0 = 0; k0 < k; k0 += BlockSize)
                {
                    int kLen = Math.Min(k0 + BlockSize, k) - k0;
                    for (int i = i0; i < iMax; i++)
                    {
                        ReadOnlySpan<T> aSegment = a.Slice(i * k + k0, kLen);
                        int resultRow = i * n;
                        for (int j = j0; j < jMax; j++)
                            result[resultRow + j] += VectorSpan.Dot(aSegment, bTransposed.AsSpan(j * k + k0, kLen));
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MultiplyNaive<T>(
        ReadOnlySpan<T> a,
        int m,
        int k,
        ReadOnlySpan<T> b,
        int n,
        Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < m; i++)
        {
            int aRow = i * k;
            int resultRow = i * n;
            for (int j = 0; j < n; j++)
            {
                T sum = T.Zero;
                for (int t = 0; t < k; t++)
                    sum += a[aRow + t] * b[t * n + j];
                result[resultRow + j] = sum;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose<T>(ReadOnlySpan<T> source, int rows, int cols, Span<T> destination)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < rows; i++)
        {
            int sourceRow = i * cols;
            for (int j = 0; j < cols; j++)
                destination[j * rows + i] = source[sourceRow + j];
        }
    }
}
