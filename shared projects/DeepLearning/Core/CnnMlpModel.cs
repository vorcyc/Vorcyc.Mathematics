namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// End-to-end CNN backbone (<see cref="BatchSequential{T}"/>) + MLP head (<see cref="Sequential{T}"/>).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class CnnMlpModel<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Initializes a hybrid model.
    /// </summary>
    /// <param name="backbone">NHWC CNN stack; output must be N×1×1×F after flatten.</param>
    /// <param name="head">MLP head consuming 1×N×F features.</param>
    public CnnMlpModel(BatchSequential<T> backbone, Sequential<T> head)
    {
        Backbone = backbone ?? throw new ArgumentNullException(nameof(backbone));
        Head = head ?? throw new ArgumentNullException(nameof(head));
    }

    /// <summary>Gets the NHWC CNN backbone.</summary>
    public BatchSequential<T> Backbone { get; }

    /// <summary>Gets the 1×N×F MLP head.</summary>
    public Sequential<T> Head { get; }

    /// <summary>Gets all trainable parameters from backbone and head.</summary>
    public IReadOnlyList<Parameter<T>> Parameters
    {
        get
        {
            var list = new List<Parameter<T>>(Backbone.Parameters.Count + Head.Parameters.Count);
            list.AddRange(Backbone.Parameters);
            list.AddRange(Head.Parameters);
            return list;
        }
    }

    /// <summary>Zeros gradients for backbone and head parameters.</summary>
    public void ZeroGradients()
    {
        Backbone.ZeroGradients();
        Head.ZeroGradients();
    }

    /// <summary>
    /// Runs forward through backbone, bridges N×1×1×F → 1×N×F, then through head.
    /// </summary>
    public Tensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var features = Backbone.Forward(input, training);
        EnsureFeatureShape(features);
        var featureTensor = features.ToFeatureTensor();
        return Head.Forward(featureTensor, training);
    }

    /// <summary>
    /// Backpropagates through head, bridges 1×N×F → N×1×1×F, then through backbone.
    /// </summary>
    public BatchTensor<T> Backward(Tensor<T> gradOutput)
    {
        var gradFeature = Head.Backward(gradOutput);
        var gradBatch = BatchTensor<T>.FromFeatureTensor(gradFeature);
        return Backbone.Backward(gradBatch);
    }

    private static void EnsureFeatureShape(BatchTensor<T> features)
    {
        if (features.Height != 1 || features.Width != 1)
        {
            throw new InvalidOperationException(
                "CNN backbone output must be N×1×1×F. Add BatchFlattenLayer before the MLP head.");
        }
    }
}
