namespace Vorcyc.Mathematics.Calculus;

using System.Numerics;

public delegate T SingleVariableFunction<T>(T x)
    where T : struct, IFloatingPointIeee754<T>;

public delegate T MultiVariableFunction<T>(ReadOnlySpan<T> args) where T : struct, IFloatingPointIeee754<T>;


/// <summary>
/// 表示微分方程 dy/dx = f(x,y) 的委托。
/// </summary>
/// <param name="x">自变量 x</param>
/// <param name="y">因变量 y</param>
/// <returns>导数值 dy/dx</returns>
public delegate T DifferentialFunction<T>(T x, T y) where T : struct, IFloatingPointIeee754<T>;