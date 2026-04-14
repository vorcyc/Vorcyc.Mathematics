namespace Vorcyc.Mathematics.DeepLearning.Training;

using System.Numerics;

/// <summary>
/// NHWC input batch with integer class labels (one per sample).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <param name="Input">NHWC input batch.</param>
/// <param name="ClassIndices">Class index per batch item (length = batch size).</param>
public readonly record struct BatchClassLabelSample<T>(BatchTensor<T> Input, int[] ClassIndices)
    where T : unmanaged, IBinaryFloatingPointIeee754<T>;
