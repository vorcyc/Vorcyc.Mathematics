namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Helper methods for constructing tensors used in training pipelines.
/// </summary>
public static class TensorUtilities
{
    /// <summary>
    /// Creates a 1×1×N vector tensor from a span of values.
    /// </summary>
    public static Tensor<T> FromVector<T>(ReadOnlySpan<T> values)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var tensor = new Tensor<T>(1, 1, values.Length);
        values.CopyTo(tensor.Values);
        return tensor;
    }

    /// <summary>
    /// Creates a 1×1×N vector tensor from an array of values.
    /// </summary>
    public static Tensor<T> FromVector<T>(params T[] values)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => FromVector((ReadOnlySpan<T>)values);

    /// <summary>
    /// Creates a batched vector tensor 1×N×F from rows of feature values.
    /// </summary>
    public static Tensor<T> FromBatchVectors<T>(ReadOnlySpan<T> values, int batchSize, int features)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (values.Length != batchSize * features)
        {
            throw new ArgumentException("Value count must equal batchSize * features.", nameof(values));
        }

        var tensor = new Tensor<T>(1, batchSize, features);
        values.CopyTo(tensor.Values);
        return tensor;
    }

    /// <summary>
    /// Stacks single-sample 1×1×F tensors into one 1×N×F batch tensor.
    /// </summary>
    public static Tensor<T> StackBatch<T>(ReadOnlySpan<Tensor<T>> samples)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (samples.Length == 0)
        {
            throw new ArgumentException("At least one sample is required.", nameof(samples));
        }

        int features = samples[0].Depth;
        var batch = new Tensor<T>(1, samples.Length, features);
        for (int i = 0; i < samples.Length; i++)
        {
            var sample = samples[i];
            if (sample.Width != 1 || sample.Height != 1 || sample.Depth != features)
            {
                throw new ArgumentException("All samples must have shape 1×1×F.", nameof(samples));
            }

            for (int f = 0; f < features; f++)
            {
                batch[0, i, f] = sample[0, 0, f];
            }
        }

        return batch;
    }

    /// <summary>
    /// Creates a one-hot target tensor for categorical training.
    /// </summary>
    public static Tensor<T> OneHot<T>(int classCount, int classIndex)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (classIndex < 0 || classIndex >= classCount)
        {
            throw new ArgumentOutOfRangeException(nameof(classIndex));
        }

        var tensor = new Tensor<T>(1, 1, classCount);
        tensor.Fill(T.Zero);
        tensor[0, 0, classIndex] = T.One;
        return tensor;
    }

    /// <summary>
    /// Creates a batched one-hot tensor 1×N×C.
    /// </summary>
    public static Tensor<T> OneHotBatch<T>(ReadOnlySpan<int> classIndices, int classCount)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var tensor = new Tensor<T>(1, classIndices.Length, classCount);
        tensor.Fill(T.Zero);
        for (int i = 0; i < classIndices.Length; i++)
        {
            int c = classIndices[i];
            if (c < 0 || c >= classCount)
            {
                throw new ArgumentOutOfRangeException(nameof(classIndices));
            }

            tensor[0, i, c] = T.One;
        }

        return tensor;
    }

    /// <summary>
    /// Fills a tensor with uniform random values in [-scale, scale].
    /// </summary>
    public static void FillUniformRandom<T>(Tensor<T> tensor, T scale, Random? random = null)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        random ??= Random.Shared;
        var span = tensor.Values;
        for (int i = 0; i < span.Length; i++)
        {
            var sample = (random.NextSingle() * 2f) - 1f;
            span[i] = scale * T.CreateTruncating(sample);
        }
    }
}
