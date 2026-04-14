namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A 4-D tensor stored in N×H×W×C (NHWC) memory layout.
/// </summary>
/// <remarks>
/// Backed by <see cref="Tensor4D{T}"/> with axis mapping:
/// Batch → Dim0, Height → Dim1, Width → Dim2, Channels → Dim3.
/// </remarks>
/// <typeparam name="T">Element type.</typeparam>
public sealed class BatchTensor<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Tensor4D<T> _tensor;

    /// <summary>
    /// Creates an uninitialized batch tensor.
    /// </summary>
    public BatchTensor(int batch, int height, int width, int channels)
    {
        _tensor = new Tensor4D<T>(batch, height, width, channels);
    }

    private BatchTensor(Tensor4D<T> tensor) => _tensor = tensor;

    /// <summary>Gets the batch size N.</summary>
    public int Batch => _tensor.Dim0;

    /// <summary>Gets the height H.</summary>
    public int Height => _tensor.Dim1;

    /// <summary>Gets the width W.</summary>
    public int Width => _tensor.Dim2;

    /// <summary>Gets the channel count C.</summary>
    public int Channels => _tensor.Dim3;

    /// <summary>Gets the number of elements in one batch slice (H×W×C).</summary>
    public int SampleSliceLength => Height * Width * Channels;

    /// <summary>Gets the underlying contiguous values.</summary>
    public Span<T> Values => _tensor.Values;

    /// <summary>Gets the underlying memory for zero-copy kernel access.</summary>
    internal Memory<T> Buffer => _tensor.Buffer;

    /// <summary>Gets the tensor shape metadata.</summary>
    public BatchShape Shape => new(Batch, Height, Width, Channels);

    /// <summary>Gets the layout-neutral 4-D storage.</summary>
    public Tensor4D<T> AsTensor4D() => _tensor;

    /// <summary>
    /// Gets or sets an element in NHWC order.
    /// </summary>
    public T this[int n, int h, int w, int c]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _tensor[n, h, w, c];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _tensor[n, h, w, c] = value;
    }

    /// <summary>
    /// Converts a single batch item to the legacy W×H×C <see cref="Tensor{T}"/> layout.
    /// </summary>
    public Tensor<T> GetSample(int batchIndex)
    {
        if ((uint)batchIndex >= (uint)Batch)
        {
            throw new ArgumentOutOfRangeException(nameof(batchIndex));
        }

        var sample = new Tensor<T>(Width, Height, Channels);
        Values.Slice(batchIndex * SampleSliceLength, SampleSliceLength).CopyTo(sample.Values);
        return sample;
    }

    /// <summary>
    /// Writes a legacy W×H×C tensor into one batch slice.
    /// </summary>
    public void SetSample(int batchIndex, Tensor<T> sample)
    {
        if ((uint)batchIndex >= (uint)Batch)
        {
            throw new ArgumentOutOfRangeException(nameof(batchIndex));
        }

        if (sample.Width != Width || sample.Height != Height || sample.Depth != Channels)
        {
            throw new ArgumentException("Sample shape does not match batch tensor shape.", nameof(sample));
        }

        sample.Values.CopyTo(Values.Slice(batchIndex * SampleSliceLength, SampleSliceLength));
    }

    /// <summary>
    /// Maps a batched vector N×1×1×F to the legacy 1×N×F tensor layout.
    /// </summary>
    public Tensor<T> ToFeatureTensor()
    {
        if (Height != 1 || Width != 1)
        {
            throw new InvalidOperationException("ToFeatureTensor requires shape N×1×1×F.");
        }

        var tensor = new Tensor<T>(1, Batch, Channels);
        Values.CopyTo(tensor.Values);
        return tensor;
    }

    /// <summary>
    /// Creates a batch tensor from a legacy 1×N×F feature tensor.
    /// </summary>
    public static BatchTensor<T> FromFeatureTensor(Tensor<T> tensor)
    {
        if (tensor.Width != 1)
        {
            throw new ArgumentException("Expected feature tensor width 1.", nameof(tensor));
        }

        var batch = new BatchTensor<T>(tensor.Height, 1, 1, tensor.Depth);
        tensor.Values.CopyTo(batch.Values);
        return batch;
    }

    /// <summary>
    /// Creates a batch tensor from an array of legacy sample tensors.
    /// </summary>
    public static BatchTensor<T> FromSamples(ReadOnlySpan<Tensor<T>> samples)
    {
        if (samples.Length == 0)
        {
            throw new ArgumentException("At least one sample is required.", nameof(samples));
        }

        var first = samples[0];
        var batch = new BatchTensor<T>(samples.Length, first.Height, first.Width, first.Depth);
        for (int i = 0; i < samples.Length; i++)
        {
            batch.SetSample(i, samples[i]);
        }

        return batch;
    }

    /// <summary>
    /// Creates a batch tensor that wraps existing 4-D storage.
    /// </summary>
    public static BatchTensor<T> FromTensor4D(Tensor4D<T> tensor) => new(tensor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int GetIndex(int n, int h, int w, int c)
        => _tensor.GetIndex(n, h, w, c);

    /// <summary>
    /// Gets a contiguous span for one channel across all N×H×W positions.
    /// </summary>
    internal void CopyChannelTo(int channel, Span<T> destination)
    {
        int required = Batch * Height * Width;
        if (destination.Length < required)
        {
            throw new ArgumentException("Destination span is too small.", nameof(destination));
        }

        if ((uint)channel >= (uint)Channels)
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        ReadOnlySpan<T> src = Values;
        if (Channels == 1)
        {
            src.CopyTo(destination.Slice(0, required));
            return;
        }

        int planeStride = SampleSliceLength;
        int rowStride = Width * Channels;
        int dest = 0;
        for (int n = 0; n < Batch; n++)
        {
            int batchBase = n * planeStride;
            for (int h = 0; h < Height; h++)
            {
                int rowBase = batchBase + h * rowStride + channel;
                int w = 0;
                int widthLimit = Width - 3;
                for (; w < widthLimit; w += 4)
                {
                    int baseIndex = rowBase + w * Channels;
                    destination[dest++] = src[baseIndex];
                    destination[dest++] = src[baseIndex + Channels];
                    destination[dest++] = src[baseIndex + Channels * 2];
                    destination[dest++] = src[baseIndex + Channels * 3];
                }

                for (; w < Width; w++)
                    destination[dest++] = src[rowBase + w * Channels];
            }
        }
    }

    /// <summary>
    /// Writes a contiguous channel span back into the batch tensor.
    /// </summary>
    internal void CopyChannelFrom(int channel, ReadOnlySpan<T> source)
    {
        int required = Batch * Height * Width;
        if (source.Length < required)
        {
            throw new ArgumentException("Source span is too small.", nameof(source));
        }

        if ((uint)channel >= (uint)Channels)
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        Span<T> dest = Values;
        if (Channels == 1)
        {
            source.Slice(0, required).CopyTo(dest);
            return;
        }

        int planeStride = SampleSliceLength;
        int rowStride = Width * Channels;
        int srcIndex = 0;
        for (int n = 0; n < Batch; n++)
        {
            int batchBase = n * planeStride;
            for (int h = 0; h < Height; h++)
            {
                int rowBase = batchBase + h * rowStride + channel;
                int w = 0;
                int widthLimit = Width - 3;
                for (; w < widthLimit; w += 4)
                {
                    int baseIndex = rowBase + w * Channels;
                    dest[baseIndex] = source[srcIndex++];
                    dest[baseIndex + Channels] = source[srcIndex++];
                    dest[baseIndex + Channels * 2] = source[srcIndex++];
                    dest[baseIndex + Channels * 3] = source[srcIndex++];
                }

                for (; w < Width; w++)
                    dest[rowBase + w * Channels] = source[srcIndex++];
            }
        }
    }
}
