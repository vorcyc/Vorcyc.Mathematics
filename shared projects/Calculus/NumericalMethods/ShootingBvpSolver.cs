using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

/// <summary>
/// 打靶法求解二阶边值问题（降阶为一阶方程组）。
/// </summary>
public sealed class ShootingBvpSolver<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly RungeKuttaSystem<T> _rk;
    private readonly T _slopeStep;
    private readonly T _invSlopeStep;
    private readonly T _minJacobian;

    private T[]? _initial;
    private T[]? _state;
    private T[]? _perturbed;

    /// <summary>
    /// 创建打靶求解器。
    /// </summary>
    /// <param name="firstOrderSystem">一阶系统 dy/dx = f(x,y)，y[0] 为函数值，y[1] 为导数</param>
    public ShootingBvpSolver(OdeSystemFunction<T> firstOrderSystem)
    {
        _rk = new RungeKuttaSystem<T>(firstOrderSystem);
        _slopeStep = T.CreateChecked(1e-5);
        _invSlopeStep = T.One / _slopeStep;
        _minJacobian = T.CreateChecked(1e-15);
    }

    /// <summary>
    /// 求解 y(x0)=yLeft、y(xEnd)=yRight，未知初始导数 y'(x0)。
    /// </summary>
    public T[] Solve(
        T x0, T xEnd,
        T yLeft, T yRight,
        T initialSlopeGuess,
        int steps = 200,
        int maxNewton = 30,
        T? tolerance = null)
    {
        EnsureBuffers();
        T tol = tolerance ?? T.CreateChecked(1e-8);
        T slope = initialSlopeGuess;

        for (int iter = 0; iter < maxNewton; iter++)
        {
            _initial![0] = yLeft;
            _initial[1] = slope;
            _initial.AsSpan().CopyTo(_state!);
            _rk.SolveInPlace(x0, _state!, xEnd, steps);

            T yEndValue = _state![0];
            T residual = yEndValue - yRight;
            if (T.Abs(residual) < tol)
                return (T[])_state.Clone();

            _initial[1] = slope + _slopeStep;
            _initial.AsSpan().CopyTo(_perturbed!);
            _rk.SolveInPlace(x0, _perturbed!, xEnd, steps);

            T derivative = (_perturbed![0] - yEndValue) * _invSlopeStep;
            if (T.Abs(derivative) < _minJacobian)
                throw new InvalidOperationException("打靶法雅可比接近奇异");

            slope -= residual / derivative;
        }

        throw new InvalidOperationException("打靶法未收敛");
    }

    private void EnsureBuffers()
    {
        if (_initial is not null)
            return;

        _initial = new T[2];
        _state = new T[2];
        _perturbed = new T[2];
    }
}
