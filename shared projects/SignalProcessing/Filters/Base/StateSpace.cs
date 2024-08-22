namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// State-space representation of filter.
/// </summary>
public class StateSpace
{
    /// <summary>
    /// Gets or sets state matrix.
    /// </summary>
    public float[][] A { get; set; }

    /// <summary>
    /// Gets or sets input-to-state matrix.
    /// </summary>
    public float[] B { get; set; }

    /// <summary>
    /// Gets or sets state-to-output matrix.
    /// </summary>
    public float[] C { get; set; }

    /// <summary>
    /// Gets or sets feedthrough matrix.
    /// </summary>
    public float[] D { get; set; }
}
