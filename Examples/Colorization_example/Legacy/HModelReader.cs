using Vorcyc.Mathematics.LinearAlgebra;

namespace Colorization_example.Legacy;

/// <summary>Loads ColorNet weights from the legacy half-precision HModel format.</summary>
internal sealed class HModelColorNetWeights
{
    public Tensor<float>[] LLFN_Conv1_Weights { get; init; } = [];
    public Tensor<float> LLFN_Conv1_Biases { get; init; } = null!;
    public Tensor<float>[] LLFN_Conv2_Weights { get; init; } = [];
    public Tensor<float> LLFN_Conv2_Biases { get; init; } = null!;
    public Tensor<float>[] LLFN_Conv3_Weights { get; init; } = [];
    public Tensor<float> LLFN_Conv3_Biases { get; init; } = null!;
    public Tensor<float>[] LLFN_Conv4_Weights { get; init; } = [];
    public Tensor<float> LLFN_Conv4_Biases { get; init; } = null!;
    public Tensor<float>[] LLFN_Conv5_Weights { get; init; } = [];
    public Tensor<float> LLFN_Conv5_Biases { get; init; } = null!;
    public Tensor<float>[] LLFN_Conv6_Weights { get; init; } = [];
    public Tensor<float> LLFN_Conv6_Biases { get; init; } = null!;

    public Tensor<float>[] MLFN_Conv1_Weights { get; init; } = [];
    public Tensor<float> MLFN_Conv1_Biases { get; init; } = null!;
    public Tensor<float>[] MLFN_Conv2_Weights { get; init; } = [];
    public Tensor<float> MLFN_Conv2_Biases { get; init; } = null!;

    public Tensor<float>[] GFN_Conv1_Weights { get; init; } = [];
    public Tensor<float> GFN_Conv1_Biases { get; init; } = null!;
    public Tensor<float>[] GFN_Conv2_Weights { get; init; } = [];
    public Tensor<float> GFN_Conv2_Biases { get; init; } = null!;
    public Tensor<float>[] GFN_Conv3_Weights { get; init; } = [];
    public Tensor<float> GFN_Conv3_Biases { get; init; } = null!;
    public Tensor<float>[] GFN_Conv4_Weights { get; init; } = [];
    public Tensor<float> GFN_Conv4_Biases { get; init; } = null!;
    public Tensor<float>[] GFN_Linear1_Weights { get; init; } = [];
    public Tensor<float> GFN_Linear1_Biases { get; init; } = null!;
    public Tensor<float>[] GFN_Linear2_Weights { get; init; } = [];
    public Tensor<float> GFN_Linear2_Biases { get; init; } = null!;
    public Tensor<float>[] GFN_Linear3_Weights { get; init; } = [];
    public Tensor<float> GFN_Linear3_Biases { get; init; } = null!;

    public Tensor<float>[] CN_Conv1_Weights { get; init; } = [];
    public Tensor<float> CN_Conv1_Biases { get; init; } = null!;
    public Tensor<float>[] CN_Conv2_Weights { get; init; } = [];
    public Tensor<float> CN_Conv2_Biases { get; init; } = null!;
    public Tensor<float>[] CN_Conv3_Weights { get; init; } = [];
    public Tensor<float> CN_Conv3_Biases { get; init; } = null!;
    public Tensor<float>[] CN_Conv4_Weights { get; init; } = [];
    public Tensor<float> CN_Conv4_Biases { get; init; } = null!;
    public Tensor<float>[] CN_Conv5_Weights { get; init; } = [];
    public Tensor<float> CN_Conv5_Biases { get; init; } = null!;
    public Tensor<float>[] CN_Conv6_Weights { get; init; } = [];
    public Tensor<float> CN_Conv6_Biases { get; init; } = null!;

    public static HModelColorNetWeights Load(Stream stream)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        return new HModelColorNetWeights
        {
            LLFN_Conv1_Weights = LoadWeights(reader, 3, 3, 1, 64),
            LLFN_Conv1_Biases = LoadBiases(reader, 64),
            LLFN_Conv2_Weights = LoadWeights(reader, 3, 3, 64, 128),
            LLFN_Conv2_Biases = LoadBiases(reader, 128),
            LLFN_Conv3_Weights = LoadWeights(reader, 3, 3, 128, 128),
            LLFN_Conv3_Biases = LoadBiases(reader, 128),
            LLFN_Conv4_Weights = LoadWeights(reader, 3, 3, 128, 256),
            LLFN_Conv4_Biases = LoadBiases(reader, 256),
            LLFN_Conv5_Weights = LoadWeights(reader, 3, 3, 256, 256),
            LLFN_Conv5_Biases = LoadBiases(reader, 256),
            LLFN_Conv6_Weights = LoadWeights(reader, 3, 3, 256, 512),
            LLFN_Conv6_Biases = LoadBiases(reader, 512),
            MLFN_Conv1_Weights = LoadWeights(reader, 3, 3, 512, 512),
            MLFN_Conv1_Biases = LoadBiases(reader, 512),
            MLFN_Conv2_Weights = LoadWeights(reader, 3, 3, 512, 256),
            MLFN_Conv2_Biases = LoadBiases(reader, 256),
            GFN_Conv1_Weights = LoadWeights(reader, 3, 3, 512, 512),
            GFN_Conv1_Biases = LoadBiases(reader, 512),
            GFN_Conv2_Weights = LoadWeights(reader, 3, 3, 512, 512),
            GFN_Conv2_Biases = LoadBiases(reader, 512),
            GFN_Conv3_Weights = LoadWeights(reader, 3, 3, 512, 512),
            GFN_Conv3_Biases = LoadBiases(reader, 512),
            GFN_Conv4_Weights = LoadWeights(reader, 3, 3, 512, 512),
            GFN_Conv4_Biases = LoadBiases(reader, 512),
            GFN_Linear1_Weights = LoadWeights(reader, 1, 1, 25088, 1024),
            GFN_Linear1_Biases = LoadBiases(reader, 1024),
            GFN_Linear2_Weights = LoadWeights(reader, 1, 1, 1024, 512),
            GFN_Linear2_Biases = LoadBiases(reader, 512),
            GFN_Linear3_Weights = LoadWeights(reader, 1, 1, 512, 256),
            GFN_Linear3_Biases = LoadBiases(reader, 256),
            CN_Conv1_Weights = LoadWeights(reader, 3, 3, 512, 256),
            CN_Conv1_Biases = LoadBiases(reader, 256),
            CN_Conv2_Weights = LoadWeights(reader, 3, 3, 256, 128),
            CN_Conv2_Biases = LoadBiases(reader, 128),
            CN_Conv3_Weights = LoadWeights(reader, 3, 3, 128, 64),
            CN_Conv3_Biases = LoadBiases(reader, 64),
            CN_Conv4_Weights = LoadWeights(reader, 3, 3, 64, 64),
            CN_Conv4_Biases = LoadBiases(reader, 64),
            CN_Conv5_Weights = LoadWeights(reader, 3, 3, 64, 32),
            CN_Conv5_Biases = LoadBiases(reader, 32),
            CN_Conv6_Weights = LoadWeights(reader, 3, 3, 32, 2),
            CN_Conv6_Biases = LoadBiases(reader, 2),
        };
    }

    public static HModelColorNetWeights LoadFromFile(string path)
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
