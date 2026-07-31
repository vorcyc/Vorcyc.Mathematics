using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms;

/// <summary>
/// Convert continuous (analog) pole/zero models to discrete transfer functions via bilinear transform.
/// </summary>
public static class AnalogDigitalTransform
{
    /// <summary>
    /// Bilinear (Tustin) map of analog zeros/poles to a discrete <see cref="TransferFunction"/>.
    /// <paramref name="normalizedCutoff"/> in (0, 0.5) enables frequency pre-warping (same as IIR prototype design).
    /// When null, poles/zeros are assumed already scaled in the analog s-plane.
    /// </summary>
    public static TransferFunction Bilinear(
        ReadOnlySpan<ComplexFp32> analogZeros,
        ReadOnlySpan<ComplexFp32> analogPoles,
        float gain = 1f,
        float? normalizedCutoff = null,
        float? normalizeAtNormalizedFreq = 0f)
    {
        if (analogPoles.IsEmpty)
            throw new ArgumentException("At least one analog pole is required.", nameof(analogPoles));

        var (z, p) = LaplaceTransform.BilinearMap(analogZeros, analogPoles, normalizedCutoff);
        var tf = new TransferFunction(z, p, gain);

        if (normalizeAtNormalizedFreq is float fn)
            tf.NormalizeAt(fn);

        return tf;
    }

    /// <summary>
    /// Low-pass prototype style: scale analog poles by tan(π f_c), bilinear, zeros at z=−1 when none given.
    /// Equivalent path to <c>DesignFilter.IirLpTf</c> for a supplied prototype.
    /// </summary>
    public static TransferFunction BilinearLowpass(
        float normalizedCutoff,
        ReadOnlySpan<ComplexFp32> analogPoles,
        ReadOnlySpan<ComplexFp32> analogZeros = default,
        float gain = 1f)
        => Bilinear(analogZeros, analogPoles, gain, normalizedCutoff, normalizeAtNormalizedFreq: 0f);

    /// <summary>
    /// Inverse bilinear: discrete zeros/poles → analog (drops z≈−1 entries as zeros at ∞).
    /// </summary>
    public static (ComplexFp32[] Zeros, ComplexFp32[] Poles) InverseBilinear(
        ReadOnlySpan<ComplexFp32> digitalZeros,
        ReadOnlySpan<ComplexFp32> digitalPoles,
        float? normalizedCutoff = null)
        => LaplaceTransform.InverseBilinearMap(digitalZeros, digitalPoles, normalizedCutoff);
}
