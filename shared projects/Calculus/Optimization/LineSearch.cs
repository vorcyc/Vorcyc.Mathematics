using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.Optimization;

/// <summary>线搜索工具（Armijo 回溯）。</summary>
public static class LineSearch
{
    /// <summary>
    /// Armijo 回溯：寻找 α 使 f(x + α·d) ≤ f(x) + c₁·α·(∇f·d)。
    /// </summary>
    public static T ArmijoBacktracking<T>(
        ReadOnlySpan<T> x,
        ReadOnlySpan<T> direction,
        T fx,
        ReadOnlySpan<T> gradient,
        MultiVariableFunction<T> func,
        T c1 = default,
        T alpha0 = default,
        T rho = default,
        int maxIterations = 40) where T : struct, IFloatingPointIeee754<T>
    {
        var rented = new T[x.Length];
        try
        {
            return ArmijoBacktracking(x, direction, fx, gradient, func, rented, c1, alpha0, rho, maxIterations);
        }
        finally
        {
            Array.Clear(rented);
        }
    }

    /// <summary>
    /// Armijo 回溯（复用 <paramref name="trial"/> 缓冲区，长度须 ≥ x.Length）。
    /// </summary>
    public static T ArmijoBacktracking<T>(
        ReadOnlySpan<T> x,
        ReadOnlySpan<T> direction,
        T fx,
        ReadOnlySpan<T> gradient,
        MultiVariableFunction<T> func,
        Span<T> trial,
        T c1 = default,
        T alpha0 = default,
        T rho = default,
        int maxIterations = 40) where T : struct, IFloatingPointIeee754<T>
    {
        if (x.Length != direction.Length || x.Length != gradient.Length)
            throw new ArgumentException("向量维数不匹配");
        if (trial.Length < x.Length)
            throw new ArgumentException("试探向量缓冲区长度不足", nameof(trial));

        c1 = c1 == T.Zero ? T.CreateChecked(1e-4) : c1;
        alpha0 = alpha0 == T.Zero ? T.One : alpha0;
        rho = rho == T.Zero ? T.CreateChecked(0.5) : rho;

        T slope = CalculusVectorOps.Dot(gradient, direction);
        if (slope >= T.Zero)
            return T.Zero;

        T alpha = alpha0;
        var trialSlice = trial[..x.Length];
        for (int iter = 0; iter < maxIterations; iter++)
        {
            for (int i = 0; i < x.Length; i++)
                trialSlice[i] = x[i] + alpha * direction[i];

            T fTrial = func(trialSlice);
            if (fTrial <= fx + c1 * alpha * slope)
                return alpha;

            alpha *= rho;
        }

        return alpha;
    }

}
