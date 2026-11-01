namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Gradient adaptive step-size (GASS) variants of the LMS algorithm, see <see cref="GassLmsFilter"/>.
/// They differ in the way the sensitivity of the weights to the step size is tracked.
/// </summary>
public enum GassVariant
{
    /// <summary>
    /// Benveniste: the exact recursion psi(n+1) = (I - mu x xT) psi(n) + e x.
    /// </summary>
    Benveniste,

    /// <summary>
    /// Ang and Farhang-Boroujeny: leaky recursion psi(n+1) = alpha psi(n) + e x.
    /// </summary>
    Ang,

    /// <summary>
    /// Mathews and Xie: psi(n+1) = e x, i.e. mu(n) = mu(n-1) + rho e(n) e(n-1) x(n)T x(n-1).
    /// </summary>
    Mathews,
}
