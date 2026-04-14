namespace Vorcyc.Mathematics;

/// <summary>
/// CPU execution strategy for numerical kernels.
/// </summary>
public enum CpuExecutionMode
{
    /// <summary>Scalar or simple loop implementation.</summary>
    Normal,

    /// <summary>Hardware SIMD via <see cref="System.Numerics.Vector{T}"/>.</summary>
    Simd,

    /// <summary>Multi-threaded implementation.</summary>
    Parallel,

    /// <summary>Heuristic selection based on problem size.</summary>
    Auto
}
