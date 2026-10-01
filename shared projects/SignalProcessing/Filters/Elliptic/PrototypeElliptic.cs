using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;

// Orfanidis, S. J. (2007). Lecture notes on elliptic filter design.
// URL: http://www.ece.rutgers.edu/~orfanidi/ece521/notes.pdf

/// <summary>
/// Elliptic filter prototype.
/// </summary>
public static class PrototypeElliptic
{
    /// <summary>
    /// Evaluates analog poles of elliptic filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="ripplePass">Passband ripple (in dB)</param>
    /// <param name="rippleStop">Stopband ripple (in dB)</param>
    public static ComplexFp32[] Poles(int order, float ripplePass = 1f, float rippleStop = 20f)
    {
        Guard.AgainstInvalidRange(ripplePass, rippleStop, "ripple in passband", "ripple in stopband");

        var eps_p = MathF.Sqrt(MathF.Pow(10, ripplePass / 10) - 1);
        var eps_s = MathF.Sqrt(MathF.Pow(10, rippleStop / 10) - 1);

        var r = eps_p / eps_s;

        var k1 = MathF.Sqrt(1 - r * r);
        var k1_landen = Landen(k1);

        var kp = ComplexFp32.One;
        for (var i = 0; i < order / 2; i++)
        {
            kp *= Sne((2 * i + 1.0) / order, k1_landen);
        }
        kp = ComplexFp32.Pow(k1 * k1, order / 2) * ComplexFp32.Pow(kp, 4);

        var k = MathF.Sqrt(1 - ComplexFp32.Abs(kp) * ComplexFp32.Abs(kp));
        var k_landen = Landen(k);

        var v0 = -ComplexFp32.ImaginaryOne / order * Asne(ComplexFp32.ImaginaryOne / eps_p, r);

        var poles = new ComplexFp32[order];

        for (var i = 0; i < order; i++)
        {
            var w = (2 * i + 1.0f) / order;

            poles[i] = ComplexFp32.ImaginaryOne * Cde(w - ComplexFp32.ImaginaryOne * v0, k_landen);
        }

        return poles;
    }

    /// <summary>
    /// Evaluates analog zeros of elliptic filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="ripplePass">Passband ripple (in dB)</param>
    /// <param name="rippleStop">Stopband ripple (in dB)</param>
    public static Complex[] Zeros(int order, double ripplePass = 1, double rippleStop = 20)
    {
        Guard.AgainstInvalidRange(ripplePass, rippleStop, "ripple in passband", "ripple in stopband");

        var eps_p = Math.Sqrt(Math.Pow(10, ripplePass / 10) - 1);
        var eps_s = Math.Sqrt(Math.Pow(10, rippleStop / 10) - 1);

        var r = eps_p / eps_s;

        var k1 = Math.Sqrt(1 - r * r);
        var k1_landen = Landen(k1);

        var kp = Complex.One;
        for (var i = 0; i < order / 2; i++)
        {
            kp *= Sne((2 * i + 1.0) / order, k1_landen);
        }
        kp = Complex.Pow(k1 * k1, order / 2) * Complex.Pow(kp, 4);

        var k = Math.Sqrt(1 - Complex.Abs(kp) * Complex.Abs(kp));
        var k_landen = Landen(k);

        var zeros = new Complex[order];

        for (var i = 0; i < order; i++)
        {
            var w = (2 * i + 1.0) / order;

            var d = (k * Cde(w, k_landen)).Real;
            // Order 1 degenerates to k == 0 (no finite zero): a huge finite zero keeps the bilinear transform finite (it maps to z = -1).
            zeros[i] = new Complex(0, d == 0 ? -1e16 : -1 / d);
        }

        return zeros;
    } 
    
    
    /// <summary>
    /// Evaluates analog zeros of elliptic filter of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="ripplePass">Passband ripple (in dB)</param>
    /// <param name="rippleStop">Stopband ripple (in dB)</param>
    public static ComplexFp32[] Zeros(int order, float ripplePass = 1, float rippleStop = 20)
    {
        Guard.AgainstInvalidRange(ripplePass, rippleStop, "ripple in passband", "ripple in stopband");

        var eps_p = MathF.Sqrt(MathF.Pow(10, ripplePass / 10) - 1);
        var eps_s = MathF.Sqrt(MathF.Pow(10, rippleStop / 10) - 1);

        var r = eps_p / eps_s;

        var k1 = MathF.Sqrt(1 - r * r);
        var k1_landen = Landen(k1);

        var kp = ComplexFp32.One;
        for (var i = 0; i < order / 2; i++)
        {
            kp *= Sne((2 * i + 1.0) / order, k1_landen);
        }
        kp = ComplexFp32.Pow(k1 * k1, order / 2) * ComplexFp32.Pow(kp, 4);

        var k = MathF.Sqrt(1 - ComplexFp32.Abs(kp) * ComplexFp32.Abs(kp));
        var k_landen = Landen(k);

        var zeros = new ComplexFp32[order];

        for (var i = 0; i < order; i++)
        {
            var w = (2 * i + 1.0) / order;

            var d = (k * Cde(w, k_landen)).Real;
            // Order 1 degenerates to k == 0 (no finite zero): a huge finite zero keeps the bilinear transform finite (it maps to z = -1).
            zeros[i] = new ComplexFp32(0, d == 0 ? -1e8f : -1 / d);
        }

        return zeros;
    }

    /// <summary>
    /// Computes Landen sequence.
    /// </summary>
    /// <param name="k">K</param>
    /// <param name="iterCount">Number of iterations</param>
    public static double[] Landen(double k, int iterCount = 5)
    {
        var coeffs = new double[iterCount];

        for (var i = 0; i < iterCount; i++)
        {
            var kp = Math.Sqrt(1 - k * k);
            k = (1 - kp) / (1 + kp);
            coeffs[i] = k;
        }

        return coeffs;
    }    
    
    /// <summary>
    /// Computes Landen sequence.
    /// </summary>
    /// <param name="k">K</param>
    /// <param name="iterCount">Number of iterations</param>
    public static float[] Landen(float k, int iterCount = 5)
    {
        var coeffs = new float[iterCount];

        for (var i = 0; i < iterCount; i++)
        {
            var kp = MathF.Sqrt(1 - k * k);
            k = (1 - kp) / (1 + kp);
            coeffs[i] = k;
        }

        return coeffs;
    }

    /// <summary>
    /// Computes sde.
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="landen">Landen sequence</param>
    public static Complex Cde(Complex x, double[] landen)
    {
        var invX = 1 / Complex.Cos(x * Math.PI / 2);

        for (var i = landen.Length - 1; i >= 0; i--)
        {
            invX = 1 / (1 + landen[i]) * (invX + landen[i] / invX);
        }

        return 1 / invX;
    }
      
    
    /// <summary>
    /// Computes sde.
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="landen">Landen sequence</param>
    public static ComplexFp32 Cde(ComplexFp32 x, float[] landen)
    {
        var invX = 1 / ComplexFp32.Cos(x * ConstantsFp32.PI / 2);

        for (var i = landen.Length - 1; i >= 0; i--)
        {
            invX = 1 / (1 + landen[i]) * (invX + landen[i] / invX);
        }

        return 1 / invX;
    }

    /// <summary>
    /// Computes sne.
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="landen">Landen sequence</param>
    public static Complex Sne(Complex x, double[] landen)
    {
        var invX = 1 / Complex.Sin(x * Math.PI / 2);

        for (var i = landen.Length - 1; i >= 0; i--)
        {
            invX = 1 / (1 + landen[i]) * (invX + landen[i] / invX);
        }

        return 1 / invX;
    }
      
    
    /// <summary>
    /// Computes sne.
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="landen">Landen sequence</param>
    public static ComplexFp32 Sne(ComplexFp32 x, float[] landen)
    {
        var invX = 1 / ComplexFp32.Sin(x * ConstantsFp32.PI / 2);

        for (var i = landen.Length - 1; i >= 0; i--)
        {
            invX = 1 / (1 + landen[i]) * (invX + landen[i] / invX);
        }

        return 1 / invX;
    }

    /// <summary>
    /// Computes inverse sne.
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="k">K</param>
    /// <param name="iterCount">Number of iterations</param>
    public static Complex Asne(Complex x, double k, int iterCount = 5)
    {
        for (var i = 1; i <= iterCount; i++)
        {
            var prevX = x;
            var prevK = k;

            k = Math.Pow(k / (1 + Math.Sqrt(1 - k * k)), 2);

            x = 2 * x / ((1 + k) * (1 + Complex.Sqrt(1 - prevK * prevK * x * x)));
        }

        return 2 * Complex.Asin(x) / Math.PI;
    } 
    
    
    /// <summary>
    /// Computes inverse sne.
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="k">K</param>
    /// <param name="iterCount">Number of iterations</param>
    public static ComplexFp32 Asne(ComplexFp32 x, float k, int iterCount = 5)
    {
        for (var i = 1; i <= iterCount; i++)
        {
            var prevX = x;
            var prevK = k;

            k = MathF.Pow(k / (1 + MathF.Sqrt(1 - k * k)), 2);

            x = 2 * x / ((1 + k) * (1 + ComplexFp32.Sqrt(1 - prevK * prevK * x * x)));
        }

        return 2 * ComplexFp32.Asin(x) / ConstantsFp32.PI;
    }

    /// <summary>
    /// Analog elliptic poles in <typeparamref name="T"/>.
    /// </summary>
    public static Complex<T>[] Poles<T>(int order, T? ripplePass = null, T? rippleStop = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var rp = ripplePass ?? T.One;
        var rs = rippleStop ?? T.CreateChecked(20);
        Guard.AgainstInvalidRange(double.CreateChecked(rp), double.CreateChecked(rs), "ripple in passband", "ripple in stopband");

        var ten = T.CreateChecked(10);
        var epsP = T.Sqrt(T.Pow(ten, rp / ten) - T.One);
        var epsS = T.Sqrt(T.Pow(ten, rs / ten) - T.One);
        var r = epsP / epsS;
        var k1 = T.Sqrt(T.One - r * r);
        var k1Landen = LandenT(k1);

        var kp = Complex<T>.One;
        for (var i = 0; i < order / 2; i++)
            kp *= SneT(T.CreateChecked(2 * i + 1) / T.CreateChecked(order), k1Landen);
        kp = Complex<T>.Pow(new Complex<T>(k1 * k1, T.Zero), T.CreateChecked(order / 2)) * Complex<T>.Pow(kp, T.CreateChecked(4));

        var k = T.Sqrt(T.One - Complex<T>.Abs(kp) * Complex<T>.Abs(kp));
        var kLanden = LandenT(k);
        var v0 = -Complex<T>.ImaginaryOne / T.CreateChecked(order) * AsneT(Complex<T>.ImaginaryOne / epsP, r);

        var poles = new Complex<T>[order];
        for (var i = 0; i < order; i++)
        {
            var w = T.CreateChecked(2 * i + 1) / T.CreateChecked(order);
            poles[i] = Complex<T>.ImaginaryOne * CdeT(new Complex<T>(w, T.Zero) - Complex<T>.ImaginaryOne * v0, kLanden);
        }
        return poles;
    }

    /// <summary>
    /// Analog elliptic zeros in <typeparamref name="T"/>.
    /// </summary>
    public static Complex<T>[] Zeros<T>(int order, T? ripplePass = null, T? rippleStop = null)
        where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var rp = ripplePass ?? T.One;
        var rs = rippleStop ?? T.CreateChecked(20);
        Guard.AgainstInvalidRange(double.CreateChecked(rp), double.CreateChecked(rs), "ripple in passband", "ripple in stopband");

        var ten = T.CreateChecked(10);
        var epsP = T.Sqrt(T.Pow(ten, rp / ten) - T.One);
        var epsS = T.Sqrt(T.Pow(ten, rs / ten) - T.One);
        var r = epsP / epsS;
        var k1 = T.Sqrt(T.One - r * r);
        var k1Landen = LandenT(k1);

        var kp = Complex<T>.One;
        for (var i = 0; i < order / 2; i++)
            kp *= SneT(T.CreateChecked(2 * i + 1) / T.CreateChecked(order), k1Landen);
        kp = Complex<T>.Pow(new Complex<T>(k1 * k1, T.Zero), T.CreateChecked(order / 2)) * Complex<T>.Pow(kp, T.CreateChecked(4));

        var k = T.Sqrt(T.One - Complex<T>.Abs(kp) * Complex<T>.Abs(kp));
        var kLanden = LandenT(k);

        var zeros = new Complex<T>[order];
        for (var i = 0; i < order; i++)
        {
            var w = T.CreateChecked(2 * i + 1) / T.CreateChecked(order);
            var d = (k * CdeT(new Complex<T>(w, T.Zero), kLanden)).Real;
            // Order 1 degenerates to k == 0 (no finite zero): a huge finite zero keeps the bilinear transform finite (it maps to z = -1).
            zeros[i] = new Complex<T>(T.Zero, d == T.Zero ? -T.CreateTruncating(1e16) : -T.One / d);
        }
        return zeros;
    }

    /// <summary>
    /// Landen sequence in <typeparamref name="T"/>.
    /// </summary>
    public static T[] LandenT<T>(T k, int iterCount = 5)
        where T : IFloatingPointIeee754<T>
    {
        var coeffs = new T[iterCount];
        for (var i = 0; i < iterCount; i++)
        {
            var kp = T.Sqrt(T.One - k * k);
            k = (T.One - kp) / (T.One + kp);
            coeffs[i] = k;
        }
        return coeffs;
    }

    /// <summary>
    /// Jacobi elliptic cd via Landen sequence.
    /// </summary>
    public static Complex<T> CdeT<T>(Complex<T> x, T[] landen)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var invX = T.One / Complex<T>.Cos(x * T.Pi / T.CreateChecked(2));
        for (var i = landen.Length - 1; i >= 0; i--)
            invX = T.One / (T.One + landen[i]) * (invX + landen[i] / invX);
        return T.One / invX;
    }

    /// <summary>
    /// Jacobi elliptic sn via Landen sequence.
    /// </summary>
    public static Complex<T> SneT<T>(Complex<T> x, T[] landen)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var invX = T.One / Complex<T>.Sin(x * T.Pi / T.CreateChecked(2));
        for (var i = landen.Length - 1; i >= 0; i--)
            invX = T.One / (T.One + landen[i]) * (invX + landen[i] / invX);
        return T.One / invX;
    }

    /// <summary>
    /// Inverse Jacobi elliptic sn.
    /// </summary>
    public static Complex<T> AsneT<T>(Complex<T> x, T k, int iterCount = 5)
        where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        for (var i = 1; i <= iterCount; i++)
        {
            var prevX = x;
            var prevK = k;
            k = T.Pow(k / (T.One + T.Sqrt(T.One - k * k)), T.CreateChecked(2));
            x = T.CreateChecked(2) * x / ((T.One + k) * (T.One + Complex<T>.Sqrt(T.One - prevK * prevK * prevX * prevX)));
        }
        return T.CreateChecked(2) * Complex<T>.Asin(x) / T.Pi;
    }
}
