using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Axis reductions and normalization for tensor types.
/// </summary>
public static class TensorStatistics
{
    /// <summary>Axis index for <see cref="Tensor4D{T}"/> reductions.</summary>
    public enum Tensor4DAxis
    {
        Dim0 = 0,
        Dim1 = 1,
        Dim2 = 2,
        Dim3 = 3
    }

    /// <summary>Mean along one axis of a 4-D tensor.</summary>
    public static T[] MeanAlongAxis<T>(Tensor4D<T> tensor, Tensor4DAxis axis, ComputingContext? context = null)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var shape = tensor.Shape;
        int outer = GetOuterCount(shape, (int)axis);
        int inner = GetAxisLength(shape, (int)axis);
        var result = new T[outer];

        for (int o = 0; o < outer; o++)
        {
            var buffer = new T[inner];
            GatherAlongAxis(tensor, (int)axis, o, buffer);
            result[o] = buffer.AsSpan().Average(context);
        }

        return result;
    }

    /// <summary>Standard deviation along one axis of a 4-D tensor.</summary>
    public static T[] StandardDeviationAlongAxis<T>(Tensor4D<T> tensor, Tensor4DAxis axis, ComputingContext? context = null)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var shape = tensor.Shape;
        int outer = GetOuterCount(shape, (int)axis);
        int inner = GetAxisLength(shape, (int)axis);
        var result = new T[outer];

        for (int o = 0; o < outer; o++)
        {
            var buffer = new T[inner];
            GatherAlongAxis(tensor, (int)axis, o, buffer);
            result[o] = buffer.AsSpan().StandardDeviation(context);
        }

        return result;
    }

    /// <summary>In-place z-score normalization along one axis.</summary>
    public static void NormalizeAlongAxis<T>(Tensor4D<T> tensor, Tensor4DAxis axis, T epsilon = default, ComputingContext? context = null)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (epsilon == default)
            epsilon = T.CreateChecked(1e-8);

        var shape = tensor.Shape;
        int outer = GetOuterCount(shape, (int)axis);
        int inner = GetAxisLength(shape, (int)axis);
        var buffer = new T[inner];

        for (int o = 0; o < outer; o++)
        {
            GatherAlongAxis(tensor, (int)axis, o, buffer);
            var span = buffer.AsSpan();
            T mean = span.Average(context);
            T std = span.StandardDeviation(context);
            if (std == T.Zero)
                std = epsilon;

            for (int i = 0; i < inner; i++)
                buffer[i] = (buffer[i] - mean) / std;

            ScatterAlongAxis(tensor, (int)axis, o, buffer);
        }
    }

    /// <summary>Mean along depth of a 3-D <see cref="Tensor{T}"/>.</summary>
    public static T[] MeanAlongDepth<T>(Tensor<T> tensor, ComputingContext? context = null)
        where T : IBinaryFloatingPointIeee754<T>
    {
        int planes = tensor.Width * tensor.Height;
        var result = new T[planes];
        var slice = new T[tensor.Depth];

        for (int h = 0; h < tensor.Height; h++)
        {
            for (int w = 0; w < tensor.Width; w++)
            {
                for (int d = 0; d < tensor.Depth; d++)
                    slice[d] = tensor[w, h, d];

                result[h * tensor.Width + w] = slice.AsSpan().Average(context);
            }
        }

        return result;
    }

    private static int GetAxisLength(Tensor4DShape shape, int axis) => axis switch
    {
        0 => shape.Dim0,
        1 => shape.Dim1,
        2 => shape.Dim2,
        3 => shape.Dim3,
        _ => throw new ArgumentOutOfRangeException(nameof(axis))
    };

    private static int GetOuterCount(Tensor4DShape shape, int axis)
    {
        int count = 1;
        for (int a = 0; a < 4; a++)
        {
            if (a != axis)
                count *= GetAxisLength(shape, a);
        }

        return count;
    }

    private static void GatherAlongAxis<T>(Tensor4D<T> tensor, int axis, int outerIndex, Span<T> destination)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var shape = tensor.Shape;
        int[] dims = [shape.Dim0, shape.Dim1, shape.Dim2, shape.Dim3];
        int[] coords = new int[4];
        DecodeOuterIndex(outerIndex, axis, dims, coords);

        for (int i = 0; i < destination.Length; i++)
        {
            coords[axis] = i;
            destination[i] = tensor[coords[0], coords[1], coords[2], coords[3]];
        }
    }

    private static void DecodeOuterIndex(int outerIndex, int axis, ReadOnlySpan<int> dims, Span<int> coords)
    {
        int remaining = outerIndex;
        for (int a = 3; a >= 0; a--)
        {
            if (a == axis)
                continue;
            coords[a] = remaining % dims[a];
            remaining /= dims[a];
        }
    }

    private static void ScatterAlongAxis<T>(Tensor4D<T> tensor, int axis, int outerIndex, ReadOnlySpan<T> source)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var shape = tensor.Shape;
        int[] dims = [shape.Dim0, shape.Dim1, shape.Dim2, shape.Dim3];
        int[] coords = new int[4];
        DecodeOuterIndex(outerIndex, axis, dims, coords);

        for (int i = 0; i < source.Length; i++)
        {
            coords[axis] = i;
            tensor[coords[0], coords[1], coords[2], coords[3]] = source[i];
        }
    }
}
