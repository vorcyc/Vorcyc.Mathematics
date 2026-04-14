using System.Numerics;
using Vorcyc.Mathematics.Calculus;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Calculus.Optimization;

/// <summary>
/// Levenberg-Marquardt 法求解非线性最小二乘 min Σ rᵢ²。
/// </summary>
public sealed class LevenbergMarquardt<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly VectorFieldFunction<T> _residuals;
    private readonly Jacobian<T> _jacobian;
    private readonly int _dim;
    private readonly int _residualCount;
    private readonly T _ten;
    private readonly T _lambdaFloor;

    private T[]? _x;
    private T[]? _r;
    private T[]? _rTrial;
    private T[]? _xTrial;
    private T[]? _rhs;
    private T[]? _delta;
    private T[,]? _augmented;
    private Matrix<T>? _jtj;
    private Matrix<T>? _jacobianMatrix;

    public LevenbergMarquardt(VectorFieldFunction<T> residuals, int dimension, int residualCount, T defaultH)
    {
        _residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
        if (dimension < 1) throw new ArgumentException("维数必须大于等于 1", nameof(dimension));
        if (residualCount < 1) throw new ArgumentException("残差维数必须大于等于 1", nameof(residualCount));
        _dim = dimension;
        _residualCount = residualCount;
        _jacobian = new Jacobian<T>(residuals, residualCount, defaultH);
        _ten = T.CreateChecked(10);
        _lambdaFloor = T.CreateChecked(1e-12);
    }

    /// <summary>求解最小二乘问题。</summary>
    public T[] Solve(T[] initial, int maxIterations = 100, T? tolerance = null, T? initialLambda = null)
    {
        if (initial.Length != _dim)
            throw new ArgumentException("初始点维数不匹配", nameof(initial));

        EnsureCapacity();
        initial.AsSpan().CopyTo(_x!);

        T tol = tolerance ?? T.CreateChecked(1e-10);
        T lambda = initialLambda ?? T.CreateChecked(1e-3);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            T cost = ResidualCost(_x!, _r!);
            if (cost < tol)
                return (T[])_x!.Clone();

            _jacobianMatrix = _jacobian.Calculate(_x!.AsSpan(), null, Derivative<T>.Method.Central, _jacobianMatrix);
            CalculusVectorOps.JacobianTransposeJacobian(_jacobianMatrix!, _jtj!);
            for (int i = 0; i < _dim; i++)
                _jtj![i, i] += lambda;

            CalculusVectorOps.JacobianTransposeVector(_jacobianMatrix!, _r!, _rhs!);
            for (int i = 0; i < _dim; i++)
                _rhs![i] = -_rhs![i];

            LinearEquationSolver.GaussianEliminationSolve(_jtj!, _rhs!, _delta!, _augmented);

            for (int i = 0; i < _dim; i++)
                _xTrial![i] = _x![i] + _delta![i];

            T trialCost = ResidualCost(_xTrial!, _rTrial!);
            if (trialCost < cost)
            {
                Array.Copy(_xTrial!, _x!, _dim);
                Array.Copy(_rTrial!, _r!, _residualCount);
                lambda = T.Max(lambda / _ten, _lambdaFloor);
            }
            else
            {
                lambda *= _ten;
            }
        }

        return (T[])_x!.Clone();
    }

    private void EnsureCapacity()
    {
        _x ??= new T[_dim];
        _r ??= new T[_residualCount];
        _rTrial ??= new T[_residualCount];
        _xTrial ??= new T[_dim];
        _rhs ??= new T[_dim];
        _delta ??= new T[_dim];
        _augmented ??= new T[_dim, _dim + 1];
        _jtj ??= new Matrix<T>(_dim, _dim);
        _jacobianMatrix ??= new Matrix<T>(_residualCount, _dim);
    }

    private T ResidualCost(ReadOnlySpan<T> x, Span<T> buffer)
    {
        _residuals(x, buffer);
        return CalculusVectorOps.Dot(buffer, buffer);
    }
}
