namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>Progress snapshot for long-running mode-decomposition algorithms.</summary>
public sealed class ModeDecompositionProgress
{
    public required string Algorithm { get; init; }

    /// <summary>0-based mode / IMF / component index currently being processed.</summary>
    public int CurrentMode { get; init; }

    /// <summary>Expected mode count when known; otherwise -1.</summary>
    public int TotalModes { get; init; } = -1;

    /// <summary>Inner iteration (sifting / ADMM / ensemble trial).</summary>
    public int Iteration { get; init; }

    /// <summary>Optional human-readable status.</summary>
    public string? Message { get; init; }

    /// <summary>Rough completion in [0, 1] when estimable; otherwise null.</summary>
    public double? Fraction { get; init; }
}

/// <summary>One sparse Hilbert-spectrum sample: amplitude at (time, frequency).</summary>
public readonly struct HilbertSpectrumSample
{
    public HilbertSpectrumSample(double timeSeconds, double frequencyHz, double amplitude)
    {
        TimeSeconds = timeSeconds;
        FrequencyHz = frequencyHz;
        Amplitude = amplitude;
    }

    public double TimeSeconds { get; }
    public double FrequencyHz { get; }
    public double Amplitude { get; }
}
