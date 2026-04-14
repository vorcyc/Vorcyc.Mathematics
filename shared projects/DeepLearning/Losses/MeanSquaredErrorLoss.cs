namespace Vorcyc.Mathematics.DeepLearning.Losses;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Mean squared error loss: 0.5 · mean((prediction - target)²).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class MeanSquaredErrorLoss<T> : ILoss<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    public T Compute(Tensor<T> prediction, Tensor<T> target)
    {
        ValidateShapes(prediction, target);
        var predSpan = prediction.Values;
        var targetSpan = target.Values;
        T sum = T.Zero;
        for (int i = 0; i < predSpan.Length; i++)
        {
            var diff = predSpan[i] - targetSpan[i];
            sum += diff * diff;
        }

        return sum / T.CreateTruncating(2.0 * predSpan.Length);
    }

    /// <inheritdoc/>
    public Tensor<T> Backward(Tensor<T> prediction, Tensor<T> target)
    {
        ValidateShapes(prediction, target);
        var grad = new Tensor<T>(prediction.Width, prediction.Height, prediction.Depth);
        var gradSpan = grad.Values;
        var predSpan = prediction.Values;
        var targetSpan = target.Values;
        var scale = T.One / T.CreateTruncating(predSpan.Length);

        for (int i = 0; i < gradSpan.Length; i++)
        {
            gradSpan[i] = (predSpan[i] - targetSpan[i]) * scale;
        }

        return grad;
    }

    private static void ValidateShapes(Tensor<T> prediction, Tensor<T> target)
    {
        if (prediction.Width != target.Width
            || prediction.Height != target.Height
            || prediction.Depth != target.Depth)
        {
            throw new ArgumentException("Prediction and target tensors must have the same shape.");
        }
    }
}
