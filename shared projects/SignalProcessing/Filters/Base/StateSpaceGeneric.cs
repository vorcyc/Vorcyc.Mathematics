using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// State-space representation of a filter with coefficients in <typeparamref name="T"/>.
/// </summary>
public class StateSpace<T>
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Gets or sets the state matrix.</summary>
    public Matrix<T> A { get; set; } = null!;

    /// <summary>Gets or sets the input-to-state vector.</summary>
    public T[] B { get; set; } = [];

    /// <summary>Gets or sets the state-to-output vector.</summary>
    public T[] C { get; set; } = [];

    /// <summary>Gets or sets the feedthrough vector.</summary>
    public T[] D { get; set; } = [];
}

/// <summary>Double-precision state-space (same as <see cref="StateSpace{T}"/> of <see cref="double"/>).</summary>
public sealed class StateSpace64 : StateSpace<double>
{
}
