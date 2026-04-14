using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.LinearAlgebra;
using LegacyLayers = Vorcyc.Mathematics.DeepLearning.Layers.Layers;

namespace Colorization_example.Models;

/// <summary>
/// Fully connected layer over spatial feature maps, matching the legacy ColorNet GFN linear blocks.
/// </summary>
internal sealed class SpatialLinearLayer<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T>[] _filters;
    private readonly Parameter<T> _bias;

    public SpatialLinearLayer(int inputSize, int outputSize, string? name = null)
        : base(name)
    {
        if (inputSize <= 0 || outputSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputSize), "Layer sizes must be positive.");
        }

        InputSize = inputSize;
        OutputSize = outputSize;
        _filters = new Parameter<T>[outputSize];
        for (int i = 0; i < outputSize; i++)
        {
            _filters[i] = new Parameter<T>(new Tensor<T>(1, 1, inputSize), $"{name}.filter.{i}");
        }

        _bias = new Parameter<T>(new Tensor<T>(1, 1, outputSize), $"{name}.bias");
        _bias.Value.Fill(T.Zero);
    }

    public int InputSize { get; }

    public int OutputSize { get; }

    public override IReadOnlyList<Parameter<T>> Parameters
    {
        get
        {
            var list = new List<Parameter<T>>(_filters.Length + 1);
            list.AddRange(_filters);
            list.Add(_bias);
            return list;
        }
    }

    public override TensorShape GetOutputShape(TensorShape inputShape)
        => new(1, 1, OutputSize);

    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        var filters = Array.ConvertAll(_filters, p => p.Value);
        var output = LegacyLayers.LinearLayer(input, filters, _bias.Value);
        CacheForward(input, output);
        return output;
    }

    public override Tensor<T> Backward(Tensor<T> gradOutput)
        => throw new NotSupportedException("SpatialLinearLayer backward is not implemented for inference-only ColorNet.");
}
