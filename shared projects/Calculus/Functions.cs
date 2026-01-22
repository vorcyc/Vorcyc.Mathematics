namespace Vorcyc.Mathematics.Calculus;

using System.Numerics;

/// <summary>
/// Represents a function that takes a single floating-point value and returns a floating-point result of the same type.
/// </summary>
/// <typeparam name="T">The floating-point type of the input and output values. Must implement <see cref="IFloatingPointIeee754{TSelf}"/> .</typeparam>
/// <param name="x">The input value for the function.</param>
/// <returns>The result of evaluating the function at the specified input value.</returns>
public delegate T SingleVariableFunction<T>(T x) where T : struct, IFloatingPointIeee754<T>;

/// <summary>
/// Represents a function that computes a value of type T from a read-only span of input arguments.
/// </summary>
/// <remarks>This delegate is typically used to represent mathematical functions of multiple variables, such as
/// those used in numerical analysis or optimization. The function implementation should not modify the contents of the
/// input span.</remarks>
/// <typeparam name="T">The floating-point type of the input arguments and the return value. Must implement <see cref="IFloatingPointIeee754{TSelf}"/>.</typeparam>
/// <param name="args">A read-only span containing the input arguments for the function. The number and meaning of arguments depend on the
/// specific function implementation.</param>
/// <returns>The computed value of type T based on the provided input arguments.</returns>
public delegate T MultiVariableFunction<T>(ReadOnlySpan<T> args) where T : struct, IFloatingPointIeee754<T>;


/// <summary>
/// 表示微分方程 dy/dx = f(x,y) 的委托。
/// </summary>
/// <param name="x">自变量 x</param>
/// <param name="y">因变量 y</param>
/// <returns>导数值 dy/dx</returns>
public delegate T DifferentialFunction<T>(T x, T y) where T : struct, IFloatingPointIeee754<T>;