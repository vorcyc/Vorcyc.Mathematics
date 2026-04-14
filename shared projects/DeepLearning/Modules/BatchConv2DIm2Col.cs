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

        int outChannels)

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



        for (int p = 0; p < numPatches; p++)

        {

            for (int d = 0; d < outChannels; d++)

            {

                var grad = gradOut[gradOutOffset + p * outChannels + d];

                bias.Gradient.Values[d] += grad;



                int colBase = p * patchSize;

                int filterBase = d * patchSize;

                for (int k = 0; k < patchSize; k++)

                {

                    filters[d].Gradient.Values[k] += grad * col[colBase + k];

                    gradCol[colBase + k] += grad * filterFlat[filterBase + k];

                }

            }

        }



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


