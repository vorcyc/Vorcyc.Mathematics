namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics;

/// <summary>
/// Native NHWC batch convolution with cross-sample parallelism and SIMD channel MAC.
/// </summary>
internal static class BatchConv2DMath
{
    public static void Forward<T>(
        Memory<T> inputMemory,
        int batch,
        int inHeight,
        int inWidth,
        int inChannels,
        ReadOnlySpan<Parameter<T>> filters,
        ReadOnlySpan<T> bias,
        int kernelSize,
        int stride,
        int dilation,
        Memory<T> outputMemory,
        int outHeight,
        int outWidth,
        int outChannels,
        ComputingContext? context = null)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (!MemoryMarshal.TryGetArray(inputMemory, out ArraySegment<T> inputSegment)
            || !MemoryMarshal.TryGetArray(outputMemory, out ArraySegment<T> outputSegment)
            || inputSegment.Array is null
            || outputSegment.Array is null)
        {
            throw new InvalidOperationException("Batch convolution requires array-backed tensors.");
        }

        var input = inputSegment.Array;
        var output = outputSegment.Array;
        var inputOffset = inputSegment.Offset;
        var outputOffset = outputSegment.Offset;
        var pad = (kernelSize * dilation + dilation - 1) / 2;
        var filterArray = filters.ToArray();
        var biasArray = bias.ToArray();

        long workPerSample = (long)outHeight * outWidth * outChannels * kernelSize * kernelSize * inChannels;
        ComputingContextExecution.ForEach(context, 0, batch, n =>
        {
            ForwardSample(
                input,
                inputOffset,
                n,
                inHeight,
                inWidth,
                inChannels,
                filterArray,
                biasArray,
                kernelSize,
                stride,
                dilation,
                pad,
                output,
                outputOffset,
                batch,
                outHeight,
                outWidth,
                outChannels);
        }, workPerSample);
    }

    public static void Backward<T>(
        Memory<T> inputMemory,
        Memory<T> gradOutputMemory,
        int batch,
        int inHeight,
        int inWidth,
        int inChannels,
        Parameter<T>[] filters,
        Parameter<T> bias,
        int kernelSize,
        int stride,
        int dilation,
        Memory<T> gradInputMemory,
        int outHeight,
        int outWidth,
        int outChannels,
        ComputingContext? context = null)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (!MemoryMarshal.TryGetArray(inputMemory, out ArraySegment<T> inputSegment)
            || !MemoryMarshal.TryGetArray(gradOutputMemory, out ArraySegment<T> gradOutSegment)
            || !MemoryMarshal.TryGetArray(gradInputMemory, out ArraySegment<T> gradInSegment)
            || inputSegment.Array is null
            || gradOutSegment.Array is null
            || gradInSegment.Array is null)
        {
            throw new InvalidOperationException("Batch convolution requires array-backed tensors.");
        }

        var input = inputSegment.Array;
        var gradOutput = gradOutSegment.Array;
        var gradInput = gradInSegment.Array;
        var inputOffset = inputSegment.Offset;
        var gradOutOffset = gradOutSegment.Offset;
        var gradInOffset = gradInSegment.Offset;
        var pad = (kernelSize * dilation + dilation - 1) / 2;

        gradInput.AsSpan(gradInOffset, gradInSegment.Count).Clear();

        // Backward is split into two race-free parallel kernels so it honors the
        // ComputingContext just like Forward:
        //   (1) gradInput — parallel over samples n; sample n writes only its own
        //       gradInput region, so no two threads touch the same element.
        //   (2) weight/bias grads — parallel over output channels d; channel d writes
        //       only filters[d].Gradient and biasGrad[d], which are disjoint per d.
        // input/gradOutput are read-only in both kernels. filters is passed as an array
        // (not a span) so it can be captured by the parallel closures without copying.
        var filtersArray = filters;

        long gradInputWork = (long)outHeight * outWidth * outChannels * kernelSize * kernelSize * inChannels;
        ComputingContextExecution.ForEach(context, 0, batch, n =>
        {
            BackwardGradInputSample(
                gradOutput,
                gradOutOffset,
                n,
                inHeight,
                inWidth,
                inChannels,
                filtersArray,
                kernelSize,
                stride,
                dilation,
                pad,
                gradInput,
                gradInOffset,
                inputOffset,
                batch,
                outHeight,
                outWidth,
                outChannels);
        }, gradInputWork);

        long weightWork = (long)batch * outHeight * outWidth * kernelSize * kernelSize * inChannels;
        ComputingContextExecution.ForEach(context, 0, outChannels, d =>
        {
            BackwardWeightChannel(
                input,
                inputOffset,
                gradOutput,
                gradOutOffset,
                d,
                inHeight,
                inWidth,
                inChannels,
                filtersArray,
                bias,
                kernelSize,
                stride,
                dilation,
                pad,
                batch,
                outHeight,
                outWidth,
                outChannels);
        }, weightWork);
    }

    private static void ForwardSample<T>(
        T[] input,
        int inputOffset,
        int n,
        int inHeight,
        int inWidth,
        int inChannels,
        Parameter<T>[] filters,
        T[] bias,
        int kernelSize,
        int stride,
        int dilation,
        int pad,
        T[] output,
        int outputOffset,
        int batch,
        int outHeight,
        int outWidth,
        int outChannels)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        int vectorSize = Vector<T>.Count;

        for (int d = 0; d < outChannels; d++)
        {
            var filter = filters[d].Value;
            var filterSpan = filter.Values;
            var biasValue = bias[d];

            for (int ay = 0; ay < outHeight; ay++)
            {
                var y = ay * stride - pad;
                for (int ax = 0; ax < outWidth; ax++)
                {
                    var x = ax * stride - pad;
                    T acc = T.Zero;

                    for (int fy = 0; fy < kernelSize; fy++)
                    {
                        var oy = y + fy * dilation + dilation - 1;
                        if (oy < 0 || oy >= inHeight)
                        {
                            continue;
                        }

                        for (int fx = 0; fx < kernelSize; fx++)
                        {
                            var ox = x + fx * dilation + dilation - 1;
                            if (ox < 0 || ox >= inWidth)
                            {
                                continue;
                            }

                            var fi = ((filter.Width * fy) + fx) * filter.Depth;
                            var ti = inputOffset + GetInputIndex(n, oy, ox, 0, batch, inHeight, inWidth, inChannels);

                            if (vectorSize > 1 && inChannels >= vectorSize)
                            {
                                int fd = 0;
                                for (; fd <= inChannels - vectorSize; fd += vectorSize)
                                {
                                    var wVec = new Vector<T>(filterSpan.Slice(fi + fd, vectorSize));
                                    var inVec = new Vector<T>(input.AsSpan(ti + fd, vectorSize));
                                    acc += Vector.Dot(wVec, inVec);
                                }

                                for (; fd < inChannels; fd++)
                                {
                                    acc += filterSpan[fi + fd] * input[ti + fd];
                                }
                            }
                            else
                            {
                                BatchNormMath.AccumulateDotSimd(
                                    filterSpan.Slice(fi, inChannels),
                                    input.AsSpan(ti, inChannels),
                                    ref acc);
                            }
                        }
                    }

                    output[outputOffset + GetOutputIndex(n, ay, ax, d, batch, outHeight, outWidth, outChannels)] = acc + biasValue;
                }
            }
        }
    }

    /// <summary>
    /// Computes gradInput for a single sample n. Writes only into sample n's gradInput
    /// region, so this is safe to run in parallel across samples.
    /// </summary>
    private static void BackwardGradInputSample<T>(
        T[] gradOutput,
        int gradOutOffset,
        int n,
        int inHeight,
        int inWidth,
        int inChannels,
        Parameter<T>[] filters,
        int kernelSize,
        int stride,
        int dilation,
        int pad,
        T[] gradInput,
        int gradInOffset,
        int inputOffset,
        int batch,
        int outHeight,
        int outWidth,
        int outChannels)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        for (int d = 0; d < outChannels; d++)
        {
            var filter = filters[d].Value;
            var filterSpan = filter.Values;

            for (int ay = 0; ay < outHeight; ay++)
            {
                var y = ay * stride - pad;
                for (int ax = 0; ax < outWidth; ax++)
                {
                    var x = ax * stride - pad;
                    var gradOut = gradOutput[gradOutOffset + GetOutputIndex(n, ay, ax, d, batch, outHeight, outWidth, outChannels)];

                    for (int fy = 0; fy < kernelSize; fy++)
                    {
                        var oy = y + fy * dilation + dilation - 1;
                        if (oy < 0 || oy >= inHeight)
                        {
                            continue;
                        }

                        for (int fx = 0; fx < kernelSize; fx++)
                        {
                            var ox = x + fx * dilation + dilation - 1;
                            if (ox < 0 || ox >= inWidth)
                            {
                                continue;
                            }

                            var fi = ((filter.Width * fy) + fx) * filter.Depth;
                            var ti = inputOffset + GetInputIndex(n, oy, ox, 0, batch, inHeight, inWidth, inChannels);

                            for (int fd = 0; fd < inChannels; fd++)
                            {
                                gradInput[gradInOffset + ti + fd - inputOffset] += gradOut * filterSpan[fi + fd];
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Computes filter and bias gradients for a single output channel d, accumulated
    /// over the whole batch. Writes only into filters[d].Gradient and biasGrad[d],
    /// so this is safe to run in parallel across output channels.
    /// </summary>
    private static void BackwardWeightChannel<T>(
        T[] input,
        int inputOffset,
        T[] gradOutput,
        int gradOutOffset,
        int d,
        int inHeight,
        int inWidth,
        int inChannels,
        Parameter<T>[] filters,
        Parameter<T> bias,
        int kernelSize,
        int stride,
        int dilation,
        int pad,
        int batch,
        int outHeight,
        int outWidth,
        int outChannels)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var filter = filters[d].Value;
        var filterGrad = filters[d].Gradient.Values;
        var biasGrad = bias.Gradient.Values;
        T biasAcc = T.Zero;

        for (int n = 0; n < batch; n++)
        {
            for (int ay = 0; ay < outHeight; ay++)
            {
                var y = ay * stride - pad;
                for (int ax = 0; ax < outWidth; ax++)
                {
                    var x = ax * stride - pad;
                    var gradOut = gradOutput[gradOutOffset + GetOutputIndex(n, ay, ax, d, batch, outHeight, outWidth, outChannels)];
                    biasAcc += gradOut;

                    for (int fy = 0; fy < kernelSize; fy++)
                    {
                        var oy = y + fy * dilation + dilation - 1;
                        if (oy < 0 || oy >= inHeight)
                        {
                            continue;
                        }

                        for (int fx = 0; fx < kernelSize; fx++)
                        {
                            var ox = x + fx * dilation + dilation - 1;
                            if (ox < 0 || ox >= inWidth)
                            {
                                continue;
                            }

                            var fi = ((filter.Width * fy) + fx) * filter.Depth;
                            var ti = inputOffset + GetInputIndex(n, oy, ox, 0, batch, inHeight, inWidth, inChannels);

                            for (int fd = 0; fd < inChannels; fd++)
                            {
                                filterGrad[fi + fd] += gradOut * input[ti + fd];
                            }
                        }
                    }
                }
            }
        }

        biasGrad[d] += biasAcc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetInputIndex(int n, int h, int w, int c, int batch, int height, int width, int channels)
        => ((n * height + h) * width + w) * channels + c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetOutputIndex(int n, int h, int w, int c, int batch, int height, int width, int channels)
        => ((n * height + h) * width + w) * channels + c;
}
