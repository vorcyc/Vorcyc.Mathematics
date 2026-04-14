using System.Numerics;
using Vorcyc.Mathematics.Calculus;

namespace Vorcyc.Mathematics.Calculus.Optimization;

/// <summary>
/// L-BFGS 拟牛顿法（有限内存），适用于较高维无约束极小化。
/// </summary>
public sealed class LBfgsOptimizer<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly MultiVariableFunction<T> _func;
    private readonly Derivative<T> _derivative;
    private readonly int _historySize;

    private T[]? _x;
    private T[]? _g;
    private T[]? _xNew;
    private T[]? _gNew;
    private T[]? _s;
    private T[]? _y;
    private T[]? _q;
    private T[]? _trial;
    private T[][]? _sHistory;
    private T[][]? _yHistory;
    private T[]? _rho;
    private T[]? _alpha;
    private int _n;
    private int _historyCount;
    private int _historyStart;

    public LBfgsOptimizer(MultiVariableFunction<T> func, T defaultH, int historySize = 10)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        if (historySize < 1) throw new ArgumentException("历史长度必须大于等于 1", nameof(historySize));
        _derivative = new Derivative<T>(func, defaultH);
        _historySize = historySize;
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
        _historyCount = 0;
        _historyStart = 0;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            if (CalculusVectorOps.Dot(_g!, _g!) < tol2)
                return (T[])_x!.Clone();

            ComputeSearchDirection();
            for (int i = 0; i < _n; i++)
                _s![i] = -_q![i];

            T alpha = LineSearch.ArmijoBacktracking(_x!, _s!, _func(_x!), _g!, _func, _trial!);
            if (alpha == T.Zero)
                return (T[])_x!.Clone();

            for (int i = 0; i < _n; i++)
                _xNew![i] = _x![i] + alpha * _s![i];

            _derivative.Gradient(_xNew!, _gNew!);
            for (int i = 0; i < _n; i++)
            {
                _s![i] = _xNew![i] - _x![i];
                _y![i] = _gNew![i] - _g![i];
            }

            T sy = CalculusVectorOps.Dot(_s!, _y!);
            if (sy > T.CreateChecked(1e-12))
                PushHistory(_s!, _y!, T.One / sy);

            (_x, _xNew) = (_xNew, _x);
            (_g, _gNew) = (_gNew, _g);
        }

        return (T[])_x!.Clone();
    }

    private void ComputeSearchDirection()
    {
        _g!.AsSpan().CopyTo(_q!);
        int count = _historyCount;

        for (int i = 0; i < count; i++)
        {
            int idx = HistoryIndex(i);
            _alpha![i] = _rho![idx] * CalculusVectorOps.Dot(_sHistory![idx], _q!);
            CalculusVectorOps.SubScaled(_q!, _yHistory![idx], _alpha![i]);
        }

        T gamma = T.One;
        if (count > 0)
        {
            int newest = HistoryIndex(0);
            T yy = CalculusVectorOps.Dot(_yHistory![newest], _yHistory[newest]);
            if (yy > T.Zero)
                gamma = CalculusVectorOps.Dot(_sHistory![newest], _yHistory[newest]) / yy;
        }

        CalculusVectorOps.Scale(_q!, gamma);

        for (int i = count - 1; i >= 0; i--)
        {
            int idx = HistoryIndex(i);
            T beta = _rho![idx] * CalculusVectorOps.Dot(_yHistory![idx], _q!);
            T coeff = _alpha![i] - beta;
            CalculusVectorOps.AddScaled(_q!, _sHistory![idx], coeff);
        }
    }

    private int HistoryIndex(int offsetFromNewest)
    {
        if (_historyCount < _historySize)
            return _historyCount - 1 - offsetFromNewest;
        return (_historyStart + _historyCount - 1 - offsetFromNewest + _historySize) % _historySize;
    }

    private void PushHistory(T[] s, T[] y, T rho)
    {
        int slot;
        if (_historyCount < _historySize)
        {
            slot = _historyCount;
            _historyCount++;
        }
        else
        {
            slot = _historyStart;
            _historyStart = (_historyStart + 1) % _historySize;
        }

        s.AsSpan().CopyTo(_sHistory![slot]);
        y.AsSpan().CopyTo(_yHistory![slot]);
        _rho![slot] = rho;
    }

    private void EnsureCapacity(int n)
    {
        if (n <= _n && _x is not null)
            return;

        _n = n;
        _x = new T[n];
        _g = new T[n];
        _xNew = new T[n];
        _gNew = new T[n];
        _s = new T[n];
        _y = new T[n];
        _q = new T[n];
        _trial = new T[n];
        _alpha = new T[_historySize];
        _rho = new T[_historySize];
        _sHistory = new T[_historySize][];
        _yHistory = new T[_historySize][];
        for (int i = 0; i < _historySize; i++)
        {
            _sHistory[i] = new T[n];
            _yHistory[i] = new T[n];
        }
    }

}
