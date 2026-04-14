namespace Vorcyc.Mathematics.DeepLearning.Losses;

using System.Numerics;

/// <summary>
/// Batch loss that accepts integer class indices instead of one-hot targets.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface ISparseBatchLoss<T> : IBatchLoss<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>Computes loss from per-sample class indices.</summary>
    T ComputeFromClassIndices(BatchTensor<T> prediction, ReadOnlySpan<int> classIndices);

    /// <summary>Backpropagates using integer class indices.</summary>
    BatchTensor<T> BackwardFromClassIndices(BatchTensor<T> prediction, ReadOnlySpan<int> classIndices);
}
