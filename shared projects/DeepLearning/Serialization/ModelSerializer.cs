namespace Vorcyc.Mathematics.DeepLearning.Serialization;

using System.Numerics;
using System.Text;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Saves and loads model architecture metadata, trainable parameters, and batch-norm running statistics (format v3).
/// </summary>
public static partial class ModelSerializer
{
    private const string Magic = "VMDL";
    private const int FormatVersion = 3;

    private enum ModelKind : byte
    {
        Sequential = 0,
        CnnMlp = 1,
        BatchSequential = 2,
        BatchParallelConcat = 3
    }

    private static List<NamedTensor<T>> CollectSequentialEntries<T>(Sequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => CollectLayerEntries(model.Layers, "layer");

    private static List<NamedTensor<T>> CollectCnnMlpEntries<T>(CnnMlpModel<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var entries = CollectBatchLayerEntries(model.Backbone.Layers, "backbone");
        entries.AddRange(CollectLayerEntries(model.Head.Layers, "head"));
        return entries;
    }

    private static List<NamedTensor<T>> CollectLayerEntries<T>(
        IReadOnlyList<ILayer<T>> layers,
        string prefix)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var entries = new List<NamedTensor<T>>();
        int layerIndex = 0;
        foreach (var layer in layers)
        {
            int paramIndex = 0;
            foreach (var parameter in layer.Parameters)
            {
                var name = parameter.Name ?? $"{prefix}.{layer.Name}.{paramIndex}";
                entries.Add(new NamedTensor<T>(name, parameter.Value));
                paramIndex++;
            }

            if (layer is BatchNormLayer<T> batchNorm)
            {
                entries.Add(new NamedTensor<T>($"{prefix}.{layer.Name}.running_mean.{layerIndex}", batchNorm.RunningMean));
                entries.Add(new NamedTensor<T>($"{prefix}.{layer.Name}.running_var.{layerIndex}", batchNorm.RunningVariance));
            }

            layerIndex++;
        }

        return entries;
    }

    private static List<NamedTensor<T>> CollectBatchLayerEntries<T>(
        IReadOnlyList<IBatchLayer<T>> layers,
        string prefix)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var entries = new List<NamedTensor<T>>();
        int layerIndex = 0;
        foreach (var layer in layers)
        {
            int paramIndex = 0;
            foreach (var parameter in layer.Parameters)
            {
                var name = parameter.Name ?? $"{prefix}.{layer.Name}.{paramIndex}";
                entries.Add(new NamedTensor<T>(name, parameter.Value));
                paramIndex++;
            }

            if (layer is BatchBatchNormLayer<T> batchNorm)
            {
                entries.Add(new NamedTensor<T>($"{prefix}.{layer.Name}.running_mean.{layerIndex}", batchNorm.RunningMean));
                entries.Add(new NamedTensor<T>($"{prefix}.{layer.Name}.running_var.{layerIndex}", batchNorm.RunningVariance));
            }

            layerIndex++;
        }

        return entries;
    }

    private static T[] FlattenEntries<T>(List<NamedTensor<T>> entries)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var values = new List<T>();
        foreach (var entry in entries)
        {
            values.AddRange(entry.Tensor.Values.ToArray());
        }

        return values.ToArray();
    }

    private static void WriteEntries<T>(BinaryWriter writer, List<NamedTensor<T>> entries)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        writer.Write(entries.Count);
        foreach (var entry in entries)
        {
            writer.Write(entry.Name);
            writer.Write(entry.Tensor.Width);
            writer.Write(entry.Tensor.Height);
            writer.Write(entry.Tensor.Depth);
            WriteTensorValues(writer, entry.Tensor);
        }
    }

    private static void ReadEntries<T>(BinaryReader reader, List<NamedTensor<T>> expected)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        int entryCount = reader.ReadInt32();
        if (entryCount != expected.Count)
        {
            throw new InvalidDataException($"Expected {expected.Count} tensors, file contains {entryCount}.");
        }

        for (int i = 0; i < entryCount; i++)
        {
            var name = reader.ReadString();
            int w = reader.ReadInt32();
            int h = reader.ReadInt32();
            int d = reader.ReadInt32();
            if (expected[i].Name != name
                || expected[i].Tensor.Width != w
                || expected[i].Tensor.Height != h
                || expected[i].Tensor.Depth != d)
            {
                throw new InvalidDataException($"Tensor metadata mismatch at entry {i} ({name}).");
            }

            ReadTensorValues(reader, expected[i].Tensor);
        }
    }

    private static void WriteTensorValues<T>(BinaryWriter writer, Tensor<T> tensor)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        foreach (var value in tensor.Values)
        {
            writer.Write(double.CreateTruncating(value));
        }
    }

    private static void ReadTensorValues<T>(BinaryReader reader, Tensor<T> tensor)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var span = tensor.Values;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = T.CreateTruncating(reader.ReadDouble());
        }
    }

    private readonly record struct NamedTensor<T>(string Name, Tensor<T> Tensor)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>;
}
