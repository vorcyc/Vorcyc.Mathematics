namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics;

/// <summary>
/// Tensor layout utilities for NHWC (batch × height × width × channels) storage.
/// </summary>
public static class TensorOps
{
    private const int GemmParallelRowThreshold = 64;

    /// <summary>
    /// Same-origin padding used by batch convolution in this library.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvolutionSamePadding(int kernelSize, int dilation)
        => (kernelSize * dilation + dilation - 1) / 2;

    /// <summary>
    /// Output spatial size for stride/dilation convolution with same padding.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvolutionOutputSize(int inputSize, int kernelSize, int stride, int dilation)
        => (inputSize + stride - 1) / stride;

    /// <summary>
    /// Unfolds an NHWC tensor into a column matrix of shape (numPatches × patchSize).
    /// </summary>
    public static void Im2ColNhwc<T>(
        ReadOnlySpan<T> input,
        int batch,
        int inHeight,
        int inWidth,
        int inChannels,
        int kernelSize,
        int stride,
        int dilation,
        int outHeight,
        int outWidth,
        Span<T> columns)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        int patchSize = kernelSize * kernelSize * inChannels;
        int numPatches = batch * outHeight * outWidth;
        if (columns.Length < numPatches * patchSize)
            throw new ArgumentException("Column buffer is too small.", nameof(columns));

        int pad = ConvolutionSamePadding(kernelSize, dilation);
        int patchIndex = 0;

        for (int n = 0; n < batch; n++)
        {
            for (int ay = 0; ay < outHeight; ay++)
            {
                int y = ay * stride - pad;
                for (int ax = 0; ax < outWidth; ax++)
                {
                    int x = ax * stride - pad;
                    var patch = columns.Slice(patchIndex * patchSize, patchSize);
                    patch.Clear();
                    int k = 0;

                    for (int fy = 0; fy < kernelSize; fy++)
                    {
                        int oy = y + fy * dilation + dilation - 1;
                        for (int fx = 0; fx < kernelSize; fx++)
                        {
                            int ox = x + fx * dilation + dilation - 1;
                            if ((uint)oy < (uint)inHeight && (uint)ox < (uint)inWidth)
                            {
                                int ti = ((n * inHeight + oy) * inWidth + ox) * inChannels;
                                input.Slice(ti, inChannels).CopyTo(patch.Slice(k, inChannels));
                            }

                            k += inChannels;
                        }
                    }

                    patchIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Scatters a column matrix back into an NHWC gradient tensor (accumulates).
    /// </summary>
    public static void Col2ImNhwc<T>(
        ReadOnlySpan<T> columns,
        int batch,
        int inHeight,
        int inWidth,
        int inChannels,
        int kernelSize,
        int stride,
        int dilation,
        int outHeight,
        int outWidth,
        Span<T> gradInput)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        int patchSize = kernelSize * kernelSize * inChannels;
        int numPatches = batch * outHeight * outWidth;
        if (columns.Length < numPatches * patchSize)
            throw new ArgumentException("Column buffer is too small.", nameof(columns));
        if (gradInput.Length < batch * inHeight * inWidth * inChannels)
            throw new ArgumentException("Gradient buffer is too small.", nameof(gradInput));

        int pad = ConvolutionSamePadding(kernelSize, dilation);
        int patchIndex = 0;

        for (int n = 0; n < batch; n++)
        {
            for (int ay = 0; ay < outHeight; ay++)
            {
                int y = ay * stride - pad;
                for (int ax = 0; ax < outWidth; ax++)
                {
                    int x = ax * stride - pad;
                    var patch = columns.Slice(patchIndex * patchSize, patchSize);
                    int k = 0;

                    for (int fy = 0; fy < kernelSize; fy++)
                    {
                        int oy = y + fy * dilation + dilation - 1;
                        for (int fx = 0; fx < kernelSize; fx++)
                        {
                            int ox = x + fx * dilation + dilation - 1;
                            if ((uint)oy < (uint)inHeight && (uint)ox < (uint)inWidth)
                            {
                                int ti = ((n * inHeight + oy) * inWidth + ox) * inChannels;
                                for (int c = 0; c < inChannels; c++)
                                    gradInput[ti + c] += patch[k + c];
                            }

                            k += inChannels;
                        }
                    }

                    patchIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Computes C = A·Bᵀ where A is M×K and B is N×K (row-major).
    /// </summary>
    public static void GemmTransposeRight<T>(
        ReadOnlySpan<T> a,
        int m,
        int k,
        ReadOnlySpan<T> b,
        int n,
        Span<T> output)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (a.Length < m * k || b.Length < n * k || output.Length < m * n)
            throw new ArgumentException("Matrix buffer sizes are invalid.");

        for (int row = 0; row < m; row++)
            GemmTransposeRightRow(a, b, output, row, k, n);
    }

    /// <summary>
    /// Array-backed GEMM with optional row-parallel execution for large patch counts.
    /// </summary>
    public static void GemmTransposeRight<T>(
        T[] a,
        int aOffset,
        int m,
        int k,
        T[] b,
        int bOffset,
        int n,
        T[] output,
        int outputOffset,
        ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (a.Length - aOffset < m * k || b.Length - bOffset < n * k || output.Length - outputOffset < m * n)
            throw new ArgumentException("Matrix buffer sizes are invalid.");

        long workPerRow = (long)n * k;
        if (m >= GemmParallelRowThreshold
            && ComputingContextExecution.UseParallelIndexed(context, m, workPerRow))
        {
            ComputingContextExecution.ForEach(context, 0, m, row =>
                GemmTransposeRightRowArray(a, aOffset, b, bOffset, output, outputOffset, row, k, n, context),
                workPerRow);
            return;
        }

        for (int row = 0; row < m; row++)
            GemmTransposeRightRowArray(a, aOffset, b, bOffset, output, outputOffset, row, k, n, context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GemmTransposeRightRow<T>(
        ReadOnlySpan<T> a,
        ReadOnlySpan<T> b,
        Span<T> output,
        int row,
        int k,
        int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        ReadOnlySpan<T> aRow = a.Slice(row * k, k);
        int outBase = row * n;
        for (int col = 0; col < n; col++)
            output[outBase + col] = VectorSpan.Dot(aRow, b.Slice(col * k, k));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GemmTransposeRightRowArray<T>(
        T[] a,
        int aOffset,
        T[] b,
        int bOffset,
        T[] output,
        int outputOffset,
        int row,
        int k,
        int n,
        ComputingContext? context = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        ReadOnlySpan<T> aRow = a.AsSpan(aOffset + row * k, k);
        int outRow = outputOffset + row * n;
        for (int col = 0; col < n; col++)
            output[outRow + col] = VectorSpan.Dot(aRow, b.AsSpan(bOffset + col * k, k), context);
    }
}
