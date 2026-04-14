namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// 2× nearest-neighbor upsampling on NHWC tensors.
/// </summary>
public sealed class BatchUpsample2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchShape _inputShape;

    public BatchUpsample2DLayer(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
        => new(inputShape.Batch, inputShape.Height * 2, inputShape.Width * 2, inputShape.Channels);

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        _inputShape = input.Shape;
        var output = new BatchTensor<T>(input.Batch, input.Height * 2, input.Width * 2, input.Channels);

        for (int n = 0; n < input.Batch; n++)
        {
            for (int c = 0; c < input.Channels; c++)
            {
                for (int ay = 0; ay < input.Height; ay++)
                {
                    var y = 2 * ay;
                    for (int ax = 0; ax < input.Width; ax++)
                    {
                        var x = 2 * ax;
                        var value = input[n, ay, ax, c];
                        for (int fy = 0; fy < 2; fy++)
                        {
                            for (int fx = 0; fx < 2; fx++)
                            {
                                output[n, y + fy, x + fx, c] = value;
                            }
                        }
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
        var gradInput = new BatchTensor<T>(_inputShape.Batch, _inputShape.Height, _inputShape.Width, _inputShape.Channels);

        for (int n = 0; n < _inputShape.Batch; n++)
        {
            for (int c = 0; c < _inputShape.Channels; c++)
            {
                for (int ay = 0; ay < _inputShape.Height; ay++)
                {
                    var y = 2 * ay;
                    for (int ax = 0; ax < _inputShape.Width; ax++)
                    {
                        var x = 2 * ax;
                        T sum = T.Zero;
                        for (int fy = 0; fy < 2; fy++)
                        {
                            for (int fx = 0; fx < 2; fx++)
                            {
                                sum += gradOutput[n, y + fy, x + fx, c];
                            }
                        }

                        gradInput[n, ay, ax, c] = sum;
                    }
                }
            }
        }

        return gradInput;
    }
}
