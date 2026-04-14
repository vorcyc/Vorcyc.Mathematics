using Colorization_example.Models;

namespace Colorization_example.Serialization;

internal static class ChromaGanSerializer
{
  public const byte ModelKind = 5;

  public static void SaveToFile(ChromaGanModel model, string path)
  {
    using var stream = File.Create(path);
    Save(model, stream);
  }

  public static void Save(ChromaGanModel model, Stream stream)
  {
    var entries = VmdlTensorArchive.BuildEntries(model.EnumerateWeightTensors());
    VmdlTensorArchive.Write(stream, ModelKind, entries);
  }

  public static ChromaGanModel LoadFromFile(string path)
  {
    using var stream = File.OpenRead(path);
    return Load(stream);
  }

  public static ChromaGanModel Load(Stream stream)
  {
    var model = ChromaGanModel.Create();
    LoadInto(model, stream);
    return model;
  }

  public static void LoadInto(ChromaGanModel model, Stream stream)
  {
    var entries = VmdlTensorArchive.BuildEntries(model.EnumerateWeightTensors());
    VmdlTensorArchive.Read(stream, ModelKind, entries);
  }
}
