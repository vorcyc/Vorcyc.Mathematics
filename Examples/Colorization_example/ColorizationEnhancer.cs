using Colorization_example.Models;
using Colorization_example.Serialization;
using Colorization_example.Vision;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Colorization_example;

/// <summary>Unified colorization helper for ColorNet and ChromaGAN scenarios.</summary>
public sealed class ColorizationEnhancer : IDisposable
{
  public enum Scenario
  {
    /// <summary>Real-world scenes (ColorNet).</summary>
    RealisticView,
    /// <summary>People and group photos (ChromaGAN).</summary>
    People,
  }

  private readonly Scenario _scenario;
  private readonly ColorNetModel? _colorNet;
  private readonly ChromaGanModel? _chromaGan;

  public ColorizationEnhancer(Scenario scenario, string modelPath)
  {
    if (string.IsNullOrWhiteSpace(modelPath))
    {
      throw new ArgumentException("Model path is required.", nameof(modelPath));
    }

    if (!File.Exists(modelPath))
    {
      throw new FileNotFoundException("Model file was not found.", modelPath);
    }

    _scenario = scenario;
    switch (scenario)
    {
      case Scenario.RealisticView:
        _colorNet = ColorNetSerializer.LoadFromFile(modelPath);
        break;
      case Scenario.People:
        _chromaGan = ChromaGanSerializer.LoadFromFile(modelPath);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
    }
  }

  public Mat Colorize(Mat image)
  {
    ArgumentNullException.ThrowIfNull(image);
    if (image.IsEmpty)
    {
      throw new ArgumentException("Input image is empty.", nameof(image));
    }

    return _scenario switch
    {
      Scenario.RealisticView => ColorizeWithColorNet(image),
      Scenario.People => ColorizeWithChromaGan(image),
      _ => throw new InvalidOperationException($"Unsupported scenario: {_scenario}"),
    };
  }

  public Mat Colorize(string imagePath)
  {
    if (string.IsNullOrWhiteSpace(imagePath))
    {
      throw new ArgumentException("Image path is required.", nameof(imagePath));
    }

    if (!File.Exists(imagePath))
    {
      throw new FileNotFoundException("Input image was not found.", imagePath);
    }

    using var image = CvInvoke.Imread(imagePath, ImreadModes.Color);
    if (image.IsEmpty)
    {
      throw new InvalidDataException($"Failed to read image: {imagePath}");
    }

    return Colorize(image);
  }

  public void Dispose()
  {
  }

  private Mat ColorizeWithColorNet(Mat image)
  {
    var (original, resized224) = ColorNetIo.Preprocess(image);
    var chroma = _colorNet!.Colorize(original, resized224);
    return ColorNetIo.Deprocess(original, chroma);
  }

  private Mat ColorizeWithChromaGan(Mat image)
  {
    using var original = image.Clone();
    var input = ChromaGanIo.Preprocess(image);
    var chroma = _chromaGan!.Colorize(input);
    return ChromaGanIo.Deprocess(original, chroma);
  }
}
