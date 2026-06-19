namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// Shared sampling utilities for ensemble learning.
/// </summary>
internal static class EnsembleHelpers
{
    /// <summary>
    /// Bootstrap (sampling with replacement) indices, with length equal to <paramref name="count"/>.
    /// </summary>
    public static int[] CreateBootstrapIndices(int count, Random random)
    {
        var indices = new int[count];
        for (int i = 0; i < count; i++)
            indices[i] = random.Next(count);
        return indices;
    }
}
