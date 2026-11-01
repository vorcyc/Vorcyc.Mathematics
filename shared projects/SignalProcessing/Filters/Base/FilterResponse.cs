using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Frequency response evaluation in double precision, shared by <see cref="TransferFunction"/> and <see cref="TransferFunction{T}"/>.
/// </summary>
internal static class FilterResponse
{
    // 1 dB as a natural logarithm of the magnitude ratio
    private const double MaxLogDisagreement = 0.1151292546497023;

    /// <summary>
    /// Gets the factor that brings the magnitude of <c>B(z) / A(z)</c> to 1 at the digital frequency <paramref name="omega"/> (radians)
    /// when it multiplies the numerator.
    /// </summary>
    /// <remarks>
    /// The stored coefficients are what a filter runs with, so their response sets the gain (evaluated in double precision).
    /// The zeros and poles decide when they are given (their numbers match the degrees of the polynomials) and the coefficients no longer
    /// agree with them to within 1 dB at <paramref name="omega"/>: the expanded polynomials of a narrow-band or high-order design
    /// cancel completely in the rounding of the coefficients (the sum of the numerator drops below the rounding of its terms),
    /// so the value of the coefficients is rounding noise. The gain is a product of distances to the zeros and poles then.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The magnitude is zero, infinite or not a number at <paramref name="omega"/>.</exception>
    public static double UnitGainScale(double[] numerator, double[] denominator, Complex[]? zeros, Complex[]? poles, double omega)
    {
        var scale = Complex.Abs(Evaluate(denominator, omega)) / Complex.Abs(Evaluate(numerator, omega));

        if (zeros is not null && poles is not null && zeros.Length == numerator.Length - 1 && poles.Length == denominator.Length - 1)
        {
            var w = Complex.FromPolarCoordinates(1, omega);
            var logScale = Math.Log(Math.Abs(denominator[0])) - Math.Log(Math.Abs(numerator[0]));

            foreach (var p in poles)
                logScale += Math.Log(Complex.Abs(w - p));

            foreach (var z in zeros)
                logScale -= Math.Log(Complex.Abs(w - z));

            var rootsScale = Math.Exp(logScale);

            // also true when the scale of the coefficients is not a number
            if (!(Math.Abs(Math.Log(scale / rootsScale)) <= MaxLogDisagreement))
                scale = rootsScale;
        }

        if (!(scale > 0) || double.IsInfinity(scale))
            throw new InvalidOperationException("The frequency response is zero, infinite or not defined at the normalization frequency.");

        return scale;
    }

    /// <summary>
    /// Converts coefficients to double precision.
    /// </summary>
    public static double[] Coefficients<T>(T[] values)
        where T : IFloatingPointIeee754<T>
    {
        var result = new double[values.Length];

        for (var i = 0; i < values.Length; i++)
            result[i] = double.CreateChecked(values[i]);

        return result;
    }

    /// <summary>
    /// Converts zeros (or poles) to double precision.
    /// </summary>
    public static Complex[]? Roots(ComplexFp32[]? roots)
    {
        if (roots is null)
            return null;

        var result = new Complex[roots.Length];

        for (var i = 0; i < roots.Length; i++)
            result[i] = new Complex(roots[i].Real, roots[i].Imaginary);

        return result;
    }

    /// <summary>
    /// Converts zeros (or poles) to double precision.
    /// </summary>
    public static Complex[]? Roots<T>(Complex<T>[]? roots)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (roots is null)
            return null;

        var result = new Complex[roots.Length];

        for (var i = 0; i < roots.Length; i++)
            result[i] = new Complex(double.CreateChecked(roots[i].Real), double.CreateChecked(roots[i].Imaginary));

        return result;
    }

    /// <summary>
    /// Converts zeros (or poles) given as real and imaginary parts to double precision.
    /// </summary>
    public static Complex[] Roots<T>(T[] re, T[] im)
        where T : IFloatingPointIeee754<T>
    {
        var result = new Complex[re.Length];

        for (var i = 0; i < re.Length; i++)
            result[i] = new Complex(double.CreateChecked(re[i]), double.CreateChecked(im[i]));

        return result;
    }

    // sum of c[k] * e^(-j * omega * k)
    private static Complex Evaluate(double[] coefficients, double omega)
    {
        var x = Complex.FromPolarCoordinates(1, -omega);
        var sum = Complex.Zero;

        for (var k = coefficients.Length - 1; k >= 0; k--)
            sum = sum * x + coefficients[k];

        return sum;
    }
}
