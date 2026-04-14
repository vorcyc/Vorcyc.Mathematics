namespace Vorcyc.Mathematics.DeepLearning.Training;

using System.Numerics;

/// <summary>
/// A labeled NHWC batch for <see cref="BatchSequential{T}"/> training.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <param name="Input">NHWC input batch.</param>
/// <param name="Target">NHWC target batch (e.g. one-hot logits/labels).</param>
public readonly record struct BatchLabelSample<T>(BatchTensor<T> Input, BatchTensor<T> Target)
    where T : unmanaged, IBinaryFloatingPointIeee754<T>;
