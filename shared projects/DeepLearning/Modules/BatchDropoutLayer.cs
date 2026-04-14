namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Dropout regularization on NHWC tensors (active only during training).
/// </summary>
public sealed class BatchDropoutLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Random _random = new();
    private byte[]? _mask;

    public BatchDropoutLayer(double dropRate = 0.5, string? name = null)
        : base(name)
    {
        if (dropRate is < 0.0 or >= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(dropRate), "Drop rate must be in [0, 1).");
        }

        DropRate = dropRate;
    }

    /// <summary>Gets the dropout probability.</summary>
    public double DropRate { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape) => inputShape;

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        if (!training || DropRate <= 0.0)
        {
            CacheForward(input, input);
            _mask = null;
            return input;
        }

        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        _mask = new byte[input.Values.Length];
        var keepProb = 1.0 - DropRate;
        var scale = T.CreateTruncating(1.0 / keepProb);

        for (int i = 0; i < input.Values.Length; i++)
        {
            bool keep = _random.NextDouble() >= DropRate;
            _mask[i] = keep ? (byte)1 : (byte)0;
            output.Values[i] = keep ? input.Values[i] * scale : T.Zero;
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        if (_mask is null)
        {
            return gradOutput;
        }

        var gradInput = new BatchTensor<T>(gradOutput.Batch, gradOutput.Height, gradOutput.Width, gradOutput.Channels);
        var keepProb = 1.0 - DropRate;
        var scale = T.CreateTruncating(1.0 / keepProb);

        for (int i = 0; i < _mask.Length; i++)
        {
            gradInput.Values[i] = _mask[i] == 1 ? gradOutput.Values[i] * scale : T.Zero;
        }

        return gradInput;
    }
}
