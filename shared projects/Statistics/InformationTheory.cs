// Information-theoretic primitives
// Provides Shannon entropy and divergence measures (Kullback-Leibler, Jensen-Shannon)
// over discrete probability distributions.

using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides information-theoretic measures over discrete probability distributions,
/// including Shannon entropy, Kullback-Leibler divergence, and Jensen-Shannon divergence.
/// </summary>
/// <remarks>
/// Inputs are expected to be discrete probability distributions (non-negative values that
/// sum to one). Probabilities equal to zero are treated as contributing zero to the sums,
/// consistent with the convention that the limit of <c>p * log(p)</c> as <c>p</c> approaches
/// zero is zero.
/// </remarks>
public static class InformationTheory
{
    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution using the natural logarithm (nats).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <returns>The Shannon entropy in nats.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T Entropy<T>(ReadOnlySpan<T> distribution)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
    {
        if (distribution.IsEmpty)
            throw new ArgumentException("The distribution cannot be empty.", nameof(distribution));

        T sum = T.Zero;
        for (int i = 0; i < distribution.Length; i++)
        {
            T p = distribution[i];
            if (p > T.Zero)
                sum += p * T.Log(p);
        }

        return -sum;
    }

    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution using the natural logarithm (nats).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <returns>The Shannon entropy in nats.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T Entropy<T>(T[] distribution)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
        => Entropy<T>((ReadOnlySpan<T>)distribution);

    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution using the natural logarithm (nats),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <param name="context">
    /// The execution policy. When <see langword="null"/>, the ambient or default policy is used.
    /// Parallel execution is only applied for sufficiently large distributions; otherwise the scalar
    /// implementation is used and the result is identical.
    /// </param>
    /// <returns>The Shannon entropy in nats.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T Entropy<T>(ReadOnlySpan<T> distribution, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
    {
        if (distribution.IsEmpty)
            throw new ArgumentException("The distribution cannot be empty.", nameof(distribution));

        if (!ComputingContextExecution.UseParallel(context, distribution.Length))
            return Entropy<T>(distribution);

        T[] data = distribution.ToArray();
        T sum = StatisticsParallel.ReduceParallel(data.Length, context, data, static (array, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                T p = array[i];
                if (p > T.Zero)
                    local += p * T.Log(p);
            }
            return local;
        });

        return -sum;
    }

    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution using the natural logarithm (nats),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <param name="context">The execution policy. When <see langword="null"/>, the ambient or default policy is used.</param>
    /// <returns>The Shannon entropy in nats.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T Entropy<T>(T[] distribution, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
        => Entropy<T>((ReadOnlySpan<T>)distribution, context);

    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution in bits (base-2 logarithm).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <returns>The Shannon entropy in bits.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T EntropyBits<T>(ReadOnlySpan<T> distribution)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
    {
        if (distribution.IsEmpty)
            throw new ArgumentException("The distribution cannot be empty.", nameof(distribution));

        T sum = T.Zero;
        for (int i = 0; i < distribution.Length; i++)
        {
            T p = distribution[i];
            if (p > T.Zero)
                sum += p * T.Log2(p);
        }

        return -sum;
    }

    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution in bits (base-2 logarithm).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <returns>The Shannon entropy in bits.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T EntropyBits<T>(T[] distribution)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
        => EntropyBits<T>((ReadOnlySpan<T>)distribution);

    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution in bits (base-2 logarithm),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <param name="context">
    /// The execution policy. When <see langword="null"/>, the ambient or default policy is used.
    /// Parallel execution is only applied for sufficiently large distributions; otherwise the scalar
    /// implementation is used and the result is identical.
    /// </param>
    /// <returns>The Shannon entropy in bits.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T EntropyBits<T>(ReadOnlySpan<T> distribution, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
    {
        if (distribution.IsEmpty)
            throw new ArgumentException("The distribution cannot be empty.", nameof(distribution));

        if (!ComputingContextExecution.UseParallel(context, distribution.Length))
            return EntropyBits<T>(distribution);

        T[] data = distribution.ToArray();
        T sum = StatisticsParallel.ReduceParallel(data.Length, context, data, static (array, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                T p = array[i];
                if (p > T.Zero)
                    local += p * T.Log2(p);
            }
            return local;
        });

        return -sum;
    }

    /// <summary>
    /// Computes the Shannon entropy of a discrete probability distribution in bits (base-2 logarithm),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="distribution">The probability distribution.</param>
    /// <param name="context">The execution policy. When <see langword="null"/>, the ambient or default policy is used.</param>
    /// <returns>The Shannon entropy in bits.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="distribution"/> is empty.</exception>
    public static T EntropyBits<T>(T[] distribution, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
        => EntropyBits<T>((ReadOnlySpan<T>)distribution, context);

    /// <summary>
    /// Computes the Kullback-Leibler divergence from distribution <paramref name="q"/> to distribution <paramref name="p"/> using the natural logarithm (nats).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="p">The reference probability distribution.</param>
    /// <param name="q">The approximating probability distribution.</param>
    /// <returns>
    /// The Kullback-Leibler divergence <c>D(p || q)</c> in nats. Returns <see cref="IFloatingPointIeee754{T}.PositiveInfinity"/>
    /// when <paramref name="q"/> assigns zero probability to an outcome that <paramref name="p"/> assigns positive probability.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the distributions have different lengths or are empty.</exception>
    /// <remarks>
    /// The Kullback-Leibler divergence is asymmetric: <c>D(p || q)</c> is generally not equal to <c>D(q || p)</c>.
    /// </remarks>
    public static T KullbackLeiblerDivergence<T>(ReadOnlySpan<T> p, ReadOnlySpan<T> q)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
    {
        if (p.Length != q.Length)
            throw new ArgumentException("The lengths of the two distributions must be equal.");
        if (p.IsEmpty)
            throw new ArgumentException("The distributions cannot be empty.", nameof(p));

        T sum = T.Zero;
        for (int i = 0; i < p.Length; i++)
        {
            T pi = p[i];
            if (pi <= T.Zero)
                continue;
            if (q[i] <= T.Zero)
                return T.PositiveInfinity;

            sum += pi * T.Log(pi / q[i]);
        }

        return sum;
    }

    /// <summary>
    /// Computes the Kullback-Leibler divergence from distribution <paramref name="q"/> to distribution <paramref name="p"/> using the natural logarithm (nats).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="p">The reference probability distribution.</param>
    /// <param name="q">The approximating probability distribution.</param>
    /// <returns>The Kullback-Leibler divergence <c>D(p || q)</c> in nats.</returns>
    /// <exception cref="ArgumentException">Thrown when the distributions have different lengths or are empty.</exception>
    public static T KullbackLeiblerDivergence<T>(T[] p, T[] q)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
        => KullbackLeiblerDivergence<T>((ReadOnlySpan<T>)p, (ReadOnlySpan<T>)q);

    /// <summary>
    /// Computes the Kullback-Leibler divergence from distribution <paramref name="q"/> to distribution <paramref name="p"/> using the natural logarithm (nats),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="p">The reference probability distribution.</param>
    /// <param name="q">The approximating probability distribution.</param>
    /// <param name="context">
    /// The execution policy. When <see langword="null"/>, the ambient or default policy is used.
    /// Parallel execution is only applied for sufficiently large distributions; otherwise the scalar
    /// implementation is used and the result is identical.
    /// </param>
    /// <returns>
    /// The Kullback-Leibler divergence <c>D(p || q)</c> in nats. Returns <see cref="IFloatingPointIeee754{T}.PositiveInfinity"/>
    /// when <paramref name="q"/> assigns zero probability to an outcome that <paramref name="p"/> assigns positive probability.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the distributions have different lengths or are empty.</exception>
    /// <remarks>
    /// The Kullback-Leibler divergence is asymmetric: <c>D(p || q)</c> is generally not equal to <c>D(q || p)</c>.
    /// </remarks>
    public static T KullbackLeiblerDivergence<T>(ReadOnlySpan<T> p, ReadOnlySpan<T> q, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
    {
        if (p.Length != q.Length)
            throw new ArgumentException("The lengths of the two distributions must be equal.");
        if (p.IsEmpty)
            throw new ArgumentException("The distributions cannot be empty.", nameof(p));

        if (!ComputingContextExecution.UseParallel(context, p.Length))
            return KullbackLeiblerDivergence<T>(p, q);

        T[] pData = p.ToArray();
        T[] qData = q.ToArray();

        // Each chunk returns PositiveInfinity if it detects an outcome where q assigns zero
        // probability but p assigns positive probability; since (+inf + x) == +inf, summing the
        // partial results preserves the same short-circuit semantics as the scalar implementation.
        return StatisticsParallel.ReduceParallel(pData.Length, context, pData, qData, static (ps, qs, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                T pi = ps[i];
                if (pi <= T.Zero)
                    continue;
                if (qs[i] <= T.Zero)
                    return T.PositiveInfinity;

                local += pi * T.Log(pi / qs[i]);
            }

            return local;
        });
    }

    /// <summary>
    /// Computes the Kullback-Leibler divergence from distribution <paramref name="q"/> to distribution <paramref name="p"/> using the natural logarithm (nats),
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="p">The reference probability distribution.</param>
    /// <param name="q">The approximating probability distribution.</param>
    /// <param name="context">The execution policy. When <see langword="null"/>, the ambient or default policy is used.</param>
    /// <returns>The Kullback-Leibler divergence <c>D(p || q)</c> in nats.</returns>
    /// <exception cref="ArgumentException">Thrown when the distributions have different lengths or are empty.</exception>
    public static T KullbackLeiblerDivergence<T>(T[] p, T[] q, ComputingContext? context)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
        => KullbackLeiblerDivergence<T>((ReadOnlySpan<T>)p, (ReadOnlySpan<T>)q, context);

    /// <summary>
    /// Computes the Jensen-Shannon divergence between two discrete probability distributions using the natural logarithm (nats).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="p">The first probability distribution.</param>
    /// <param name="q">The second probability distribution.</param>
    /// <returns>The Jensen-Shannon divergence, a symmetric and finite measure in the range [0, ln 2].</returns>
    /// <exception cref="ArgumentException">Thrown when the distributions have different lengths or are empty.</exception>
    /// <remarks>
    /// The Jensen-Shannon divergence is the symmetrized and smoothed version of the
    /// Kullback-Leibler divergence, defined as the average of <c>D(p || m)</c> and <c>D(q || m)</c>
    /// where <c>m = (p + q) / 2</c>. Unlike the Kullback-Leibler divergence, it is always finite.
    /// </remarks>
    public static T JensenShannonDivergence<T>(ReadOnlySpan<T> p, ReadOnlySpan<T> q)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
    {
        if (p.Length != q.Length)
            throw new ArgumentException("The lengths of the two distributions must be equal.");
        if (p.IsEmpty)
            throw new ArgumentException("The distributions cannot be empty.", nameof(p));

        int n = p.Length;
        T half = T.One / T.CreateChecked(2);
        T[] m = new T[n];
        for (int i = 0; i < n; i++)
            m[i] = (p[i] + q[i]) * half;

        T klPM = KullbackLeiblerDivergence<T>(p, m);
        T klQM = KullbackLeiblerDivergence<T>(q, m);
        return (klPM + klQM) * half;
    }

    /// <summary>
    /// Computes the Jensen-Shannon divergence between two discrete probability distributions using the natural logarithm (nats).
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="IFloatingPointIeee754{T}"/> and <see cref="ILogarithmicFunctions{T}"/>.</typeparam>
    /// <param name="p">The first probability distribution.</param>
    /// <param name="q">The second probability distribution.</param>
    /// <returns>The Jensen-Shannon divergence between <paramref name="p"/> and <paramref name="q"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the distributions have different lengths or are empty.</exception>
    public static T JensenShannonDivergence<T>(T[] p, T[] q)
        where T : IFloatingPointIeee754<T>, ILogarithmicFunctions<T>
        => JensenShannonDivergence<T>((ReadOnlySpan<T>)p, (ReadOnlySpan<T>)q);
}
