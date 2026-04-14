namespace Vorcyc.Mathematics.DeepLearning.Optimizers;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Stochastic gradient descent optimizer with optional momentum.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class SgdOptimizer<T> : IOptimizer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Dictionary<Parameter<T>, Tensor<T>> _velocity = new();

    /// <summary>
    /// Initializes the optimizer.
    /// </summary>
    /// <param name="learningRate">Step size.</param>
    /// <param name="momentum">Momentum factor in [0, 1). Zero disables momentum.</param>
    public SgdOptimizer(T learningRate, T momentum = default)
    {
        if (learningRate <= T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be positive.");
        }

        LearningRate = learningRate;
        Momentum = momentum;
    }

    /// <summary>Gets or sets the learning rate.</summary>
    public T LearningRate { get; set; }

    /// <summary>Gets or sets the momentum factor.</summary>
    public T Momentum { get; set; }

    /// <inheritdoc/>
    public void Step(IEnumerable<Parameter<T>> parameters)
    {
        foreach (var parameter in parameters)
        {
            var valueSpan = parameter.Value.Values;
            var gradSpan = parameter.Gradient.Values;

            if (Momentum > T.Zero)
            {
                if (!_velocity.TryGetValue(parameter, out var velocity))
                {
                    velocity = new Tensor<T>(
                        parameter.Value.Width,
                        parameter.Value.Height,
                        parameter.Value.Depth);
                    velocity.Fill(T.Zero);
                    _velocity[parameter] = velocity;
                }

                var velSpan = velocity.Values;
                for (int i = 0; i < valueSpan.Length; i++)
                {
                    velSpan[i] = Momentum * velSpan[i] + gradSpan[i];
                    valueSpan[i] -= LearningRate * velSpan[i];
                }
            }
            else
            {
                for (int i = 0; i < valueSpan.Length; i++)
                {
                    valueSpan[i] -= LearningRate * gradSpan[i];
                }
            }
        }
    }

    /// <inheritdoc/>
    public void ZeroGrad(IEnumerable<Parameter<T>> parameters)
    {
        foreach (var parameter in parameters)
        {
            parameter.ZeroGradient();
        }
    }

    /// <inheritdoc/>
    public void SetLearningRate(T learningRate) => LearningRate = learningRate;
}
