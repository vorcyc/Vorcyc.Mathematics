using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Natural cubic spline through strictly increasing knots. Used by EMD / MEMD envelopes.
/// </summary>
/// <typeparam name="T">IEEE-754 knot / value type.</typeparam>
internal readonly struct NaturalCubicSpline<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly T[] _x;
    private readonly T[] _a;
    private readonly T[] _b;
    private readonly T[] _c;
    private readonly T[] _d;
    private readonly int _seg;

    private NaturalCubicSpline(T[] x, T[] a, T[] b, T[] c, T[] d, int seg)
    {
        _x = x;
        _a = a;
        _b = b;
        _c = c;
        _d = d;
        _seg = seg;
    }

    public static bool TryCreate(T[] x, T[] y, out NaturalCubicSpline<T> spline)
    {
        spline = default;
        int n = x.Length - 1;
        if (n < 1 || x.Length != y.Length) return false;

        var h = new T[n];
        for (int i = 0; i < n; i++)
        {
            h[i] = x[i + 1] - x[i];
            if (!(h[i] > T.Zero)) return false;
        }

        T three = T.CreateChecked(3);
        T two = T.CreateChecked(2);
        var alpha = new T[n];
        for (int i = 1; i < n; i++)
            alpha[i] = three * ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1]);

        var l = new T[n + 1];
        var mu = new T[n];
        var z = new T[n + 1];
        var c = new T[n + 1];
        var b = new T[n];
        var d = new T[n];

        l[0] = T.One;
        T tiny = T.CreateChecked(1e-30);
        for (int i = 1; i < n; i++)
        {
            l[i] = two * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
            if (T.Abs(l[i]) < tiny) return false;
            mu[i] = h[i] / l[i];
            z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
        }

        l[n] = T.One;
        for (int j = n - 1; j >= 0; j--)
        {
            c[j] = z[j] - mu[j] * c[j + 1];
            b[j] = (y[j + 1] - y[j]) / h[j] - h[j] * (c[j + 1] + two * c[j]) / three;
            d[j] = (c[j + 1] - c[j]) / (three * h[j]);
        }

        spline = new NaturalCubicSpline<T>(x, y, b, c, d, n);
        return true;
    }

    public T Evaluate(T xi)
    {
        if (xi <= _x[0]) return _a[0];
        if (xi >= _x[_seg]) return _a[_seg];

        int i = 0;
        while (i < _seg && xi > _x[i + 1]) i++;
        if (i == _seg) i--;
        T dx = xi - _x[i];
        return _a[i] + _b[i] * dx + _c[i] * dx * dx + _d[i] * dx * dx * dx;
    }

    public T Evaluate(int sampleIndex) => Evaluate(T.CreateChecked(sampleIndex));
}
