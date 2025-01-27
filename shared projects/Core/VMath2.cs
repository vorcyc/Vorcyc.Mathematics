namespace Vorcyc.Mathematics;

using System;
using System.Numerics;
using Vorcyc.Mathematics.Numerics;

public static partial class VMath
{

    /// <summary>
    /// Returns Sinc of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Sinc(double x)
    {
        return Math.Abs(x) > 1e-20 ? Math.Sin(Math.PI * x) / (Math.PI * x) : 1.0;
    }


    /// <summary>
    /// Returns Sinc of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sinc(float x)
    {
        return MathF.Abs(x) > 1e-20 ? MathF.Sin(ConstantsFp32.PI * x) / (ConstantsFp32.PI * x) : 1.0f;
    }


    /// <summary>
    /// Returns Sinc of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sinc<T>(T x)
        where T : IFloatingPointIeee754<T>
    {
        return T.Abs(x) > T.CreateTruncating(1e-20) ? T.Sin(T.Pi * x) / (T.Pi * x) : T.One;
    }

    ///// <summary>
    ///// Returns next power of 2 closest to the given number <paramref name="n"/>.
    ///// </summary>
    //public static int NextPowerOfTwo(int n)
    //{
    //    return (int)Math.Pow(2, Math.Ceiling(Math.Log(n, 2)));
    //}

    ///// <summary>
    ///// Modulo function that works correctly with negative numbers (as np.mod).
    ///// </summary>
    //public static double Mod(double a, double b)
    //{
    //    return ((a % b) + b) % b;
    //}

    /// <summary>
    /// Computes Inverse Sinh of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Asinh(double x)
    {
        return Math.Log(x + Math.Sqrt(x * x + 1));
    }


    /// <summary>
    /// Computes Inverse Sinh of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Asinh(float x)
    {
        return MathF.Log(x + MathF.Sqrt(x * x + 1));
    }

    /// <summary>
    /// Computes Inverse Sinh of <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Asinh<T>(T x)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.Log(x + T.Sqrt(x * x + T.One));
    }

    ///// <summary>
    ///// Computes factorial <paramref name="n"/>!.
    ///// </summary>
    //public static double Factorial(int n)
    //{
    //    var f = 1.0;

    //    for (var i = 2; i <= n; f *= i++) ;

    //    return f;
    //}

    /// <summary>
    /// Evaluates Binomial coefficient.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BinomialCoefficient(int k, int n)
    {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }



    /// <summary>
    /// Evaluate discrete difference of <paramref name="samples"/> (array of the 1st order derivatives).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Diff(float[] samples, float[] diff)
    {
        diff[0] = samples[0];

        for (var i = 1; i < samples.Length; i++)
        {
            diff[i] = samples[i] - samples[i - 1];
        }
    }

    /// <summary>
    /// Does linear interpolation (as numpy.interp).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InterpolateLinear(float[] x, float[] y, float[] arg, float[] interp)
    {
        var left = 0;
        var right = 1;

        for (var i = 0; i < arg.Length; i++)
        {
            while (arg[i] > x[right] && right < x.Length - 1)
            {
                right++;
                left++;
            }

            interp[i] = y[left] + (y[right] - y[left]) * (arg[i] - x[left]) / (x[right] - x[left]);
        }
    }

    /// <summary>
    /// Does bilinear transform (in-place).
    /// </summary>
    /// <param name="re">Real parts of complex values</param>
    /// <param name="im">Imaginary parts of complex values</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BilinearTransform(double[] re, double[] im)
    {
        for (var k = 0; k < re.Length; k++)
        {
            var den = (1 - re[k]) * (1 - re[k]) + im[k] * im[k];
            re[k] = (1 - re[k] * re[k] - im[k] * im[k]) / den;
            im[k] = 2 * im[k] / den;
        }

        // equivalent to:

        //for (var k = 0; k < re.Length; k++)
        //{
        //      var c1 = new Complex(1 + re[k],  im[k]);
        //      var c2 = new Complex(1 - re[k], -im[k]);
        //      var c = c1 / c2;

        //      re[k] = c.Real;
        //      im[k] = c.Imaginary;
        //}
    }


    /// <summary>
    /// Does bilinear transform (in-place).
    /// </summary>
    /// <param name="re">Real parts of complex values</param>
    /// <param name="im">Imaginary parts of complex values</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BilinearTransform(float[] re, float[] im)
    {
        for (var k = 0; k < re.Length; k++)
        {
            var den = (1 - re[k]) * (1 - re[k]) + im[k] * im[k];
            re[k] = (1 - re[k] * re[k] - im[k] * im[k]) / den;
            im[k] = 2 * im[k] / den;
        }

        // equivalent to:

        //for (var k = 0; k < re.Length; k++)
        //{
        //      var c1 = new Complex(1 + re[k],  im[k]);
        //      var c2 = new Complex(1 - re[k], -im[k]);
        //      var c = c1 / c2;

        //      re[k] = c.Real;
        //      im[k] = c.Imaginary;
        //}
    }

    /// <summary>
    /// Unwraps phase.
    /// </summary>
    /// <param name="phase">Phase array</param>
    /// <param name="tolerance">Jump size</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] Unwrap(double[] phase, double tolerance = Math.PI)
    {
        var unwrapped = phase.FastCopy();

        var offset = 0.0;

        for (var n = 1; n < phase.Length; n++)
        {
            var delta = phase[n] - phase[n - 1];

            if (delta > tolerance)
            {
                offset -= tolerance * 2;
            }
            else if (delta < -tolerance)
            {
                offset += tolerance * 2;
            }

            unwrapped[n] = phase[n] + offset;
        }

        return unwrapped;
    }

    /// <summary>
    /// Unwraps phase.
    /// </summary>
    /// <param name="phase">Phase array</param>
    /// <param name="tolerance">Jump size</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] Unwrap(float[] phase, float tolerance = ConstantsFp32.PI)
    {
        var unwrapped = phase.FastCopy();

        var offset = 0.0f;

        for (var n = 1; n < phase.Length; n++)
        {
            var delta = phase[n] - phase[n - 1];

            if (delta > tolerance)
            {
                offset -= tolerance * 2;
            }
            else if (delta < -tolerance)
            {
                offset += tolerance * 2;
            }

            unwrapped[n] = phase[n] + offset;
        }

        return unwrapped;
    }

    /// <summary>
    /// Unwraps phase.
    /// </summary>
    /// <param name="phase">Phase array</param>
    /// <param name="tolerance">Jump size, the default value is PI</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Unwrap<T>(T[] phase, T? tolerance = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {

        tolerance = T.Pi;

        var unwrapped = phase.Copy();

        T offset = T.Zero;

        for (var n = 1; n < phase.Length; n++)
        {
            var delta = phase[n] - phase[n - 1];

            if (delta > tolerance)
            {
                offset -= tolerance.Value * T.CreateTruncating(2);
            }
            else if (delta < -tolerance)
            {
                offset += tolerance.Value * T.CreateTruncating(2);
            }

            unwrapped[n] = phase[n] + offset;
        }

        return unwrapped;
    }

    /// <summary>
    /// Wraps phase.
    /// </summary>
    /// <param name="phase">Phase array</param>
    /// <param name="tolerance">Jump size</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] Wrap(double[] phase, double tolerance = Math.PI)
    {
        var wrapped = phase.FastCopy();

        for (var n = 0; n < phase.Length; n++)
        {
            var offset = phase[n] % (tolerance * 2);

            if (offset > tolerance)
            {
                offset -= tolerance * 2;
            }
            else if (offset < -tolerance)
            {
                offset += tolerance * 2;
            }

            wrapped[n] = offset;
        }

        return wrapped;
    }


    /// <summary>
    /// Wraps phase.
    /// </summary>
    /// <param name="phase">Phase array</param>
    /// <param name="tolerance">Jump size</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] Wrap(float[] phase, float tolerance = ConstantsFp32.PI)
    {
        var wrapped = phase.FastCopy();

        for (var n = 0; n < phase.Length; n++)
        {
            var offset = phase[n] % (tolerance * 2);

            if (offset > tolerance)
            {
                offset -= tolerance * 2;
            }
            else if (offset < -tolerance)
            {
                offset += tolerance * 2;
            }

            wrapped[n] = offset;
        }

        return wrapped;
    }


    /// <summary>
    /// Finds the n-th order statistic (n-th smallest value) in the array <paramref name="a"/>.
    /// </summary>
    /// <param name="a">The array to search.</param>
    /// <param name="n">The order of the statistic to find (0-based index).</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <param name="end">The ending index of the range to search.</param>
    /// <returns>The n-th smallest value in the array <paramref name="a"/>.</returns>
    /// <remarks>
    /// This method uses a partitioning approach similar to the quickselect algorithm to find the n-th smallest value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FindNth(float[] a, int n, int start, int end)
    {
        while (true)
        {
            // ============== Partitioning =============
            var pivotElem = a[end];
            var pivot = start - 1;
            for (var i = start; i < end; i++)
            {
                if (a[i] <= pivotElem)
                {
                    pivot++;
                    var temp = a[i];
                    a[i] = a[pivot];
                    a[pivot] = temp;
                }
            }
            pivot++;
            var tmp = a[end];
            a[end] = a[pivot];
            a[pivot] = tmp;
            // ========================================

            if (pivot == n)
            {
                return a[pivot];
            }
            if (n < pivot)
            {
                end = pivot - 1;
            }
            else
            {
                start = pivot + 1;
            }
        }
    }

    /// <summary>
    /// Finds the n-th order statistic (n-th smallest value) in the array <paramref name="a"/>.
    /// </summary>
    /// <param name="a">The array to search.</param>
    /// <param name="n">The order of the statistic to find (0-based index).</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <param name="end">The ending index of the range to search.</param>
    /// <returns>The n-th smallest value in the array <paramref name="a"/>.</returns>
    /// <remarks>
    /// This method uses a partitioning approach similar to the quickselect algorithm to find the n-th smallest value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T FindNth<T>(Span<T> values, int n)
        where T : INumber<T>
    {
        int start = 0;
        int end = values.Length - 1;

        while (true)
        {
            // ============== Partitioning =============
            var pivotElem = values[end];
            var pivot = start - 1;
            for (var i = start; i < end; i++)
            {
                if (values[i] <= pivotElem)
                {
                    pivot++;
                    var temp = values[i];
                    values[i] = values[pivot];
                    values[pivot] = temp;
                }
            }
            pivot++;
            var tmp = values[end];
            values[end] = values[pivot];
            values[pivot] = tmp;
            // ========================================

            if (pivot == n)
            {
                return values[pivot];
            }
            if (n < pivot)
            {
                end = pivot - 1;
            }
            else
            {
                start = pivot + 1;
            }
        }
    }


    /// <summary>
    /// Modified Bessel function I0(<paramref name="x"/>) of the 1st kind 
    /// (using Taylor series, not very precise method).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double I0(double x)
    {
        var y = 1.0;
        var prev = 1.0;

        var i = 1;

        while (Math.Abs(prev) > 1e-20)
        {
            var summand = prev * x * x / (4 * i * i);
            y += summand;
            prev = summand;
            i++;
        }

        return y;
    }


    /// <summary>
    /// Modified Bessel function I0(<paramref name="x"/>) of the 1st kind 
    /// (using Taylor series, not very precise method).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float I0(float x)
    {
        var y = 1.0f;
        var prev = 1.0f;

        var i = 1;

        while (MathF.Abs(prev) > 1e-20f)
        {
            var summand = prev * x * x / (4 * i * i);
            y += summand;
            prev = summand;
            i++;
        }

        return y;
    }


    /// <summary>
    /// Modified Bessel function I0(<paramref name="x"/>) of the 1st kind 
    /// (using Taylor series, not very precise method).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T I0<T>(T x)
        where T : IFloatingPointIeee754<T>
    {
        var y = T.One;// 1.0f;
        var prev = T.One;// 1.0f;

        var i = 1;

        while (T.Abs(prev) > T.CreateTruncating(1e-20f))
        {
            var summand = prev * x * x / T.CreateChecked(4 * i * i);
            y += summand;
            prev = summand;
            i++;
        }

        return y;
    }


    /// <summary>
    /// Modified Bessel function I0(<paramref name="x"/>) of the 1st kind 
    /// (using Taylor series, not very precise method).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex<T> I0<T>(Complex<T> x)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var y = Complex<T>.One;// 1.0f;
        var prev = Complex<T>.One;// 1.0f;

        var i = 1;

        while (Complex<T>.Abs(prev) > T.CreateTruncating(1e-20f))
        {
            var summand = prev * x * x / T.CreateChecked(4 * i * i);
            y += summand;
            prev = summand;
            i++;
        }

        return y;
    }


    #region polynomials

    /// <summary>
    /// Number of iterations in Durand-Kerner algorithm for evaluating polynomial roots.
    /// </summary>
    public const int PolyRootsIterations = 25000;

    /// <summary>
    /// Evaluates complex roots of polynomials using Durand-Kerner algorithm. 
    /// Works for polynomials of order up to approx. 50.
    /// </summary>
    /// <param name="a">Polynomial coefficients</param>
    /// <param name="maxIterations">Max number of iterations</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex[]? PolynomialRoots(double[] a, int maxIterations = PolyRootsIterations)
    {
        var n = a.Length;
        if (n <= 1)
        {
            return null;
        }

        var c1 = Complex.One;

        var rootsPrev = new Complex[a.Length - 1];
        var roots = new Complex[a.Length - 1];

        var result = new Complex(0.4, 0.9);
        rootsPrev[0] = c1;

        for (var i = 1; i < rootsPrev.Length; i++)
        {
            rootsPrev[i] = rootsPrev[i - 1] * result;
        }

        var iter = 0;
        while (true)
        {
            for (int i = 0; i < rootsPrev.Length; i++)
            {
                result = c1;

                for (int j = 0; j < rootsPrev.Length; j++)
                {
                    if (i != j)
                    {
                        result = (rootsPrev[i] - rootsPrev[j]) * result;
                    }
                }

                roots[i] = rootsPrev[i] - (EvaluatePolynomial(a, rootsPrev[i]) / result);
            }

            if (++iter > maxIterations || ArraysAreEqual(rootsPrev, roots))
            {
                break;
            }

            Array.Copy(roots, rootsPrev, roots.Length);
        }

        return roots;
    }

    /// <summary>
    /// Evaluates complex roots of polynomials using Durand-Kerner algorithm. 
    /// Works for polynomials of order up to approx. 50.
    /// </summary>
    /// <param name="a">Polynomial coefficients</param>
    /// <param name="maxIterations">Max number of iterations</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexFp32[]? PolynomialRoots(float[] a, int maxIterations = PolyRootsIterations)
    {
        var n = a.Length;
        if (n <= 1)
        {
            return null;
        }

        var c1 = ComplexFp32.One;

        var rootsPrev = new ComplexFp32[a.Length - 1];
        var roots = new ComplexFp32[a.Length - 1];

        var result = new ComplexFp32(0.4f, 0.9f);
        rootsPrev[0] = c1;

        for (var i = 1; i < rootsPrev.Length; i++)
        {
            rootsPrev[i] = rootsPrev[i - 1] * result;
        }

        var iter = 0;
        while (true)
        {
            for (int i = 0; i < rootsPrev.Length; i++)
            {
                result = c1;

                for (int j = 0; j < rootsPrev.Length; j++)
                {
                    if (i != j)
                    {
                        result = (rootsPrev[i] - rootsPrev[j]) * result;
                    }
                }

                roots[i] = rootsPrev[i] - (EvaluatePolynomial(a, rootsPrev[i]) / result);
            }

            if (++iter > maxIterations || ArraysAreEqual(rootsPrev, roots))
            {
                break;
            }

            Array.Copy(roots, rootsPrev, roots.Length);
        }

        return roots;
    }

    /// <summary>
    /// Checks if two arrays of complex numbers are essentially identical.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ArraysAreEqual(Complex[] a, Complex[] b, double tolerance = 1e-16)
    {
        for (var i = 0; i < a.Length; i++)
        {
            if (Complex.Abs(a[i] - b[i]) > tolerance)
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Checks if two arrays of complex numbers are essentially identical.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ArraysAreEqual(ComplexFp32[] a, ComplexFp32[] b, float tolerance = 1e-16f)
    {
        for (var i = 0; i < a.Length; i++)
        {
            if (ComplexFp32.Abs(a[i] - b[i]) > tolerance)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Evaluates polynomial according to Horner scheme.
    /// </summary>
    /// <param name="a">Polynomial coefficients</param>
    /// <param name="x">Argument</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex EvaluatePolynomial(double[] a, Complex x)
    {
        var res = new Complex(a[0], 0);

        for (var i = 1; i < a.Length; i++)
        {
            res *= x;
            res += a[i];
        }

        return res;
    }


    /// <summary>
    /// Evaluates polynomial according to Horner scheme.
    /// </summary>
    /// <param name="a">Polynomial coefficients</param>
    /// <param name="x">Argument</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexFp32 EvaluatePolynomial(float[] a, ComplexFp32 x)
    {
        var res = new ComplexFp32(a[0], 0);

        for (var i = 1; i < a.Length; i++)
        {
            res *= x;
            res += a[i];
        }

        return res;
    }


    /// <summary>
    /// Multiplies polynomials.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex[] MultiplyPolynomials(Complex[] poly1, Complex[] poly2)
    {
        var length = poly1.Length + poly2.Length - 1;
        var result = new Complex[length];

        for (var i = 0; i < poly1.Length; i++)
        {
            for (var j = 0; j < poly2.Length; j++)
            {
                result[i + j] += poly1[i] * poly2[j];
            }
        }

        return result;
    }


    /// <summary>
    /// Multiplies polynomials.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexFp32[] MultiplyPolynomials(ComplexFp32[] poly1, ComplexFp32[] poly2)
    {
        var length = poly1.Length + poly2.Length - 1;
        var result = new ComplexFp32[length];

        for (var i = 0; i < poly1.Length; i++)
        {
            for (var j = 0; j < poly2.Length; j++)
            {
                result[i + j] += poly1[i] * poly2[j];
            }
        }

        return result;
    }

    /// <summary>
    /// Divides polynomials.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex[][] DividePolynomial(Complex[] dividend, Complex[] divisor)
    {
        var output = (Complex[])dividend.Clone();
        var normalizer = divisor[0];

        for (var i = 0; i < dividend.Length - divisor.Length + 1; i++)
        {
            output[i] /= normalizer;

            var coeff = output[i];
            if (Math.Abs(coeff.Real) > 1e-10 || Math.Abs(coeff.Imaginary) > 1e-10)
            {
                for (var j = 1; j < divisor.Length; j++)
                {
                    output[i + j] -= divisor[j] * coeff;
                }
            }
        }

        var separator = output.Length - divisor.Length + 1;

        var q = new Complex[separator];
        var r = new Complex[output.Length - separator];

        Array.Copy(output, 0, q, 0, separator);
        Array.Copy(output, separator, r, 0, output.Length - separator);

        return [q, r];
    }


    /// <summary>
    /// Divides polynomials.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexFp32[][] DividePolynomial(ComplexFp32[] dividend, ComplexFp32[] divisor)
    {
        var output = (ComplexFp32[])dividend.Clone();
        var normalizer = divisor[0];

        for (var i = 0; i < dividend.Length - divisor.Length + 1; i++)
        {
            output[i] /= normalizer;

            var coeff = output[i];
            if (MathF.Abs(coeff.Real) > 1e-10f || MathF.Abs(coeff.Imaginary) > 1e-10f)
            {
                for (var j = 1; j < divisor.Length; j++)
                {
                    output[i + j] -= divisor[j] * coeff;
                }
            }
        }

        var separator = output.Length - divisor.Length + 1;

        var q = new ComplexFp32[separator];
        var r = new ComplexFp32[output.Length - separator];

        Array.Copy(output, 0, q, 0, separator);
        Array.Copy(output, separator, r, 0, output.Length - separator);

        return new[] { q, r };
    }


    #endregion















}
