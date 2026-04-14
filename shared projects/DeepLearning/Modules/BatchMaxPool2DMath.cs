namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics;

/// <summary>
/// Native NHWC 2×2 max pooling with stride 2 for batched tensors.
/// </summary>
internal static class BatchMaxPool2DMath
{
    public static void Forward<T>(
        Memory<T> inputMemory,
        int batch,
        int inHeight,
        int inWidth,
        int channels,
        Memory<T> outputMemory,
        int outHeight,
        int outWidth,
        int[] argmaxIndices,
        ComputingContext? context = null)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (!MemoryMarshal.TryGetArray(inputMemory, out ArraySegment<T> inputSegment)
            || !MemoryMarshal.TryGetArray(outputMemory, out ArraySegment<T> outputSegment)
            || inputSegment.Array is null
            || outputSegment.Array is null)
        {
            throw new InvalidOperationException("Batch max pool requires array-backed tensors.");
        }

        var input = inputSegment.Array;
        var output = outputSegment.Array;
        var inputOffset = inputSegment.Offset;
        var outputOffset = outputSegment.Offset;

        long workPerSample = (long)outHeight * outWidth * channels * 4;
        ComputingContextExecution.ForEach(context, 0, batch, n =>
        {
            for (int c = 0; c < channels; c++)
            {
                for (int ay = 0; ay < outHeight; ay++)
                {
                    var y = 2 * ay;
                    for (int ax = 0; ax < outWidth; ax++)
                    {
                        var x = 2 * ax;
                        T max = T.MinValue;
                        int bestIndex = 0;

                        for (int fy = 0; fy < 2; fy++)
                        {
                            var oy = y + fy;
                            if (oy >= inHeight)
                            {
                                continue;
                            }

                            for (int fx = 0; fx < 2; fx++)
                            {
                                var ox = x + fx;
                                if (ox >= inWidth)
                                {
                                    continue;
                                }

                                var flatIndex = inputOffset + GetInputIndex(n, oy, ox, c, batch, inHeight, inWidth, channels);
                                var v = input[flatIndex];
                                if (v > max)
                                {
                                    max = v;
                                    bestIndex = flatIndex;
                                }
                            }
                        }

                        var outIndex = outputOffset + GetOutputIndex(n, ay, ax, c, batch, outHeight, outWidth, channels);
                        output[outIndex] = max;
                        argmaxIndices[outIndex - outputOffset] = bestIndex;
                    }
                }
            }
        }, workPerSample);
    }

    public static void Backward<T>(
        Memory<T> gradOutputMemory,
        int batch,
        int outHeight,
        int outWidth,
        int channels,
        ReadOnlySpan<int> argmaxIndices,
        Memory<T> gradInputMemory)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (!MemoryMarshal.TryGetArray(gradOutputMemory, out ArraySegment<T> gradOutSegment)
            || !MemoryMarshal.TryGetArray(gradInputMemory, out ArraySegment<T> gradInSegment)
            || gradOutSegment.Array is null
            || gradInSegment.Array is null)
        {
            throw new InvalidOperationException("Batch max pool requires array-backed tensors.");
        }

        var gradOutput = gradOutSegment.Array;
        var gradInput = gradInSegment.Array;
        var gradOutOffset = gradOutSegment.Offset;
        var gradInOffset = gradInSegment.Offset;

        gradInput.AsSpan(gradInOffset, gradInSegment.Count).Clear();
        int length = batch * outHeight * outWidth * channels;
        for (int i = 0; i < length; i++)
        {
            gradInput[argmaxIndices[i]] += gradOutput[gradOutOffset + i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetInputIndex(int n, int h, int w, int c, int batch, int height, int width, int channels)
        => ((n * height + h) * width + w) * channels + c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetOutputIndex(int n, int h, int w, int c, int batch, int height, int width, int channels)
        => ((n * height + h) * width + w) * channels + c;
}
