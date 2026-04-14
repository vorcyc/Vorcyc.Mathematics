namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// 1D Perlin (simplex) noise generator.
/// </summary>
public sealed class PerlinNoiseGenerator : ISampleGenerator
{
    private int _n;
    private readonly Random _rand = new();
    private readonly byte[] _permutation = new byte[512];

    /// <summary>
    /// Lower output level.
    /// </summary>
    public float Min { get; set; } = -1f;

    /// <summary>
    /// Upper output level.
    /// </summary>
    public float Max { get; set; } = 1f;

    /// <summary>
    /// Noise scale.
    /// </summary>
    public float Scale { get; set; } = 0.02f;

    /// <summary>
    /// Constructs generator and initializes permutation table.
    /// </summary>
    public PerlinNoiseGenerator()
    {
        _rand.NextBytes(_permutation);
    }

    /// <inheritdoc />
    public float NextSample()
    {
        var sample = GenerateSample(_n * Scale) * (Max - Min) / 2 + (Max + Min) / 2;
        _n++;
        return (float)sample;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _n = 0;
        _rand.NextBytes(_permutation);
    }

    private double GenerateSample(double x)
    {
        var i1 = (int)x < x ? (int)x : (int)x - 1;
        var i2 = (i1 + 1) & 0xff;
        var f1 = x - i1;
        var f2 = f1 - 1.0;

        i1 &= 0xff;

        return 0.188 * Lerp(Fade(f1), Gradient(_permutation[i1], f1),
                                      Gradient(_permutation[i2], f2));
    }

    private static double Gradient(int hash, double x)
    {
        var h = hash & 15;
        var g = 1.0 + (h & 7);
        return (h & 8) == 0 ? g * x : -g * x;
    }

    private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static double Lerp(double t, double a, double b) => a + t * (b - a);
}
