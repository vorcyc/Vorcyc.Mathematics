namespace Vorcyc.Mathematics.DeepLearning.Serialization;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;

/// <summary>
/// Reconstructs trainable models from <see cref="LayerDescriptor"/> metadata.
/// </summary>
public static class ModelArchitectureBuilder
{
    /// <summary>Builds a <see cref="BatchSequential{T}"/> from layer descriptors.</summary>
    public static BatchSequential<T> BuildBatchSequential<T>(IReadOnlyList<LayerDescriptor> layers)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (layers.Count == 0)
        {
            throw new ArgumentException("At least one layer descriptor is required.", nameof(layers));
        }

        var built = new IBatchLayer<T>[layers.Count];
        for (int i = 0; i < layers.Count; i++)
        {
            built[i] = BuildBatchLayer<T>(layers[i]);
        }

        return new BatchSequential<T>(built);
    }

    /// <summary>Builds a <see cref="CnnMlpModel{T}"/> from prefixed descriptors.</summary>
    public static CnnMlpModel<T> BuildCnnMlp<T>(IReadOnlyList<LayerDescriptor> layers)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var backbone = new List<LayerDescriptor>();
        var head = new List<LayerDescriptor>();
        foreach (var layer in layers)
        {
            if (layer.Name.StartsWith("backbone.", StringComparison.Ordinal))
            {
                backbone.Add(layer with { Name = layer.Name["backbone.".Length..] });
            }
            else if (layer.Name.StartsWith("head.", StringComparison.Ordinal))
            {
                head.Add(layer with { Name = layer.Name["head.".Length..] });
            }
            else
            {
                throw new InvalidDataException($"CnnMlp descriptor '{layer.Name}' must start with 'backbone.' or 'head.'.");
            }
        }

        if (backbone.Count == 0 || head.Count == 0)
        {
            throw new InvalidDataException("CnnMlp architecture requires both backbone and head layers.");
        }

        return new CnnMlpModel<T>(BuildBatchSequential<T>(backbone), BuildSequential<T>(head));
    }

    /// <summary>Builds a <see cref="BatchParallelConcatModel{T}"/> from left/right descriptor sections.</summary>
    public static BatchParallelConcatModel<T> BuildBatchParallelConcat<T>(
        IReadOnlyList<LayerDescriptor> leftLayers,
        IReadOnlyList<LayerDescriptor> rightLayers)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (leftLayers.Count == 0 || rightLayers.Count == 0)
        {
            throw new ArgumentException("Both branches require at least one layer.");
        }

        return new BatchParallelConcatModel<T>(BuildBatchSequential<T>(leftLayers), BuildBatchSequential<T>(rightLayers));
    }

    /// <summary>Builds a <see cref="Sequential{T}"/> from layer descriptors.</summary>
    public static Sequential<T> BuildSequential<T>(IReadOnlyList<LayerDescriptor> layers)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (layers.Count == 0)
        {
            throw new ArgumentException("At least one layer descriptor is required.", nameof(layers));
        }

        var built = new ILayer<T>[layers.Count];
        for (int i = 0; i < layers.Count; i++)
        {
            built[i] = BuildSequentialLayer<T>(layers[i]);
        }

        return new Sequential<T>(built);
    }

    /// <summary>Builds a single batch layer from a descriptor.</summary>
    public static IBatchLayer<T> BuildBatchLayer<T>(LayerDescriptor descriptor)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var ints = descriptor.IntParameters;
        var doubles = descriptor.DoubleParameters;
        return descriptor.TypeId switch
        {
            "BatchConv2D" => new BatchConvolution2DLayer<T>(ints[0], ints[1], ints[2], ints[3], ints[4], descriptor.Name),
            "BatchDepthwiseConv2D" => new BatchDepthwiseConvolution2DLayer<T>(ints[0], ints[1], ints[2], ints[3], descriptor.Name),
            "BatchConv2DTranspose" => new BatchTransposedConvolution2DLayer<T>(ints[0], ints[1], ints[2], ints[3], ints[4], descriptor.Name),
            "BatchBatchNorm" => new BatchBatchNormLayer<T>(ints[0], descriptor.Name),
            "BatchLayerNorm" => new BatchLayerNormLayer<T>(ints[0], descriptor.Name),
            "BatchFC" => new BatchFullyConnectedLayer<T>(ints[0], ints[1], descriptor.Name),
            "BatchDropout" => new BatchDropoutLayer<T>(doubles[0], descriptor.Name),
            "BatchLeakyReLU" => new BatchLeakyReLUActivation<T>(doubles[0], descriptor.Name),
            "BatchFlatten" => new BatchFlattenLayer<T>(),
            "BatchGlobalAvgPool2D" => new BatchGlobalAveragePool2DLayer<T>(descriptor.Name),
            "BatchAvgPool2D" => new BatchAvgPool2DLayer<T>(descriptor.Name),
            "BatchMaxPool2D" => CreateMaxPoolLayer<T>(),
            "BatchUpsample2D" => new BatchUpsample2DLayer<T>(descriptor.Name),
            "BatchReLU" => new BatchReLUActivation<T>(descriptor.Name),
            "BatchSigmoid" => new BatchSigmoidActivation<T>(descriptor.Name),
            "BatchTanh" => new BatchTanhActivation<T>(descriptor.Name),
            "BatchSoftmax" => new BatchSoftmaxLayer<T>(descriptor.Name),
            "BatchConcatenate" => new BatchConcatenateLayer<T>(descriptor.Name),
            "BatchResidualBlock" => new BatchResidualBlockLayer<T>(ints[0], ints[1], ints[2], ints[3], descriptor.Name),
            "BatchSE" => new BatchSqueezeExciteLayer<T>(ints[0], ints[1], descriptor.Name),
            _ => throw new NotSupportedException($"Unsupported batch layer type '{descriptor.TypeId}'.")
        };
    }

    private static IBatchLayer<T> CreateMaxPoolLayer<T>()
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        => new BatchMaxPool2DLayer<T>();

    /// <summary>Builds a single legacy sequential layer from a descriptor.</summary>
    public static ILayer<T> BuildSequentialLayer<T>(LayerDescriptor descriptor)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var ints = descriptor.IntParameters;
        return descriptor.TypeId switch
        {
            "FC" => new FullyConnectedLayer<T>(ints[0], ints[1], descriptor.Name),
            "BatchNorm" => new BatchNormLayer<T>(ints[0], descriptor.Name),
            "Sigmoid" => new SigmoidActivation<T>(descriptor.Name),
            "ReLU" => new ReLUActivation<T>(descriptor.Name),
            "Softmax" => new SoftmaxLayer<T>(descriptor.Name),
            _ => throw new NotSupportedException($"Unsupported sequential layer type '{descriptor.TypeId}'.")
        };
    }
}
