using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Legacy;

/// <summary>Loads ChromaGAN weights from the legacy half-precision HModel format.</summary>
internal sealed class HModelChromaGanWeights
{
  public Tensor<float>[] VGG_Conv1_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv1_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv2_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv2_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv3_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv3_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv4_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv4_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv5_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv5_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv6_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv6_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv7_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv7_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv8_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv8_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv9_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv9_Biases { get; init; } = null!;
  public Tensor<float>[] VGG_Conv10_Weights { get; init; } = [];
  public Tensor<float> VGG_Conv10_Biases { get; init; } = null!;

  public Tensor<float>[] Global_Conv1_Weights { get; init; } = [];
  public Tensor<float> Global_Conv1_Biases { get; init; } = null!;
  public Tensor<float> Global_BN1_Mean { get; init; } = null!;
  public Tensor<float> Global_BN1_Variance { get; init; } = null!;
  public Tensor<float> Global_BN1_Beta { get; init; } = null!;
  public Tensor<float> Global_BN1_Gamma { get; init; } = null!;
  public Tensor<float>[] Global_Conv2_Weights { get; init; } = [];
  public Tensor<float> Global_Conv2_Biases { get; init; } = null!;
  public Tensor<float> Global_BN2_Mean { get; init; } = null!;
  public Tensor<float> Global_BN2_Variance { get; init; } = null!;
  public Tensor<float> Global_BN2_Beta { get; init; } = null!;
  public Tensor<float> Global_BN2_Gamma { get; init; } = null!;
  public Tensor<float>[] Global_Conv3_Weights { get; init; } = [];
  public Tensor<float> Global_Conv3_Biases { get; init; } = null!;
  public Tensor<float> Global_BN3_Mean { get; init; } = null!;
  public Tensor<float> Global_BN3_Variance { get; init; } = null!;
  public Tensor<float> Global_BN3_Beta { get; init; } = null!;
  public Tensor<float> Global_BN3_Gamma { get; init; } = null!;
  public Tensor<float>[] Global_Conv4_Weights { get; init; } = [];
  public Tensor<float> Global_Conv4_Biases { get; init; } = null!;
  public Tensor<float> Global_BN4_Mean { get; init; } = null!;
  public Tensor<float> Global_BN4_Variance { get; init; } = null!;
  public Tensor<float> Global_BN4_Beta { get; init; } = null!;
  public Tensor<float> Global_BN4_Gamma { get; init; } = null!;

  public Tensor<float>[] Global2_Dense1_Weights { get; init; } = [];
  public Tensor<float> Global2_Dense1_Biases { get; init; } = null!;
  public Tensor<float>[] Global2_Dense2_Weights { get; init; } = [];
  public Tensor<float> Global2_Dense2_Biases { get; init; } = null!;
  public Tensor<float>[] Global2_Dense3_Weights { get; init; } = [];
  public Tensor<float> Global2_Dense3_Biases { get; init; } = null!;

  public Tensor<float>[] Midlevel_Conv1_Weights { get; init; } = [];
  public Tensor<float> Midlevel_Conv1_Biases { get; init; } = null!;
  public Tensor<float> Midlevel_BN1_Mean { get; init; } = null!;
  public Tensor<float> Midlevel_BN1_Variance { get; init; } = null!;
  public Tensor<float> Midlevel_BN1_Beta { get; init; } = null!;
  public Tensor<float> Midlevel_BN1_Gamma { get; init; } = null!;
  public Tensor<float>[] Midlevel_Conv2_Weights { get; init; } = [];
  public Tensor<float> Midlevel_Conv2_Biases { get; init; } = null!;
  public Tensor<float> Midlevel_BN2_Mean { get; init; } = null!;
  public Tensor<float> Midlevel_BN2_Variance { get; init; } = null!;
  public Tensor<float> Midlevel_BN2_Beta { get; init; } = null!;
  public Tensor<float> Midlevel_BN2_Gamma { get; init; } = null!;

  public Tensor<float>[] Output_Conv1_Weights { get; init; } = [];
  public Tensor<float> Output_Conv1_Biases { get; init; } = null!;
  public Tensor<float>[] Output_Conv2_Weights { get; init; } = [];
  public Tensor<float> Output_Conv2_Biases { get; init; } = null!;
  public Tensor<float>[] Output_Conv3_Weights { get; init; } = [];
  public Tensor<float> Output_Conv3_Biases { get; init; } = null!;
  public Tensor<float>[] Output_Conv4_Weights { get; init; } = [];
  public Tensor<float> Output_Conv4_Biases { get; init; } = null!;
  public Tensor<float>[] Output_Conv5_Weights { get; init; } = [];
  public Tensor<float> Output_Conv5_Biases { get; init; } = null!;
  public Tensor<float>[] Output_Conv6_Weights { get; init; } = [];
  public Tensor<float> Output_Conv6_Biases { get; init; } = null!;

  public static HModelChromaGanWeights Load(Stream stream)
  {
    using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
    return new HModelChromaGanWeights
    {
      VGG_Conv1_Weights = LoadWeights(reader, 3, 3, 3, 64),
      VGG_Conv1_Biases = LoadBiases(reader, 64),
      VGG_Conv2_Weights = LoadWeights(reader, 3, 3, 64, 64),
      VGG_Conv2_Biases = LoadBiases(reader, 64),
      VGG_Conv3_Weights = LoadWeights(reader, 3, 3, 64, 128),
      VGG_Conv3_Biases = LoadBiases(reader, 128),
      VGG_Conv4_Weights = LoadWeights(reader, 3, 3, 128, 128),
      VGG_Conv4_Biases = LoadBiases(reader, 128),
      VGG_Conv5_Weights = LoadWeights(reader, 3, 3, 128, 256),
      VGG_Conv5_Biases = LoadBiases(reader, 256),
      VGG_Conv6_Weights = LoadWeights(reader, 3, 3, 256, 256),
      VGG_Conv6_Biases = LoadBiases(reader, 256),
      VGG_Conv7_Weights = LoadWeights(reader, 3, 3, 256, 256),
      VGG_Conv7_Biases = LoadBiases(reader, 256),
      VGG_Conv8_Weights = LoadWeights(reader, 3, 3, 256, 512),
      VGG_Conv8_Biases = LoadBiases(reader, 512),
      VGG_Conv9_Weights = LoadWeights(reader, 3, 3, 512, 512),
      VGG_Conv9_Biases = LoadBiases(reader, 512),
      VGG_Conv10_Weights = LoadWeights(reader, 3, 3, 512, 512),
      VGG_Conv10_Biases = LoadBiases(reader, 512),
      Global_Conv1_Weights = LoadWeights(reader, 3, 3, 512, 512),
      Global_Conv1_Biases = LoadBiases(reader, 512),
      Global_BN1_Mean = LoadBiases(reader, 512),
      Global_BN1_Variance = LoadBiases(reader, 512),
      Global_BN1_Beta = LoadBiases(reader, 512),
      Global_BN1_Gamma = LoadBiases(reader, 512),
      Global_Conv2_Weights = LoadWeights(reader, 3, 3, 512, 512),
      Global_Conv2_Biases = LoadBiases(reader, 512),
      Global_BN2_Mean = LoadBiases(reader, 512),
      Global_BN2_Variance = LoadBiases(reader, 512),
      Global_BN2_Beta = LoadBiases(reader, 512),
      Global_BN2_Gamma = LoadBiases(reader, 512),
      Global_Conv3_Weights = LoadWeights(reader, 3, 3, 512, 512),
      Global_Conv3_Biases = LoadBiases(reader, 512),
      Global_BN3_Mean = LoadBiases(reader, 512),
      Global_BN3_Variance = LoadBiases(reader, 512),
      Global_BN3_Beta = LoadBiases(reader, 512),
      Global_BN3_Gamma = LoadBiases(reader, 512),
      Global_Conv4_Weights = LoadWeights(reader, 3, 3, 512, 512),
      Global_Conv4_Biases = LoadBiases(reader, 512),
      Global_BN4_Mean = LoadBiases(reader, 512),
      Global_BN4_Variance = LoadBiases(reader, 512),
      Global_BN4_Beta = LoadBiases(reader, 512),
      Global_BN4_Gamma = LoadBiases(reader, 512),
      Global2_Dense1_Weights = LoadWeights(reader, 1, 1, 25088, 1024),
      Global2_Dense1_Biases = LoadBiases(reader, 1024),
      Global2_Dense2_Weights = LoadWeights(reader, 1, 1, 1024, 512),
      Global2_Dense2_Biases = LoadBiases(reader, 512),
      Global2_Dense3_Weights = LoadWeights(reader, 1, 1, 512, 256),
      Global2_Dense3_Biases = LoadBiases(reader, 256),
      Midlevel_Conv1_Weights = LoadWeights(reader, 3, 3, 512, 512),
      Midlevel_Conv1_Biases = LoadBiases(reader, 512),
      Midlevel_BN1_Mean = LoadBiases(reader, 512),
      Midlevel_BN1_Variance = LoadBiases(reader, 512),
      Midlevel_BN1_Beta = LoadBiases(reader, 512),
      Midlevel_BN1_Gamma = LoadBiases(reader, 512),
      Midlevel_Conv2_Weights = LoadWeights(reader, 3, 3, 512, 256),
      Midlevel_Conv2_Biases = LoadBiases(reader, 256),
      Midlevel_BN2_Mean = LoadBiases(reader, 256),
      Midlevel_BN2_Variance = LoadBiases(reader, 256),
      Midlevel_BN2_Beta = LoadBiases(reader, 256),
      Midlevel_BN2_Gamma = LoadBiases(reader, 256),
      Output_Conv1_Weights = LoadWeights(reader, 1, 1, 512, 256),
      Output_Conv1_Biases = LoadBiases(reader, 256),
      Output_Conv2_Weights = LoadWeights(reader, 3, 3, 256, 128),
      Output_Conv2_Biases = LoadBiases(reader, 128),
      Output_Conv3_Weights = LoadWeights(reader, 3, 3, 128, 64),
      Output_Conv3_Biases = LoadBiases(reader, 64),
      Output_Conv4_Weights = LoadWeights(reader, 3, 3, 64, 64),
      Output_Conv4_Biases = LoadBiases(reader, 64),
      Output_Conv5_Weights = LoadWeights(reader, 3, 3, 64, 32),
      Output_Conv5_Biases = LoadBiases(reader, 32),
      Output_Conv6_Weights = LoadWeights(reader, 3, 3, 32, 2),
      Output_Conv6_Biases = LoadBiases(reader, 2),
    };
  }

  public static HModelChromaGanWeights LoadFromFile(string path)
  {
    using var stream = File.OpenRead(path);
    return Load(stream);
  }

  private static Tensor<float>[] LoadWeights(BinaryReader reader, int w, int h, int d, int n)
  {
    var result = new Tensor<float>[n];
    for (int i = 0; i < n; i++)
    {
      result[i] = new Tensor<float>(w, h, d);
      var tensor = result[i];
      for (int z = 0; z < d; z++)
      {
        for (int y = 0; y < h; y++)
        {
          for (int x = 0; x < w; x++)
          {
            tensor[x, y, z] = HalfHelper.HalfToSingle(reader.ReadUInt16());
          }
        }
      }
    }

    return result;
  }

  private static Tensor<float> LoadBiases(BinaryReader reader, int n)
  {
    var result = new Tensor<float>(1, 1, n);
    for (int i = 0; i < n; i++)
    {
      result[0, 0, i] = HalfHelper.HalfToSingle(reader.ReadUInt16());
    }

    return result;
  }
}
