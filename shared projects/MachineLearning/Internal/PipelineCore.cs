using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;

namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// Preprocessing chain logic shared by the classification/regression pipelines.
/// </summary>
internal static class PipelineCore<T>
    where T : struct, IFloatingPointIeee754<T>
{
    public static T[,] FitTransformChain(IReadOnlyList<IPreprocessor<T>> preprocessors, T[,] x)
    {
        var current = x;
        foreach (var preprocessor in preprocessors)
        {
            preprocessor.Fit(current);
            current = preprocessor.Transform(current);
        }
        return current;
    }

    /// <summary>
    /// Applies a matrix-level transform on the fitted preprocessing chain (inference batch path).
    /// </summary>
    public static T[,] TransformChain(IReadOnlyList<IPreprocessor<T>> preprocessors, T[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        if (preprocessors.Count == 0)
            return x;

        if (preprocessors.Count >= 2 && AllSupportTransformInto(preprocessors))
        {
            int rows = x.GetLength(0);
            int cols = x.GetLength(1);
            var bufferA = new T[rows, cols];
            var bufferB = new T[rows, cols];
            ((IMatrixTransformInto<T>)preprocessors[0]).TransformInto(x, bufferA);
            T[,] src = bufferA;
            T[,] dst = bufferB;
            for (int i = 1; i < preprocessors.Count; i++)
            {
                ((IMatrixTransformInto<T>)preprocessors[i]).TransformInto(src, dst);
                (src, dst) = (dst, src);
            }
            return src;
        }

        var current = x;
        foreach (var preprocessor in preprocessors)
            current = preprocessor.Transform(current);
        return current;
    }

    private static bool AllSupportTransformInto(IReadOnlyList<IPreprocessor<T>> preprocessors)
    {
        for (int i = 0; i < preprocessors.Count; i++)
        {
            if (preprocessors[i] is not IMatrixTransformInto<T>)
                return false;
        }
        return true;
    }

    public static T[] TransformSample(IReadOnlyList<IPreprocessor<T>> preprocessors, T[] sample)
    {
        var current = sample;
        foreach (var preprocessor in preprocessors)
            current = preprocessor.Transform(current);
        return current;
    }

    /// <summary>
    /// Applies the preprocessing chain to a single sample, writing the result into <paramref name="destination"/>.
    /// </summary>
    public static void TransformSampleInto(
        IReadOnlyList<IPreprocessor<T>> preprocessors,
        ReadOnlySpan<T> sample,
        Span<T> destination)
    {
        if (preprocessors.Count == 0)
        {
            sample.CopyTo(destination);
            return;
        }

        var current = sample.ToArray();
        for (int p = 0; p < preprocessors.Count; p++)
        {
            var next = preprocessors[p].Transform(current);
            if (p == preprocessors.Count - 1)
            {
                next.AsSpan().CopyTo(destination);
                return;
            }
            current = next;
        }
    }
}
