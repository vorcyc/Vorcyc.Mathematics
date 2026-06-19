namespace Vorcyc.Mathematics.DeepLearning.Modules;
using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// im2col + GEMM batch convolution for larger kernels.
/// </summary>
internal static class BatchConv2DIm2Col
{
    /// <summary>Kernel sizes at or above this value use the im2col path.</summary>
    public const int KernelThreshold = 5;

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

        int patchSize = kernelSize * kernelSize * inChannels;
        int numPatches = batch * outHeight * outWidth;
        var col = new T[numPatches * patchSize];
        var filterFlat = new T[outChannels * patchSize];

        BuildFilterMatrix(filters, outChannels, patchSize, filterFlat);
        TensorOps.Im2ColNhwc(
            inputSegment.Array.AsSpan(inputSegment.Offset, inputSegment.Count),
            batch,
            inHeight,
            inWidth,
            inChannels,
            kernelSize,
            stride,
            dilation,
            outHeight,
            outWidth,
            col);

        var output = outputSegment.Array!;
        var outputOffset = outputSegment.Offset;
        TensorOps.GemmTransposeRight(
            col, 0,
            numPatches, patchSize,
            filterFlat, 0,
            outChannels,
            output, outputOffset,
            context);

        for (int p = 0; p < numPatches; p++)
        {
            int baseIndex = outputOffset + p * outChannels;
            for (int d = 0; d < outChannels; d++)
                output[baseIndex + d] += bias[d];
        }
    }

    public static void Backward<T>(
        Memory<T> inputMemory,
        Memory<T> gradOutputMemory,
        int batch,
        int inHeight,
        int inWidth,
        int inChannels,
        ReadOnlySpan<Parameter<T>> filters,
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

        int patchSize = kernelSize * kernelSize * inChannels;
        int numPatches = batch * outHeight * outWidth;
        var col = new T[numPatches * patchSize];
        var filterFlat = new T[outChannels * patchSize];
        var gradCol = new T[numPatches * patchSize];

        BuildFilterMatrix(filters, outChannels, patchSize, filterFlat);
        TensorOps.Im2ColNhwc(
            inputSegment.Array.AsSpan(inputSegment.Offset, inputSegment.Count),
            batch,
            inHeight,
            inWidth,
            inChannels,
            kernelSize,
            stride,
            dilation,
            outHeight,
            outWidth,
            col);

        var gradOut = gradOutSegment.Array;
        var gradOutOffset = gradOutSegment.Offset;
        gradInSegment.Array.AsSpan(gradInSegment.Offset, gradInSegment.Count).Clear();

        // Two race-free parallel kernels (mirrors BatchConv2DMath.Backward):
        //   (1) gradCol 鈥?parallel over patches p; patch p writes only gradCol[p*patchSize ..],
        //       a disjoint block per p.
        //   (2) weight/bias grads 鈥?parallel over output channels d; channel d writes only
        //       filters[d].Gradient and bias.Gradient[d], disjoint per d.
        // col/filterFlat/gradOut are read-only in both kernels.
        var filterParams = filters.ToArray();
        var biasParam = bias;
        ComputingContextExecution.ForEach(context, 0, numPatches, p =>
        {
            int colBase = p * patchSize;
            int gradOutBase = gradOutOffset + p * outChannels;
            for (int d = 0; d < outChannels; d++)
            {
                var grad = gradOut[gradOutBase + d];
                int filterBase = d * patchSize;
                for (int k = 0; k < patchSize; k++)
                {
                    gradCol[colBase + k] += grad * filterFlat[filterBase + k];
                }
            }
        }, (long)outChannels * patchSize);
        ComputingContextExecution.ForEach(context, 0, outChannels, d =>
        {
            var filterGrad = filterParams[d].Gradient.Values;
            int filterBase = d * patchSize;
            T biasAcc = T.Zero;
            for (int p = 0; p < numPatches; p++)
            {
                var grad = gradOut[gradOutOffset + p * outChannels + d];
                biasAcc += grad;
                int colBase = p * patchSize;
                for (int k = 0; k < patchSize; k++)
                {
                    filterGrad[k] += grad * col[colBase + k];
                }
            }
            biasParam.Gradient.Values[d] += biasAcc;
        }, (long)numPatches * patchSize);

        TensorOps.Col2ImNhwc(
            gradCol,
            batch,
            inHeight,
            inWidth,
            inChannels,
            kernelSize,
            stride,
            dilation,
            outHeight,
            outWidth,
            gradInSegment.Array.AsSpan(gradInSegment.Offset, gradInSegment.Count));
    }

    private static void BuildFilterMatrix<T>(
        ReadOnlySpan<Parameter<T>> filters,
        int outChannels,
        int patchSize,
        Span<T> destination)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        for (int d = 0; d < outChannels; d++)
            filters[d].Value.Values.CopyTo(destination.Slice(d * patchSize, patchSize));
    }
}
