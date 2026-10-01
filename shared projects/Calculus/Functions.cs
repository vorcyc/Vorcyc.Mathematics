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
/// Represents a delegate for the differential equation dy/dx = f(x,y).
/// </summary>
/// <param name="x">The independent variable x.</param>
/// <param name="y">The dependent variable y.</param>
/// <returns>The derivative value dy/dx.</returns>
public delegate T DifferentialFunction<T>(T x, T y) where T : struct, IFloatingPointIeee754<T>;

/// <summary>
/// Represents a system of ordinary differential equations dy/dx = f(x, y), where y is a vector.
/// </summary>
/// <param name="x">The independent variable.</param>
/// <param name="y">The state vector.</param>
/// <param name="dydx">The output derivative vector, with the same length as <paramref name="y"/>.</param>
public delegate void OdeSystemFunction<T>(T x, ReadOnlySpan<T> y, Span<T> dydx) where T : struct, IFloatingPointIeee754<T>;

/// <summary>
/// Represents a vector-valued function f: R^n → R^m, writing its result to <paramref name="output"/>.
/// </summary>
/// <param name="point">The input point.</param>
/// <param name="output">The output vector, with length m.</param>
public delegate void VectorFieldFunction<T>(ReadOnlySpan<T> point, Span<T> output) where T : struct, IFloatingPointIeee754<T>;