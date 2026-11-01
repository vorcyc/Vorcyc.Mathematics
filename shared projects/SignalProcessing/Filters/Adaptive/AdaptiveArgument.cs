namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

using Vorcyc.Mathematics.Numerics;

/// <summary>
/// Argument checks of the adaptive filters. The general <see cref="Guard"/> helpers let NaN (and some of them infinity) pass, which ends as NaN
/// weights with the first sample, so the filters check their numeric arguments with these helpers.
/// </summary>
internal static class AdaptiveArgument
{
    /// <summary>
    /// The value must be a finite number greater than zero.
    /// </summary>
    public static void Positive(float value, string name)
    {
        if (!(float.IsFinite(value) && value > 0))
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a positive finite number.");
    }

    /// <summary>
    /// The value must be a finite number greater than zero.
    /// </summary>
    public static void Positive(double value, string name)
    {
        if (!(double.IsFinite(value) && value > 0))
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a positive finite number.");
    }

    /// <summary>
    /// The value must be a finite number that is not negative.
    /// </summary>
    public static void NonNegative(float value, string name)
    {
        if (!(float.IsFinite(value) && value >= 0))
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a finite number that is not negative.");
    }

    /// <summary>
    /// The value must be a finite number.
    /// </summary>
    public static void Finite(float value, string name)
    {
        if (!float.IsFinite(value))
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a finite number.");
    }

    /// <summary>
    /// The value must lie in [<paramref name="low"/>, <paramref name="high"/>] (NaN and infinity do not).
    /// </summary>
    public static void InRange(float value, float low, float high, string name)
    {
        if (!(value >= low && value <= high))
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be in [{low}, {high}].");
    }

    /// <summary>
    /// Every value must be a finite number: weights that are not make every output, and so every error, non-finite, and the filter would never adapt (or recover) again.
    /// </summary>
    public static void AllFinite(ReadOnlySpan<float> values, string name)
    {
        foreach (var value in values)
        {
            if (!float.IsFinite(value))
                throw new ArgumentException($"{name} must contain finite numbers only.", name);
        }
    }

    /// <summary>
    /// Every real and imaginary part must be a finite number.
    /// </summary>
    public static void AllFinite(ReadOnlySpan<ComplexFp32> values, string name)
    {
        foreach (var value in values)
        {
            if (!float.IsFinite(value.Real) || !float.IsFinite(value.Imaginary))
                throw new ArgumentException($"{name} must contain finite numbers only.", name);
        }
    }

    /// <summary>
    /// The leakage must be finite and not negative, and the share of the weights that is forgotten per sample, leakage · <paramref name="mu"/>, must not exceed 1.
    /// </summary>
    public static void Leakage(float leakage, float mu)
    {
        NonNegative(leakage, nameof(leakage));
        if ((double)leakage * mu > 1)
            throw new ArgumentOutOfRangeException(nameof(leakage), leakage, "leakage · mu must not exceed 1: it is the share of the weights that is forgotten per sample.");
    }

    /// <summary>
    /// The number of elements of an array that the filter needs (computed without overflow) must not exceed the maximum length of an array.
    /// </summary>
    /// <returns>The number of elements as an int</returns>
    public static int Count(long count, string name)
    {
        if (count > Array.MaxLength)
            throw new ArgumentOutOfRangeException(name, "The filter is too large: its state needs more elements than an array can hold.");

        return (int)count;
    }
}
