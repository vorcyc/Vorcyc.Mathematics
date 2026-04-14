namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Squeeze-and-Excitation channel attention (GAP → FC → ReLU → FC → Sigmoid → scale).
/// </summary>
public sealed class BatchSqueezeExciteLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly BatchGlobalAveragePool2DLayer<T> _squeeze;
    private readonly BatchFullyConnectedLayer<T> _reduce;
    private readonly BatchReLUActivation<T> _relu;
    private readonly BatchFullyConnectedLayer<T> _expand;
    private readonly BatchSigmoidActivation<T> _gate;
    private BatchTensor<T>? _cachedGates;

    public BatchSqueezeExciteLayer(int channels, int reduction = 16, string? name = null)
        : base(name)
    {
        if (channels <= 0 || reduction <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channels), "Channels and reduction must be positive.");
        }

        Channels = channels;
        Reduction = reduction;
        int hidden = Math.Max(1, channels / reduction);

        _squeeze = new BatchGlobalAveragePool2DLayer<T>($"{name}.squeeze");
        _reduce = new BatchFullyConnectedLayer<T>(channels, hidden, $"{name}.fc1");
        _relu = new BatchReLUActivation<T>($"{name}.relu");
        _expand = new BatchFullyConnectedLayer<T>(hidden, channels, $"{name}.fc2");
        _gate = new BatchSigmoidActivation<T>($"{name}.sigmoid");
    }

    public int Channels { get; }
    public int Reduction { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters
    {
        get
        {
            var list = new List<Parameter<T>>();
            list.AddRange(_reduce.Parameters);
            list.AddRange(_expand.Parameters);
            return list;
        }
    }

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (inputShape.Channels != Channels)
        {
            throw new ArgumentException($"Expected {Channels} channels, got {inputShape.Channels}.");
        }

        return inputShape;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var squeezed = _squeeze.Forward(input, training);
        var reduced = _relu.Forward(_reduce.Forward(squeezed, training), training);
        _cachedGates = _gate.Forward(_expand.Forward(reduced, training), training);

        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        for (int n = 0; n < input.Batch; n++)
        {
            for (int c = 0; c < input.Channels; c++)
            {
                var scale = _cachedGates[n, 0, 0, c];
                for (int h = 0; h < input.Height; h++)
                {
                    for (int w = 0; w < input.Width; w++)
                    {
                        output[n, h, w, c] = input[n, h, w, c] * scale;
                    }
                }
            }
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        if (_cachedGates is null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        var input = CachedInput!;
        var gates = _cachedGates;
        var gradInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        var gradGate = new BatchTensor<T>(input.Batch, 1, 1, input.Channels);

        for (int n = 0; n < input.Batch; n++)
        {
            for (int c = 0; c < input.Channels; c++)
            {
                var gate = gates[n, 0, 0, c];
                T gateGrad = T.Zero;
                for (int h = 0; h < input.Height; h++)
                {
                    for (int w = 0; w < input.Width; w++)
                    {
                        gradInput[n, h, w, c] = gradOutput[n, h, w, c] * gate;
                        gateGrad += gradOutput[n, h, w, c] * input[n, h, w, c];
                    }
                }

                gradGate[n, 0, 0, c] = gateGrad;
            }
        }

        var gradExpand = _gate.Backward(gradGate);
        var gradReduced = _expand.Backward(gradExpand);
        var gradRelu = _relu.Backward(gradReduced);
        var gradSqueezed = _reduce.Backward(gradRelu);
        _squeeze.Backward(gradSqueezed);

        return gradInput;
    }
}
