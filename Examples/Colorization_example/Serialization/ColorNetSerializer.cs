using Colorization_example.Models;
using Vorcyc.Mathematics.DeepLearning;

namespace Colorization_example.Serialization;

internal static class ColorNetSerializer
{
  public const byte ModelKind = 4;

  public static void SaveToFile(ColorNetModel model, string path)
  {
    using var stream = File.Create(path);
    Save(model, stream);
  }

  public static void Save(ColorNetModel model, Stream stream)
  {
    var entries = VmdlTensorArchive.BuildEntries(
      model.Parameters.Select(p => (p.Name, p.Value)));
    VmdlTensorArchive.Write(stream, ModelKind, entries);
  }

  public static ColorNetModel LoadFromFile(string path)
  {
    using var stream = File.OpenRead(path);
    return Load(stream);
  }

  public static ColorNetModel Load(Stream stream)
  {
    var model = ColorNetModel.Create();
    LoadInto(model, stream);
    return model;
  }

  public static void LoadInto(ColorNetModel model, Stream stream)
  {
    var entries = VmdlTensorArchive.BuildEntries(
      model.Parameters.Select(p => (p.Name, p.Value)));
    VmdlTensorArchive.Read(stream, ModelKind, entries);
  }
}
