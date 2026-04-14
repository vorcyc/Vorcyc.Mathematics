using System.Numerics;
using System.Text;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Serialization;

internal readonly record struct NamedTensorEntry(string Name, Tensor<float> Tensor);

/// <summary>Shared VMDL v3 tensor archive read/write helpers for custom vision models.</summary>
internal static class VmdlTensorArchive
{
  public const string Magic = "VMDL";
  public const int FormatVersion = 3;

  public static void Write(Stream stream, byte modelKind, IReadOnlyList<NamedTensorEntry> entries)
  {
    using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
    writer.Write(Magic);
    writer.Write(FormatVersion);
    writer.Write(modelKind);
    writer.Write(typeof(float).FullName ?? nameof(Single));
    writer.Write(0);
    WriteEntries(writer, entries);
  }

  public static void Read(Stream stream, byte expectedModelKind, IReadOnlyList<NamedTensorEntry> entries)
  {
    using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
    ReadHeader(reader, expectedModelKind);
    _ = reader.ReadInt32();
    ReadEntries(reader, entries);
  }

  public static List<NamedTensorEntry> BuildEntries(
    IEnumerable<(string? Name, Tensor<float> Tensor)> tensors)
  {
    var entries = new List<NamedTensorEntry>();
    foreach (var (name, tensor) in tensors)
    {
      var entryName = string.IsNullOrEmpty(name) ? $"param.{entries.Count}" : name;
      entries.Add(new NamedTensorEntry(entryName, tensor));
    }

    return entries;
  }

  private static void ReadHeader(BinaryReader reader, byte expectedModelKind)
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

    var kind = reader.ReadByte();
    if (kind != expectedModelKind)
    {
      throw new InvalidDataException($"Unexpected model kind {kind}, expected {expectedModelKind}.");
    }

    _ = reader.ReadString();
  }

  private static void WriteEntries(BinaryWriter writer, IReadOnlyList<NamedTensorEntry> entries)
  {
    writer.Write(entries.Count);
    foreach (var entry in entries)
    {
      writer.Write(entry.Name);
      writer.Write(entry.Tensor.Width);
      writer.Write(entry.Tensor.Height);
      writer.Write(entry.Tensor.Depth);
      foreach (var value in entry.Tensor.Values)
      {
        writer.Write((double)value);
      }
    }
  }

  private static void ReadEntries(BinaryReader reader, IReadOnlyList<NamedTensorEntry> expected)
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

      var span = expected[i].Tensor.Values;
      for (int j = 0; j < span.Length; j++)
      {
        span[j] = (float)reader.ReadDouble();
      }
    }
  }
}
