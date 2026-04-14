namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics;

/// <summary>
/// <see cref="ComputingContext"/>-aware vector span operations.
/// </summary>
public static partial class VectorSpan
{
    /// <summary>Dot product with optional execution policy.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException("向量长度必须相同。", nameof(b));
        }

        if (ComputingContextExecution.UseParallel(context, a.Length))
        {
            return DotParallel(a, b, context);
        }

        return ComputingContext.Resolve(context).ResolveCpuMode(a.Length) == CpuExecutionMode.Normal
            ? DotScalar(a, b)
            : Dot(a, b);
    }

    /// <summary>Element sum with optional execution policy.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(ReadOnlySpan<T> values, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (ComputingContextExecution.UseParallel(context, values.Length))
        {
            return SumParallel(values, context);
        }

        return ComputingContext.Resolve(context).ResolveCpuMode(values.Length) == CpuExecutionMode.Normal
            ? SumScalar(values)
            : Sum(values);
    }

    /// <summary>Euclidean norm with optional execution policy.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Norm<T>(ReadOnlySpan<T> vector, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
        => T.Sqrt(Dot(vector, vector, context));

    /// <summary>Writes <c>a + b</c> with optional execution policy.</summary>
    public static void Add<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        ValidateSameLength(a, b, result);

        if (ComputingContextExecution.UseParallel(context, a.Length))
        {
            AddParallel(a, b, result, context);
            return;
        }

        if (ComputingContext.Resolve(context).ResolveCpuMode(a.Length) == CpuExecutionMode.Normal)
        {
            AddScalar(a, b, result);
            return;
        }

        Add(a, b, result);
    }

    /// <summary>Computes <c>y += alpha * x</c> with optional execution policy.</summary>
    public static void Axpy<T>(T alpha, ReadOnlySpan<T> x, Span<T> y, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("向量长度必须相同。", nameof(x));
        }

        if (ComputingContextExecution.UseParallel(context, x.Length))
        {
            AxpyParallel(alpha, x, y, context);
            return;
        }

        if (ComputingContext.Resolve(context).ResolveCpuMode(x.Length) == CpuExecutionMode.Normal)
        {
            AxpyScalar(alpha, x, y);
            return;
        }

        Axpy(alpha, x, y);
    }

    /// <summary>Writes <c>a - b</c> with optional execution policy.</summary>
    public static void Subtract<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        ValidateSameLength(a, b, result);

        if (ComputingContextExecution.UseParallel(context, a.Length))
        {
            SubtractParallel(a, b, result, context);
            return;
        }

        if (ComputingContext.Resolve(context).ResolveCpuMode(a.Length) == CpuExecutionMode.Normal)
        {
            SubtractScalar(a, b, result);
            return;
        }

        Subtract(a, b, result);
    }

    /// <summary>Writes <c>vector * scalar</c> with optional execution policy.</summary>
    public static void Scale<T>(ReadOnlySpan<T> vector, T scalar, Span<T> result, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (result.Length != vector.Length)
        {
            throw new ArgumentException("结果向量长度必须与输入向量相同。", nameof(result));
        }

        if (ComputingContextExecution.UseParallel(context, vector.Length))
        {
            ScaleParallel(vector, scalar, result, context);
            return;
        }

        if (ComputingContext.Resolve(context).ResolveCpuMode(vector.Length) == CpuExecutionMode.Normal)
        {
            ScaleScalar(vector, scalar, result);
            return;
        }

        Scale(vector, scalar, result);
    }

    private static void ValidateSameLength<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result)
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException("向量长度必须相同。", nameof(b));
        }

        if (result.Length != a.Length)
        {
            throw new ArgumentException("结果向量长度必须与输入向量相同。", nameof(result));
        }
    }

    private static T DotScalar<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
        where T : struct, IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (var i = 0; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }

        return sum;
    }

    private static T SumScalar<T>(ReadOnlySpan<T> values)
        where T : struct, IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (var i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    private static void AddScalar<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (var i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }
    }

    private static void ScaleScalar<T>(ReadOnlySpan<T> vector, T scalar, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (var i = 0; i < vector.Length; i++)
        {
            result[i] = vector[i] * scalar;
        }
    }

    private static void AxpyScalar<T>(T alpha, ReadOnlySpan<T> x, Span<T> y)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (var i = 0; i < x.Length; i++)
        {
            y[i] += alpha * x[i];
        }
    }

    private static void SubtractScalar<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (var i = 0; i < a.Length; i++)
        {
            result[i] = a[i] - b[i];
        }
    }

    private static void AxpyParallel<T>(T alpha, ReadOnlySpan<T> x, Span<T> y, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        var xData = x.ToArray();
        var yData = y.ToArray();
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        int length = xData.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            for (int i = start; i < end; i++)
            {
                yData[i] += alpha * xData[i];
            }
        });

        yData.AsSpan().CopyTo(y);
    }

    private static void SubtractParallel<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        var aData = a.ToArray();
        var bData = b.ToArray();
        var outData = GC.AllocateUninitializedArray<T>(result.Length);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        int length = aData.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            for (int i = start; i < end; i++)
            {
                outData[i] = aData[i] - bData[i];
            }
        });

        outData.AsSpan().CopyTo(result);
    }

    private static T DotParallel<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
        => ReduceParallel(a.Length, context, (dataA, dataB, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                local += dataA[i] * dataB[i];
            }

            return local;
        }, a.ToArray(), b.ToArray());

    private static T SumParallel<T>(ReadOnlySpan<T> values, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        var data = values.ToArray();
        return ReduceParallel(values.Length, context, (array, _, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                local += array[i];
            }

            return local;
        }, data, Array.Empty<T>());
    }

    private static void AddParallel<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        var aData = a.ToArray();
        var bData = b.ToArray();
        var outData = GC.AllocateUninitializedArray<T>(result.Length);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        int length = aData.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            for (int i = start; i < end; i++)
            {
                outData[i] = aData[i] + bData[i];
            }
        });

        outData.AsSpan(0, result.Length).CopyTo(result);
    }

    private static void ScaleParallel<T>(ReadOnlySpan<T> vector, T scalar, Span<T> result, ComputingContext? context)
        where T : struct, IFloatingPointIeee754<T>
    {
        var data = vector.ToArray();
        var outData = GC.AllocateUninitializedArray<T>(result.Length);
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            for (int i = start; i < end; i++)
            {
                outData[i] = data[i] * scalar;
            }
        });

        outData.AsSpan(0, result.Length).CopyTo(result);
    }

    private static T ReduceParallel<T>(
        int length,
        ComputingContext? context,
        Func<T[], T[], int, int, T> accumulate,
        T[] primary,
        T[] secondary)
        where T : struct, IFloatingPointIeee754<T>
    {
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new T[workers];
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            partials[worker] = accumulate(primary, secondary, start, end);
        });

        T sum = T.Zero;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }
}
