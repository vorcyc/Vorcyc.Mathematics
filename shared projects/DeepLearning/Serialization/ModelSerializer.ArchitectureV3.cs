namespace Vorcyc.Mathematics.DeepLearning.Serialization;

using System.Numerics;
using System.Text;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Public save/load API (format v3: architecture metadata + weights).
/// </summary>
public static partial class ModelSerializer
{
    /// <summary>Saves architecture and weights for a <see cref="Sequential{T}"/> model.</summary>
    public static void Save<T>(Sequential<T> model, Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(stream);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        WriteHeader(writer, ModelKind.Sequential, typeof(T));
        WriteArchitecture(writer, ModelArchitectureCatalog.Describe(model));
        WriteEntries(writer, CollectSequentialEntries(model));
    }

    /// <summary>Rebuilds a <see cref="Sequential{T}"/> from a file and loads weights.</summary>
    public static Sequential<T> LoadSequential<T>(Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var kind = ReadHeader(reader, ModelKind.Sequential);
        if (kind != ModelKind.Sequential)
        {
            throw new InvalidDataException("The model file is not a Sequential model.");
        }

        var architecture = ReadArchitecture(reader);
        var model = ModelArchitectureBuilder.BuildSequential<T>(architecture);
        ReadEntries(reader, CollectSequentialEntries(model));
        return model;
    }

    /// <summary>Loads weights into an existing <see cref="Sequential{T}"/> and verifies architecture.</summary>
    public static void Load<T>(Sequential<T> model, Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var kind = ReadHeader(reader, ModelKind.Sequential);
        if (kind != ModelKind.Sequential)
        {
            throw new InvalidDataException("The model file is not a Sequential model.");
        }

        var architecture = ReadArchitecture(reader);
        ModelArchitectureCatalog.VerifyMatches(architecture, model);
        ReadEntries(reader, CollectSequentialEntries(model));
    }

    /// <summary>Saves architecture and weights for a <see cref="BatchSequential{T}"/> model.</summary>
    public static void Save<T>(BatchSequential<T> model, Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(stream);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        WriteHeader(writer, ModelKind.BatchSequential, typeof(T));
        WriteArchitecture(writer, ModelArchitectureCatalog.Describe(model));
        WriteEntries(writer, CollectBatchSequentialEntries(model));
    }

    /// <summary>Rebuilds a <see cref="BatchSequential{T}"/> from a file and loads weights.</summary>
    public static BatchSequential<T> LoadBatchSequential<T>(Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var kind = ReadHeader(reader, ModelKind.BatchSequential);
        if (kind != ModelKind.BatchSequential)
        {
            throw new InvalidDataException("The model file is not a BatchSequential model.");
        }

        var architecture = ReadArchitecture(reader);
        var model = ModelArchitectureBuilder.BuildBatchSequential<T>(architecture);
        ReadEntries(reader, CollectBatchSequentialEntries(model));
        return model;
    }

    /// <summary>Loads weights into an existing <see cref="BatchSequential{T}"/> and verifies architecture.</summary>
    public static void Load<T>(BatchSequential<T> model, Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var kind = ReadHeader(reader, ModelKind.BatchSequential);
        if (kind != ModelKind.BatchSequential)
        {
            throw new InvalidDataException("The model file is not a BatchSequential model.");
        }

        var architecture = ReadArchitecture(reader);
        ModelArchitectureCatalog.VerifyMatches(architecture, model);
        ReadEntries(reader, CollectBatchSequentialEntries(model));
    }

    /// <summary>Saves architecture and weights for a <see cref="CnnMlpModel{T}"/>.</summary>
    public static void Save<T>(CnnMlpModel<T> model, Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(stream);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        WriteHeader(writer, ModelKind.CnnMlp, typeof(T));
        WriteArchitecture(writer, ModelArchitectureCatalog.Describe(model));
        WriteEntries(writer, CollectCnnMlpEntries(model));
    }

    /// <summary>Rebuilds a <see cref="CnnMlpModel{T}"/> from a file and loads weights.</summary>
    public static CnnMlpModel<T> LoadCnnMlp<T>(Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var kind = ReadHeader(reader, ModelKind.CnnMlp);
        if (kind != ModelKind.CnnMlp)
        {
            throw new InvalidDataException("The model file is not a CnnMlp model.");
        }

        var architecture = ReadArchitecture(reader);
        var model = ModelArchitectureBuilder.BuildCnnMlp<T>(architecture);
        ReadEntries(reader, CollectCnnMlpEntries(model));
        return model;
    }

    /// <summary>Loads weights into an existing <see cref="CnnMlpModel{T}"/> and verifies architecture.</summary>
    public static void Load<T>(CnnMlpModel<T> model, Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var kind = ReadHeader(reader, ModelKind.CnnMlp);
        if (kind != ModelKind.CnnMlp)
        {
            throw new InvalidDataException("The model file is not a CnnMlp model.");
        }

        var architecture = ReadArchitecture(reader);
        ModelArchitectureCatalog.VerifyMatches(architecture, model);
        ReadEntries(reader, CollectCnnMlpEntries(model));
    }

    /// <summary>Saves architecture and weights for a <see cref="BatchParallelConcatModel{T}"/>.</summary>
    public static void Save<T>(BatchParallelConcatModel<T> model, Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(stream);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        WriteHeader(writer, ModelKind.BatchParallelConcat, typeof(T));
        WriteParallelArchitecture(writer, ModelArchitectureCatalog.Describe(model.Left), ModelArchitectureCatalog.Describe(model.Right));
        WriteEntries(writer, CollectBatchParallelConcatEntries(model));
    }

    /// <summary>Rebuilds a <see cref="BatchParallelConcatModel{T}"/> from a file and loads weights.</summary>
    public static BatchParallelConcatModel<T> LoadBatchParallelConcat<T>(Stream stream)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var kind = ReadHeader(reader, ModelKind.BatchParallelConcat);
        if (kind != ModelKind.BatchParallelConcat)
        {
            throw new InvalidDataException("The model file is not a BatchParallelConcat model.");
        }

        var (left, right) = ReadParallelArchitecture(reader);
        var model = ModelArchitectureBuilder.BuildBatchParallelConcat<T>(left, right);
        ReadEntries(reader, CollectBatchParallelConcatEntries(model));
        return model;
    }

    /// <summary>Saves a <see cref="Sequential{T}"/> to a file path.</summary>
    public static void SaveToFile<T>(Sequential<T> model, string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        using var stream = File.Create(path);
        Save(model, stream);
    }

    /// <summary>Rebuilds a <see cref="Sequential{T}"/> from a file path.</summary>
    public static Sequential<T> LoadSequentialFromFile<T>(string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        using var stream = File.OpenRead(path);
        return LoadSequential<T>(stream);
    }

    /// <summary>Loads weights into an existing <see cref="Sequential{T}"/> from a file path.</summary>
    public static void LoadFromFile<T>(Sequential<T> model, string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        using var stream = File.OpenRead(path);
        Load(model, stream);
    }

    /// <summary>Saves a <see cref="BatchSequential{T}"/> to a file path.</summary>
    public static void SaveToFile<T>(BatchSequential<T> model, string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        using var stream = File.Create(path);
        Save(model, stream);
    }

    /// <summary>Rebuilds a <see cref="BatchSequential{T}"/> from a file path.</summary>
    public static BatchSequential<T> LoadBatchSequentialFromFile<T>(string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        using var stream = File.OpenRead(path);
        return LoadBatchSequential<T>(stream);
    }

    /// <summary>Loads weights into an existing <see cref="BatchSequential{T}"/> from a file path.</summary>
    public static void LoadFromFile<T>(BatchSequential<T> model, string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        using var stream = File.OpenRead(path);
        Load(model, stream);
    }

    /// <summary>Saves a <see cref="CnnMlpModel{T}"/> to a file path.</summary>
    public static void SaveToFile<T>(CnnMlpModel<T> model, string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        using var stream = File.Create(path);
        Save(model, stream);
    }

    /// <summary>Rebuilds a <see cref="CnnMlpModel{T}"/> from a file path.</summary>
    public static CnnMlpModel<T> LoadCnnMlpFromFile<T>(string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        using var stream = File.OpenRead(path);
        return LoadCnnMlp<T>(stream);
    }

    /// <summary>Loads weights into an existing <see cref="CnnMlpModel{T}"/> from a file path.</summary>
    public static void LoadFromFile<T>(CnnMlpModel<T> model, string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        using var stream = File.OpenRead(path);
        Load(model, stream);
    }

    /// <summary>Saves a <see cref="BatchParallelConcatModel{T}"/> to a file path.</summary>
    public static void SaveToFile<T>(BatchParallelConcatModel<T> model, string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        using var stream = File.Create(path);
        Save(model, stream);
    }

    /// <summary>Rebuilds a <see cref="BatchParallelConcatModel{T}"/> from a file path.</summary>
    public static BatchParallelConcatModel<T> LoadBatchParallelConcatFromFile<T>(string path)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        using var stream = File.OpenRead(path);
        return LoadBatchParallelConcat<T>(stream);
    }

    /// <summary>Flattens all tensors from a sequential model.</summary>
    public static T[] FlattenParameters<T>(Sequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => FlattenEntries(CollectSequentialEntries(model));

    /// <summary>Flattens all tensors from a batch sequential model.</summary>
    public static T[] FlattenParameters<T>(BatchSequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => FlattenEntries(CollectBatchSequentialEntries(model));

    /// <summary>Flattens all tensors from a hybrid CNN+MLP model.</summary>
    public static T[] FlattenParameters<T>(CnnMlpModel<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => FlattenEntries(CollectCnnMlpEntries(model));

    /// <summary>Flattens all tensors from a parallel-concat model.</summary>
    public static T[] FlattenParameters<T>(BatchParallelConcatModel<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => FlattenEntries(CollectBatchParallelConcatEntries(model));

    private static List<NamedTensor<T>> CollectBatchSequentialEntries<T>(BatchSequential<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
        => CollectBatchLayerEntries(model.Layers, "backbone");

    private static List<NamedTensor<T>> CollectBatchParallelConcatEntries<T>(BatchParallelConcatModel<T> model)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        var entries = CollectBatchLayerEntries(model.Left.Layers, "left");
        entries.AddRange(CollectBatchLayerEntries(model.Right.Layers, "right"));
        return entries;
    }

    private static void WriteHeader(BinaryWriter writer, ModelKind kind, Type elementType)
    {
        writer.Write(Magic);
        writer.Write(FormatVersion);
        writer.Write((byte)kind);
        writer.Write(elementType.FullName ?? elementType.Name);
    }

    private static ModelKind ReadHeader(BinaryReader reader, ModelKind expectedKind)
    {
        var magic = reader.ReadString();
        if (magic != Magic)
        {
            throw new InvalidDataException("Invalid model file header.");
        }

        var version = reader.ReadInt32();
        if (version != FormatVersion)
        {
            throw new NotSupportedException($"Expected format version {FormatVersion}, got {version}.");
        }

        var kind = (ModelKind)reader.ReadByte();
        _ = reader.ReadString();
        return kind;
    }

    private static void WriteArchitecture(BinaryWriter writer, IReadOnlyList<LayerDescriptor> layers)
    {
        writer.Write(layers.Count);
        foreach (var layer in layers)
        {
            WriteLayerDescriptor(writer, layer);
        }
    }

    private static void WriteParallelArchitecture(
        BinaryWriter writer,
        IReadOnlyList<LayerDescriptor> leftLayers,
        IReadOnlyList<LayerDescriptor> rightLayers)
    {
        writer.Write(leftLayers.Count);
        foreach (var layer in leftLayers)
        {
            WriteLayerDescriptor(writer, layer);
        }

        writer.Write(rightLayers.Count);
        foreach (var layer in rightLayers)
        {
            WriteLayerDescriptor(writer, layer);
        }
    }

    private static void WriteLayerDescriptor(BinaryWriter writer, LayerDescriptor layer)
    {
        writer.Write(layer.TypeId);
        writer.Write(layer.Name);
        writer.Write(layer.IntParameters.Length);
        foreach (var value in layer.IntParameters)
        {
            writer.Write(value);
        }

        writer.Write(layer.DoubleParameters.Length);
        foreach (var value in layer.DoubleParameters)
        {
            writer.Write(value);
        }
    }

    private static IReadOnlyList<LayerDescriptor> ReadArchitecture(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var layers = new LayerDescriptor[count];
        for (int i = 0; i < count; i++)
        {
            layers[i] = ReadLayerDescriptor(reader);
        }

        return layers;
    }

    private static (IReadOnlyList<LayerDescriptor> Left, IReadOnlyList<LayerDescriptor> Right) ReadParallelArchitecture(BinaryReader reader)
    {
        int leftCount = reader.ReadInt32();
        var left = new LayerDescriptor[leftCount];
        for (int i = 0; i < leftCount; i++)
        {
            left[i] = ReadLayerDescriptor(reader);
        }

        int rightCount = reader.ReadInt32();
        var right = new LayerDescriptor[rightCount];
        for (int i = 0; i < rightCount; i++)
        {
            right[i] = ReadLayerDescriptor(reader);
        }

        return (left, right);
    }

    private static LayerDescriptor ReadLayerDescriptor(BinaryReader reader)
    {
        var typeId = reader.ReadString();
        var name = reader.ReadString();
        int intCount = reader.ReadInt32();
        var ints = new int[intCount];
        for (int j = 0; j < intCount; j++)
        {
            ints[j] = reader.ReadInt32();
        }

        int doubleCount = reader.ReadInt32();
        var doubles = new double[doubleCount];
        for (int j = 0; j < doubleCount; j++)
        {
            doubles[j] = reader.ReadDouble();
        }

        return new LayerDescriptor(typeId, name, ints, doubles);
    }
}
