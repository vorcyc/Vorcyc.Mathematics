using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

/// <summary>标量 ODE 事件：返回 true 时停止积分。</summary>
public delegate bool OdeEvent<T>(T x, T y) where T : struct, IFloatingPointIeee754<T>;

/// <summary>ODE 方程组事件。</summary>
public delegate bool OdeSystemEvent<T>(T x, ReadOnlySpan<T> y) where T : struct, IFloatingPointIeee754<T>;

/// <summary>标量 ODE 积分轨迹。</summary>
public sealed class OdeTrajectory<T> where T : struct, IFloatingPointIeee754<T>
{
    public T[] X { get; }
    public T[] Y { get; }

    public OdeTrajectory(T[] x, T[] y)
    {
        X = x ?? throw new ArgumentNullException(nameof(x));
        Y = y ?? throw new ArgumentNullException(nameof(y));
        if (X.Length != Y.Length)
            throw new ArgumentException("轨迹 x/y 长度必须一致");
    }

    public int Count => X.Length;
}

/// <summary>ODE 方程组积分轨迹，States[component][step]。</summary>
public sealed class OdeSystemTrajectory<T> where T : struct, IFloatingPointIeee754<T>
{
    public T[] X { get; }
    public T[][] States { get; }

    public OdeSystemTrajectory(T[] x, T[][] states)
    {
        X = x ?? throw new ArgumentNullException(nameof(x));
        States = states ?? throw new ArgumentNullException(nameof(states));
        if (States.Length == 0) throw new ArgumentException("状态维数必须大于 0", nameof(states));
        if (States[0].Length != X.Length)
            throw new ArgumentException("轨迹长度必须一致");
    }

    public int Dimension => States.Length;
    public int Count => X.Length;
}
