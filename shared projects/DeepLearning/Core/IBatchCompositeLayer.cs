namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;

/// <summary>
/// A batch layer composed of child layers (e.g. residual block, squeeze-excite).
/// Implementing this lets infrastructure such as the serializer recurse into nested
/// layers so their non-parameter state (e.g. batch-norm running statistics) is handled.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IBatchCompositeLayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>Gets the ordered child layers nested inside this composite.</summary>
    IReadOnlyList<IBatchLayer<T>> Children { get; }
}
