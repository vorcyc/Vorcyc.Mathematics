using System.Numerics;
using Vorcyc.Mathematics.Calculus;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

/// <summary>
/// 使用牛顿法求解非线性方程组 F(x) = 0，F: Rⁿ→Rⁿ。
/// </summary>
/// <typeparam name="T">浮点类型</typeparam>
public sealed class NewtonRaphsonSystem<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly VectorFieldFunction<T> _func;
    private readonly Jacobian<T> _jacobian;
    private readonly int _dimension;
    private readonly T _half;

    private T[]? _x;
    private T[]? _residual;
    private T[]? _trial;
    private T[]? _lsResidual;
    private T[]? _delta;
    private T[,]? _augmented;
    private Matrix<T>? _jacobianMatrix;

    public NewtonRaphsonSystem(VectorFieldFunction<T> func, int dimension, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        if (dimension < 1) throw new ArgumentException("维数必须大于等于 1", nameof(dimension));
        _dimension = dimension;
        _jacobian = new Jacobian<T>(func, dimension, defaultH);
        _half = T.CreateChecked(0.5);
    }

    /// <summary>
    /// 求解 F(x) = 0。
    /// </summary>
    /// <param name="useLineSearch">为 true 时对牛顿步做回溯线搜索以改善收敛。</param>
    public T[] Solve(T[] initialGuess, int maxIterations = 50, T? tolerance = null, bool useLineSearch = false)
    {
        if (initialGuess.Length != _dimension)
            throw new ArgumentException("初始猜测维数与系统维数不匹配", nameof(initialGuess));

        EnsureBuffers();
        initialGuess.AsSpan().CopyTo(_x!);
        T tol = tolerance ?? T.CreateChecked(1e-10);
        T tol2 = tol * tol;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            _func(_x!, _residual!);
            if (CalculusVectorOps.Dot(_residual!, _residual!) < tol2)
                return (T[])_x!.Clone();

            _jacobianMatrix = _jacobian.Calculate(_x!, null, Derivative<T>.Method.Central, _jacobianMatrix);
            LinearEquationSolver.GaussianEliminationSolve(_jacobianMatrix!, _residual!, _delta!, _augmented);

            if (!useLineSearch)
            {
                for (int i = 0; i < _dimension; i++)
                    _x![i] -= _delta![i];
                continue;
            }

            T alpha = T.One;
            T currentNorm2 = CalculusVectorOps.Dot(_residual!, _residual!);
            for (int ls = 0; ls < 20; ls++)
            {
                for (int i = 0; i < _dimension; i++)
                    _trial![i] = _x![i] - alpha * _delta![i];
                _func(_trial!, _lsResidual!);
                T trialNorm2 = CalculusVectorOps.Dot(_lsResidual!, _lsResidual!);
                if (trialNorm2 < currentNorm2)
                {
                    _trial.AsSpan().CopyTo(_x!);
                    break;
                }

                alpha *= _half;
                if (ls == 19)
                {
                    for (int i = 0; i < _dimension; i++)
                        _x![i] -= alpha * _delta![i];
                }
            }
        }

        throw new InvalidOperationException("多元牛顿法未在指定迭代次数内收敛");
    }

    private void EnsureBuffers()
    {
        _x ??= new T[_dimension];
        _residual ??= new T[_dimension];
        _trial ??= new T[_dimension];
        _lsResidual ??= new T[_dimension];
        _delta ??= new T[_dimension];
        _augmented ??= new T[_dimension, _dimension + 1];
        _jacobianMatrix ??= new Matrix<T>(_dimension, _dimension);
    }
}
