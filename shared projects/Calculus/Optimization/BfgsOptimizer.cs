using System.Numerics;
using Vorcyc.Mathematics.Calculus;

namespace Vorcyc.Mathematics.Calculus.Optimization;

/// <summary>
/// BFGS 拟牛顿法求无约束极小 min f(x)。
/// </summary>
public sealed class BfgsOptimizer<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly MultiVariableFunction<T> _func;
    private readonly Derivative<T> _derivative;

    private T[]? _x;
    private T[]? _g;
    private T[]? _hFlat;
    private T[]? _s;
    private T[]? _y;
    private T[]? _xNew;
    private T[]? _gNew;
    private T[]? _hd;
    private T[]? _hy;
    private T[]? _trial;
    private int _n;

    public BfgsOptimizer(MultiVariableFunction<T> func, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _derivative = new Derivative<T>(func, defaultH);
    }

    /// <summary>求解无约束极小点。</summary>
    public T[] Minimize(T[] initial, int maxIterations = 200, T? tolerance = null)
    {
        if (initial.Length == 0) throw new ArgumentException("初始点不能为空", nameof(initial));
        EnsureCapacity(initial.Length);

        T tol = tolerance ?? T.CreateChecked(1e-8);
        T tol2 = tol * tol;
        initial.AsSpan().CopyTo(_x!);
        _derivative.Gradient(_x!, _g!);

        ResetHessianInverse(_n);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            if (CalculusVectorOps.Dot(_g!, _g!) < tol2)
                return (T[])_x!.Clone();

            CalculusVectorOps.MatVec(_hFlat!, _n, _g!, _hd!);
            for (int i = 0; i < _n; i++)
                _s![i] = -_hd![i];

            T alpha = LineSearch.ArmijoBacktracking(_x!, _s!, _func(_x!), _g!, _func, _trial!);
            if (alpha == T.Zero)
                return (T[])_x!.Clone();

            for (int i = 0; i < _n; i++)
                _xNew![i] = _x![i] + alpha * _s![i];

            _derivative.Gradient(_xNew!, _gNew!);
            for (int i = 0; i < _n; i++)
            {
                _y![i] = _gNew![i] - _g![i];
                _s![i] = _xNew![i] - _x![i];
            }

            T sy = CalculusVectorOps.Dot(_s!, _y!);
            if (sy > T.CreateChecked(1e-12))
            {
                CalculusVectorOps.MatVec(_hFlat!, _n, _y!, _hy!);
                T yTy = CalculusVectorOps.Dot(_y!, _hy!);
                T invSy = T.One / sy;
                T factor = (T.One + yTy * invSy) * invSy;
                CalculusVectorOps.SymmetricBfgsUpdate(_hFlat!, _n, _s!, _hy!, invSy, factor);
            }

            (_x, _xNew) = (_xNew, _x);
            (_g, _gNew) = (_gNew, _g);
        }

        return (T[])_x!.Clone();
    }

    private void EnsureCapacity(int n)
    {
        if (n <= _n && _x is not null)
            return;

        _n = n;
        _x = new T[n];
        _g = new T[n];
        _hFlat = new T[n * n];
        _s = new T[n];
        _y = new T[n];
        _xNew = new T[n];
        _gNew = new T[n];
        _hd = new T[n];
        _hy = new T[n];
        _trial = new T[n];
    }

    private static void ResetHessianInverse(int n, T[] hFlat)
    {
        Array.Clear(hFlat);
        for (int i = 0; i < n; i++)
            hFlat[i * n + i] = T.One;
    }

    private void ResetHessianInverse(int n) => ResetHessianInverse(n, _hFlat!);
}
