using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Result of a hypothesis test including statistic, p-value, and degrees of freedom.
/// </summary>
/// <typeparam name="T">Numeric type for statistic and p-value.</typeparam>
public readonly record struct HypothesisTestResult<T>(T Statistic, T PValue, int DegreesOfFreedom)
    where T : IFloatingPointIeee754<T>
{
    /// <summary>
    /// Returns whether the null hypothesis is rejected at <paramref name="alpha"/>.
    /// </summary>
    public bool RejectsNullHypothesisAt(double alpha) =>
        double.CreateChecked(PValue) < alpha;
}
