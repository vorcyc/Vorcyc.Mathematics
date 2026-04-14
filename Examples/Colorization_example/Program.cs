using Colorization_example.Models;
using Colorization_example.Serialization;
using Emgu.CV;

namespace Colorization_example;

internal static class Program
{
  private const string DefaultColorNetHModel = @"model_zoo\vision\colorization\Colornet.HModel";
  private const string DefaultChromaGanHModel = @"model_zoo\vision\colorization\ChromaGAN.HModel";
  private const string DefaultColorNetVmdl = @"models\Colornet.vmdl";
  private const string DefaultChromaGanVmdl = @"models\ChromaGAN.vmdl";

  public static int Main(string[] args)
  {
    try
    {
      if (args.Length == 0)
      {
        PrintUsage();
        return 1;
      }

      return args[0].ToLowerInvariant() switch
      {
        "export" => Export(args),
        "colorize" => Colorize(args),
        _ => UnknownCommand(args[0]),
      };
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine(ex);
      return 1;
    }
  }

  private static int Export(string[] args)
  {
    string modelName = args.Length > 1 ? args[1].ToLowerInvariant() : "colornet";
    string hmodelPath = args.Length > 2 ? args[2] : GetDefaultHModelPath(modelName);
    string vmdlPath = args.Length > 3 ? args[3] : GetDefaultVmdlPath(modelName);

    if (!File.Exists(hmodelPath))
    {
      Console.Error.WriteLine($"Legacy model not found: {hmodelPath}");
      return 1;
    }

    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(vmdlPath))!);
    switch (modelName)
    {
      case "colornet":
        ColorNetSerializer.SaveToFile(ColorNetModel.FromHModel(hmodelPath), vmdlPath);
        break;
      case "chromagan":
        ChromaGanSerializer.SaveToFile(ChromaGanModel.FromHModel(hmodelPath), vmdlPath);
        break;
      default:
        Console.Error.WriteLine($"Unknown model '{modelName}'. Use colornet or chromagan.");
        return 1;
    }

    Console.WriteLine($"Exported {modelName} model to {vmdlPath}");
    return 0;
  }

  private static int Colorize(string[] args)
  {
    if (args.Length < 2)
    {
      Console.Error.WriteLine("Usage: colorize <image-path> [output-path] [--scenario realistic|people] [--model <vmdl-path>]");
      return 1;
    }

    string imagePath = args[1];
    string outputPath = "result.jpg";
    string scenarioName = "realistic";
    string? modelPath = null;

    for (int i = 2; i < args.Length; i++)
    {
      if (args[i] == "--scenario" && i + 1 < args.Length)
      {
        scenarioName = args[++i].ToLowerInvariant();
        continue;
      }

      if (args[i] == "--model" && i + 1 < args.Length)
      {
        modelPath = args[++i];
        continue;
      }

      outputPath = args[i];
    }

    var scenario = ParseScenario(scenarioName);
    modelPath ??= GetDefaultVmdlPath(scenario);
    EnsureModelExists(scenario, modelPath);

    if (!File.Exists(modelPath))
    {
      Console.Error.WriteLine($"Model not found: {modelPath}");
      return 1;
    }

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    using var enhancer = new ColorizationEnhancer(ToEnhancerScenario(scenario), modelPath);
    using var result = enhancer.Colorize(imagePath);
    stopwatch.Stop();

    CvInvoke.Imwrite(outputPath, result);
    Console.WriteLine($"Saved colorized image to {outputPath} ({stopwatch.Elapsed})");
    return 0;
  }

  private static void EnsureModelExists(string scenario, string modelPath)
  {
    if (File.Exists(modelPath))
    {
      return;
    }

    string hmodelPath = GetDefaultHModelPath(scenario);
    if (!File.Exists(hmodelPath))
    {
      return;
    }

    Console.WriteLine($"VMDL model not found, exporting from legacy HModel: {hmodelPath}");
    Export(["export", scenario, hmodelPath, modelPath]);
  }

  private static string GetDefaultHModelPath(string scenario)
    => scenario switch
    {
      "chromagan" or "people" => DefaultChromaGanHModel,
      _ => DefaultColorNetHModel,
    };

  private static string GetDefaultVmdlPath(string scenario)
    => scenario switch
    {
      "chromagan" or "people" => DefaultChromaGanVmdl,
      _ => DefaultColorNetVmdl,
    };

  private static string ParseScenario(string scenarioName)
    => scenarioName switch
    {
      "people" or "chromagan" => "chromagan",
      "realistic" or "colornet" => "colornet",
      _ => throw new ArgumentException($"Unknown scenario '{scenarioName}'. Use realistic or people."),
    };

  private static ColorizationEnhancer.Scenario ToEnhancerScenario(string scenario)
    => scenario == "chromagan"
      ? ColorizationEnhancer.Scenario.People
      : ColorizationEnhancer.Scenario.RealisticView;

  private static int UnknownCommand(string command)
  {
    Console.Error.WriteLine($"Unknown command: {command}");
    PrintUsage();
    return 1;
  }

  private static void PrintUsage()
  {
    Console.WriteLine("Image colorization example (ColorNet + ChromaGAN)");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  export <colornet|chromagan> [hmodel-path] [vmdl-path]");
    Console.WriteLine("  colorize <image-path> [output-path] [--scenario realistic|people] [--model <vmdl-path>]");
    Console.WriteLine();
    Console.WriteLine("Defaults:");
    Console.WriteLine($"  ColorNet HModel:   {DefaultColorNetHModel}");
    Console.WriteLine($"  ColorNet VMDL:     {DefaultColorNetVmdl}");
    Console.WriteLine($"  ChromaGAN HModel:  {DefaultChromaGanHModel}");
    Console.WriteLine($"  ChromaGAN VMDL:    {DefaultChromaGanVmdl}");
  }
}
