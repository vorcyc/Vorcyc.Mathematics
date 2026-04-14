namespace Vorcyc.Mathematics.DeepLearning.Optimizers;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Adam optimizer (Kingma &amp; Ba).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class AdamOptimizer<T> : IOptimizer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Dictionary<Parameter<T>, Tensor<T>> _firstMoment = new();
    private readonly Dictionary<Parameter<T>, Tensor<T>> _secondMoment = new();
    private int _stepCount;

    /// <summary>
    /// Initializes Adam with the given hyperparameters.
    /// </summary>
    public AdamOptimizer(T learningRate, T beta1 = default, T beta2 = default, T epsilon = default)
    {
        if (learningRate <= T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be positive.");
        }

        LearningRate = learningRate;
        Beta1 = beta1 == T.Zero ? T.CreateChecked(0.9) : beta1;
        Beta2 = beta2 == T.Zero ? T.CreateChecked(0.999) : beta2;
        Epsilon = epsilon == T.Zero ? T.CreateChecked(1e-8) : epsilon;
    }

    /// <summary>Gets or sets the learning rate.</summary>
    public T LearningRate { get; set; }

    /// <summary>Gets or sets the first moment decay.</summary>
    public T Beta1 { get; set; }

    /// <summary>Gets or sets the second moment decay.</summary>
    public T Beta2 { get; set; }

    /// <summary>Gets or sets the numerical stability constant.</summary>
    public T Epsilon { get; set; }

    /// <inheritdoc/>
    public void Step(IEnumerable<Parameter<T>> parameters)
    {
        _stepCount++;
        var biasCorrection1 = T.One - T.Pow(Beta1, T.CreateTruncating(_stepCount));
        var biasCorrection2 = T.One - T.Pow(Beta2, T.CreateTruncating(_stepCount));

        foreach (var parameter in parameters)
        {
            if (!_firstMoment.TryGetValue(parameter, out var m))
            {
                m = new Tensor<T>(parameter.Value.Width, parameter.Value.Height, parameter.Value.Depth);
                m.Fill(T.Zero);
                _firstMoment[parameter] = m;
            }

            if (!_secondMoment.TryGetValue(parameter, out var v))
            {
                v = new Tensor<T>(parameter.Value.Width, parameter.Value.Height, parameter.Value.Depth);
                v.Fill(T.Zero);
                _secondMoment[parameter] = v;
            }

            var valueSpan = parameter.Value.Values;
            var gradSpan = parameter.Gradient.Values;
            var mSpan = m.Values;
            var vSpan = v.Values;

            for (int i = 0; i < valueSpan.Length; i++)
            {
                var g = gradSpan[i];
                mSpan[i] = Beta1 * mSpan[i] + (T.One - Beta1) * g;
                vSpan[i] = Beta2 * vSpan[i] + (T.One - Beta2) * g * g;

                var mHat = mSpan[i] / biasCorrection1;
                var vHat = vSpan[i] / biasCorrection2;
                valueSpan[i] -= LearningRate * mHat / (T.Sqrt(vHat) + Epsilon);
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
