namespace Vorcyc.Mathematics.DeepLearning.Serialization;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Modules;

/// <summary>
/// Describes layer hyper-parameters for architecture-aware serialization.
/// </summary>
/// <param name="TypeId">Stable layer type identifier.</param>
/// <param name="Name">Layer name.</param>
/// <param name="IntParameters">Integer hyper-parameters.</param>
/// <param name="DoubleParameters">Floating hyper-parameters.</param>
public readonly record struct LayerDescriptor(
    string TypeId,
    string Name,
    int[] IntParameters,
    double[] DoubleParameters);

/// <summary>
/// Maps trainable layers to architecture descriptors.
/// </summary>
public static class ModelArchitectureCatalog
{
    public static IReadOnlyList<LayerDescriptor> Describe<T>(Sequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => DescribeSequentialLayers(model.Layers);

    public static IReadOnlyList<LayerDescriptor> Describe<T>(BatchSequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => DescribeBatchLayers(model.Layers);

    public static IReadOnlyList<LayerDescriptor> Describe<T>(CnnMlpModel<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var list = new List<LayerDescriptor>();
        foreach (var layer in DescribeBatchLayers(model.Backbone.Layers))
        {
            list.Add(layer with { Name = $"backbone.{layer.Name}" });
        }

        foreach (var layer in DescribeSequentialLayers(model.Head.Layers))
        {
            list.Add(layer with { Name = $"head.{layer.Name}" });
        }

        return list;
    }

    public static void VerifyMatches<T>(IReadOnlyList<LayerDescriptor> expected, Sequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => Verify(expected, DescribeSequentialLayers(model.Layers));

    public static void VerifyMatches<T>(IReadOnlyList<LayerDescriptor> expected, BatchSequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => Verify(expected, DescribeBatchLayers(model.Layers));

    public static void VerifyMatches<T>(IReadOnlyList<LayerDescriptor> expected, CnnMlpModel<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => Verify(expected, Describe(model));

    private static List<LayerDescriptor> DescribeBatchLayers<T>(IReadOnlyList<IBatchLayer<T>> layers)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var list = new List<LayerDescriptor>(layers.Count);
        foreach (var layer in layers)
        {
            list.Add(DescribeBatchLayer(layer));
        }

        return list;
    }

    private static List<LayerDescriptor> DescribeSequentialLayers<T>(IReadOnlyList<ILayer<T>> layers)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var list = new List<LayerDescriptor>(layers.Count);
        foreach (var layer in layers)
        {
            list.Add(DescribeSequentialLayer(layer));
        }

        return list;
    }

    private static LayerDescriptor DescribeBatchLayer<T>(IBatchLayer<T> layer)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        return layer switch
        {
            BatchConvolution2DLayer<T> c => new(
                "BatchConv2D",
                layer.Name,
                [c.InnerLayer.InputChannels, c.InnerLayer.OutputChannels, c.InnerLayer.KernelSize, c.InnerLayer.Stride, c.InnerLayer.Dilation],
                []),
            BatchDepthwiseConvolution2DLayer<T> d => new(
                "BatchDepthwiseConv2D",
                layer.Name,
                [d.Channels, d.KernelSize, d.Stride, d.Dilation],
                []),
            BatchBatchNormLayer<T> b => new("BatchBatchNorm", layer.Name, [b.Channels], []),
            BatchLayerNormLayer<T> l => new("BatchLayerNorm", layer.Name, [l.Channels], []),
            BatchFullyConnectedLayer<T> f => new("BatchFC", layer.Name, [f.InputSize, f.OutputSize], []),
            BatchDropoutLayer<T> drop => new("BatchDropout", layer.Name, [], [drop.DropRate]),
            BatchLeakyReLUActivation<T> leaky => new(
                "BatchLeakyReLU",
                layer.Name,
                [],
                [double.CreateTruncating(leaky.NegativeSlope)]),
            BatchFlattenLayer<T> => new("BatchFlatten", layer.Name, [], []),
            BatchGlobalAveragePool2DLayer<T> => new("BatchGlobalAvgPool2D", layer.Name, [], []),
            BatchAvgPool2DLayer<T> => new("BatchAvgPool2D", layer.Name, [], []),
            BatchUpsample2DLayer<T> => new("BatchUpsample2D", layer.Name, [], []),
            BatchReLUActivation<T> => new("BatchReLU", layer.Name, [], []),
            BatchSigmoidActivation<T> => new("BatchSigmoid", layer.Name, [], []),
            BatchTanhActivation<T> => new("BatchTanh", layer.Name, [], []),
            BatchSoftmaxLayer<T> => new("BatchSoftmax", layer.Name, [], []),
            BatchConcatenateLayer<T> => new("BatchConcatenate", layer.Name, [], []),
            BatchResidualBlockLayer<T> r => new("BatchResidualBlock", layer.Name, [r.InputChannels, r.OutputChannels, r.Stride, r.KernelSize], []),
            BatchSqueezeExciteLayer<T> s => new("BatchSE", layer.Name, [s.Channels, s.Reduction], []),
            BatchTransposedConvolution2DLayer<T> t => new(
                "BatchConv2DTranspose",
                layer.Name,
                [t.InputChannels, t.OutputChannels, t.KernelSize, t.Stride, t.Dilation],
                []),
            _ when layer.GetType().Name.StartsWith("BatchMaxPool2DLayer", StringComparison.Ordinal) => new("BatchMaxPool2D", layer.Name, [], []),
            _ => new(layer.GetType().Name, layer.Name, [], [])
        };
    }

    private static LayerDescriptor DescribeSequentialLayer<T>(ILayer<T> layer)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        return layer switch
        {
            FullyConnectedLayer<T> f => new("FC", layer.Name, [f.InputSize, f.OutputSize], []),
            BatchNormLayer<T> b => new("BatchNorm", layer.Name, [b.RunningMean.Depth], []),
            SigmoidActivation<T> => new("Sigmoid", layer.Name, [], []),
            ReLUActivation<T> => new("ReLU", layer.Name, [], []),
            SoftmaxLayer<T> => new("Softmax", layer.Name, [], []),
            _ => new(layer.GetType().Name, layer.Name, [], [])
        };
    }

    private static void Verify(IReadOnlyList<LayerDescriptor> expected, IReadOnlyList<LayerDescriptor> actual)
    {
        if (expected.Count != actual.Count)
        {
            throw new InvalidDataException($"Architecture layer count mismatch: expected {expected.Count}, got {actual.Count}.");
        }

        for (int i = 0; i < expected.Count; i++)
        {
            var e = expected[i];
            var a = actual[i];
            if (e.TypeId != a.TypeId || e.Name != a.Name)
            {
                throw new InvalidDataException($"Architecture mismatch at layer {i}: expected {e.TypeId}/{e.Name}, got {a.TypeId}/{a.Name}.");
            }

            if (!e.IntParameters.AsSpan().SequenceEqual(a.IntParameters))
            {
                throw new InvalidDataException($"Architecture int-parameter mismatch at layer {i} ({e.Name}).");
            }

            if (!e.DoubleParameters.AsSpan().SequenceEqual(a.DoubleParameters))
            {
                throw new InvalidDataException($"Architecture double-parameter mismatch at layer {i} ({e.Name}).");
            }
        }
    }
}
