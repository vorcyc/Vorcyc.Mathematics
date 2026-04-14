using System.Numerics;

using System.Numerics.Tensors;

using System.Runtime.CompilerServices;

using System.Runtime.InteropServices;



namespace Vorcyc.Mathematics.MachineLearning.Internal;



/// <summary>

/// float/double 热路径的向量化内核；其他 <typeparamref name="T"/> 回退标量循环。

/// </summary>

internal static class NumericKernels

{

    private const int MaxStackScratch = 128;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static T Dot<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)

        where T : struct, IFloatingPointIeee754<T>

    {

        if (a.Length != b.Length)

            throw new ArgumentException("向量长度不一致。");



        if (typeof(T) == typeof(double))

        {

            double sum = TensorPrimitives.Dot(

                MemoryMarshal.Cast<T, double>(a),

                MemoryMarshal.Cast<T, double>(b));

            return T.CreateTruncating(sum);

        }



        if (typeof(T) == typeof(float))

        {

            float sum = TensorPrimitives.Dot(

                MemoryMarshal.Cast<T, float>(a),

                MemoryMarshal.Cast<T, float>(b));

            return T.CreateTruncating(sum);

        }



        T scalar = T.Zero;

        for (int i = 0; i < a.Length; i++)

            scalar += a[i] * b[i];

        return scalar;

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static T SquaredDistance<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)

        where T : struct, IFloatingPointIeee754<T>

    {

        if (a.Length != b.Length)

            throw new ArgumentException("向量长度不一致。");



        if (typeof(T) == typeof(double))

        {

            double d = SquaredDistanceDouble(

                MemoryMarshal.Cast<T, double>(a),

                MemoryMarshal.Cast<T, double>(b));

            return T.CreateTruncating(d);

        }



        if (typeof(T) == typeof(float))

        {

            float d = SquaredDistanceFloat(

                MemoryMarshal.Cast<T, float>(a),

                MemoryMarshal.Cast<T, float>(b));

            return T.CreateTruncating(d);

        }



        T sum = T.Zero;

        for (int i = 0; i < a.Length; i++)

        {

            T diff = a[i] - b[i];

            sum += diff * diff;

        }

        return sum;

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static T SquaredDistanceToRow<T>(T[,] matrix, int row, ReadOnlySpan<T> sample)

        where T : struct, IFloatingPointIeee754<T>

    {

        int cols = sample.Length;



        if (typeof(T) == typeof(double))

        {

            var md = (double[,])(object)matrix;

            Span<double> rowScratch = cols <= MaxStackScratch ? stackalloc double[cols] : new double[cols];

            for (int j = 0; j < cols; j++)

                rowScratch[j] = md[row, j];

            double d = SquaredDistanceDouble(rowScratch, MemoryMarshal.Cast<T, double>(sample));

            return T.CreateTruncating(d);

        }



        if (typeof(T) == typeof(float))

        {

            var mf = (float[,])(object)matrix;

            Span<float> rowScratch = cols <= MaxStackScratch ? stackalloc float[cols] : new float[cols];

            for (int j = 0; j < cols; j++)

                rowScratch[j] = mf[row, j];

            float d = SquaredDistanceFloat(rowScratch, MemoryMarshal.Cast<T, float>(sample));

            return T.CreateTruncating(d);

        }



        T sum = T.Zero;

        for (int j = 0; j < cols; j++)

        {

            T diff = matrix[row, j] - sample[j];

            sum += diff * diff;

        }

        return sum;

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static T SquaredDistanceBetweenRows<T>(T[,] a, int rowA, T[,] b, int rowB)

        where T : struct, IFloatingPointIeee754<T>

    {

        int cols = a.GetLength(1);

        if (cols != b.GetLength(1))

            throw new ArgumentException("矩阵列数不一致。");



        if (typeof(T) == typeof(double))

        {

            var ad = (double[,])(object)a;

            var bd = (double[,])(object)b;

            Span<double> scratchA = cols <= MaxStackScratch ? stackalloc double[cols] : new double[cols];

            Span<double> scratchB = cols <= MaxStackScratch ? stackalloc double[cols] : new double[cols];

            for (int j = 0; j < cols; j++)

            {

                scratchA[j] = ad[rowA, j];

                scratchB[j] = bd[rowB, j];

            }

            return T.CreateTruncating(SquaredDistanceDouble(scratchA, scratchB));

        }



        if (typeof(T) == typeof(float))

        {

            var af = (float[,])(object)a;

            var bf = (float[,])(object)b;

            Span<float> scratchA = cols <= MaxStackScratch ? stackalloc float[cols] : new float[cols];

            Span<float> scratchB = cols <= MaxStackScratch ? stackalloc float[cols] : new float[cols];

            for (int j = 0; j < cols; j++)

            {

                scratchA[j] = af[rowA, j];

                scratchB[j] = bf[rowB, j];

            }

            return T.CreateTruncating(SquaredDistanceFloat(scratchA, scratchB));

        }



        T sum = T.Zero;

        for (int j = 0; j < cols; j++)

        {

            T diff = a[rowA, j] - b[rowB, j];

            sum += diff * diff;

        }

        return sum;

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static T DotRow<T>(T[,] matrix, int row, ReadOnlySpan<T> weights)

        where T : struct, IFloatingPointIeee754<T>

    {

        int cols = weights.Length;



        if (typeof(T) == typeof(double))

        {

            var md = (double[,])(object)matrix;

            Span<double> rowScratch = cols <= MaxStackScratch ? stackalloc double[cols] : new double[cols];

            for (int j = 0; j < cols; j++)

                rowScratch[j] = md[row, j];

            double sum = TensorPrimitives.Dot(rowScratch, MemoryMarshal.Cast<T, double>(weights));

            return T.CreateTruncating(sum);

        }



        if (typeof(T) == typeof(float))

        {

            var mf = (float[,])(object)matrix;

            Span<float> rowScratch = cols <= MaxStackScratch ? stackalloc float[cols] : new float[cols];

            for (int j = 0; j < cols; j++)

                rowScratch[j] = mf[row, j];

            float sum = TensorPrimitives.Dot(rowScratch, MemoryMarshal.Cast<T, float>(weights));

            return T.CreateTruncating(sum);

        }



        T scalar = T.Zero;

        for (int j = 0; j < cols; j++)

            scalar += matrix[row, j] * weights[j];

        return scalar;

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static void AddScaled<T>(Span<T> destination, ReadOnlySpan<T> source, T scale)

        where T : struct, IFloatingPointIeee754<T>

    {

        if (destination.Length != source.Length)

            throw new ArgumentException("向量长度不一致。");



        for (int i = 0; i < destination.Length; i++)

            destination[i] += scale * source[i];

    }



    private static double SquaredDistanceDouble(ReadOnlySpan<double> a, ReadOnlySpan<double> b)

    {

        int n = a.Length;

        Span<double> diff = n <= MaxStackScratch ? stackalloc double[n] : new double[n];

        TensorPrimitives.Subtract(a, b, diff);

        return TensorPrimitives.Dot(diff, diff);

    }



    private static float SquaredDistanceFloat(ReadOnlySpan<float> a, ReadOnlySpan<float> b)

    {

        int n = a.Length;

        Span<float> diff = n <= MaxStackScratch ? stackalloc float[n] : new float[n];

        TensorPrimitives.Subtract(a, b, diff);

        return TensorPrimitives.Dot(diff, diff);

    }

}


