namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A fully connected (dense / linear) layer: y = xWᵀ + b.
/// </summary>
/// <remarks>
/// Supports single samples 1×1×F and batched vectors 1×N×F.
/// Weight layout is [1, outputSize, inputSize] at index [0, o, i].
/// </remarks>
/// <typeparam name="T">Element type.</typeparam>
public sealed class FullyConnectedLayer<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T> _weight;
    private readonly Parameter<T> _bias;

    /// <summary>
    /// Initializes a fully connected layer with Xavier-style uniform random weights.
    /// </summary>
    public FullyConnectedLayer(int inputSize, int outputSize, string? name = null)
        : this(inputSize, outputSize, name, random: null)
    {
    }

    /// <summary>
    /// Initializes a fully connected layer with Xavier-style uniform random weights.
    /// </summary>
    /// <param name="random">Optional RNG for reproducible weight initialization.</param>
    public FullyConnectedLayer(int inputSize, int outputSize, string? name, Random? random)
        : base(name)
    {
        if (inputSize <= 0 || outputSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputSize), "Layer sizes must be positive.");
        }

        InputSize = inputSize;
        OutputSize = outputSize;
        _weight = new Parameter<T>(new Tensor<T>(1, outputSize, inputSize), $"{name}.weight");
        _bias = new Parameter<T>(new Tensor<T>(1, 1, outputSize), $"{name}.bias");

        var limit = T.CreateTruncating(Math.Sqrt(6.0 / (inputSize + outputSize)));
        TensorUtilities.FillUniformRandom(_weight.Value, limit, random);
        _bias.Value.Fill(T.Zero);
    }

    /// <summary>Gets the input feature count.</summary>
    public int InputSize { get; }

    /// <summary>Gets the output feature count.</summary>
    public int OutputSize { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [_weight, _bias];

    /// <inheritdoc/>
    public override TensorShape GetOutputShape(TensorShape inputShape)
    {
        ValidateInputShape(inputShape);
        return new TensorShape(1, inputShape.BatchSize, OutputSize);
    }

    /// <inheritdoc/>
    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        ValidateInputShape(TensorShape.From(input));
        int batchSize = input.Height;
        var output = new Tensor<T>(1, batchSize, OutputSize);

        for (int b = 0; b < batchSize; b++)
        {
            for (int o = 0; o < OutputSize; o++)
            {
                T sum = _bias.Value[0, 0, o];
                for (int i = 0; i < InputSize; i++)
                {
                    sum += input[0, b, i] * _weight.Value[0, o, i];
                }

                output[0, b, o] = sum;
            }
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        var input = CachedInput!;
        int batchSize = input.Height;
        var gradInput = new Tensor<T>(1, batchSize, InputSize);
        gradInput.Fill(T.Zero);

        for (int b = 0; b < batchSize; b++)
        {
            for (int o = 0; o < OutputSize; o++)
            {
                var gradO = gradOutput[0, b, o];
                _bias.Gradient[0, 0, o] += gradO;

                for (int i = 0; i < InputSize; i++)
                {
                    var inVal = input[0, b, i];
                    _weight.Gradient[0, o, i] += gradO * inVal;
                    gradInput[0, b, i] += gradO * _weight.Value[0, o, i];
                }
            }
        }

        return gradInput;
    }

    private void ValidateInputShape(TensorShape inputShape)
    {
        if (inputShape.Width != 1 || inputShape.Depth != InputSize)
        {
            throw new ArgumentException(
                $"Expected input shape 1×N×{InputSize}, got {inputShape.Width}×{inputShape.Height}×{inputShape.Depth}.",
                nameof(inputShape));
        }
    }
}
