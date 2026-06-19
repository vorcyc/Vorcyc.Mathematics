// Correlation primitives
// Provides T[]/ReadOnlySpan<T>-friendly correlation measures (Pearson) that act as the
// single source of truth so that other modules (e.g. MachineLearning distances) can reuse
// them instead of duplicating the underlying math.

using System.Numerics;
using System.Runtime.CompilerServices;
using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Provides correlation measures between two equal-length data sets, including the
/// Pearson product-moment correlation coefficient.
/// </summary>
/// <remarks>
/// These methods are intended as low-level, allocation-free primitives that operate on
/// <see cref="ReadOnlySpan{T}"/> inputs. They are reused by higher-level statistical APIs
/// and by the machine learning distance/similarity layer to avoid duplicated formulas.
/// </remarks>
public static partial class Correlation
{
    /// <summary>
    /// Computes the Pearson product-moment correlation coefficient between two data sets.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>
    /// The Pearson correlation coefficient in the range [-1, 1]. Returns <see cref="INumberBase{T}.Zero"/>
    /// when either data set has zero variance (the coefficient is undefined in that case).
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    /// <remarks>
    /// The implementation uses a mean-centered formulation for improved numerical stability
    /// compared to a raw sum-of-products accumulation.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PearsonCorrelation<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
        where T : INumber<T>, IRootFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two data sets must be equal.");
        if (x.IsEmpty)
            throw new ArgumentException("The data sets cannot be empty.", nameof(x));

        T n = T.CreateChecked(x.Length);

        T sumX = T.Zero;
        T sumY = T.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            sumX += x[i];
            sumY += y[i];
        }

        T meanX = sumX / n;
        T meanY = sumY / n;

        T sxy = T.Zero;
        T sxx = T.Zero;
        T syy = T.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            T dx = x[i] - meanX;
            T dy = y[i] - meanY;
            sxy += dx * dy;
            sxx += dx * dx;
            syy += dy * dy;
        }

        T denominator = T.Sqrt(sxx * syy);
        return denominator == T.Zero ? T.Zero : sxy / denominator;
    }

    /// <summary>
    /// Computes the Pearson product-moment correlation coefficient between two data sets,
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <param name="context">
    /// The execution policy. When <see langword="null"/>, the ambient or default policy is used.
    /// Parallel execution is only applied for sufficiently large data sets; otherwise the scalar
    /// implementation is used and the result is identical.
    /// </param>
    /// <returns>
    /// The Pearson correlation coefficient in the range [-1, 1]. Returns <see cref="INumberBase{T}.Zero"/>
    /// when either data set has zero variance (the coefficient is undefined in that case).
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    public static T PearsonCorrelation<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, ComputingContext? context)
        where T : INumber<T>, IRootFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two data sets must be equal.");
        if (x.IsEmpty)
            throw new ArgumentException("The data sets cannot be empty.", nameof(x));

        if (!ComputingContextExecution.UseParallel(context, x.Length))
            return PearsonCorrelation(x, y);

        T[] xData = x.ToArray();
        T[] yData = y.ToArray();
        int length = xData.Length;
        T n = T.CreateChecked(length);

        T sumX = StatisticsParallel.ReduceParallel(length, context, xData, static (data, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
                local += data[i];
            return local;
        });

        T sumY = StatisticsParallel.ReduceParallel(length, context, yData, static (data, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
                local += data[i];
            return local;
        });

        T meanX = sumX / n;
        T meanY = sumY / n;

        T sxy = StatisticsParallel.ReduceParallel(length, context, xData, yData, (xs, ys, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
                local += (xs[i] - meanX) * (ys[i] - meanY);
            return local;
        });

        T sxx = StatisticsParallel.ReduceParallel(length, context, xData, (data, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                T dx = data[i] - meanX;
                local += dx * dx;
            }
            return local;
        });

        T syy = StatisticsParallel.ReduceParallel(length, context, yData, (data, start, end) =>
        {
            T local = T.Zero;
            for (int i = start; i < end; i++)
            {
                T dy = data[i] - meanY;
                local += dy * dy;
            }
            return local;
        });

        T denominator = T.Sqrt(sxx * syy);
        return denominator == T.Zero ? T.Zero : sxy / denominator;
    }

    /// <summary>
    /// Computes the Pearson product-moment correlation coefficient between two data sets,
    /// honoring the supplied <see cref="ComputingContext"/> execution policy.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <param name="context">The execution policy. When <see langword="null"/>, the ambient or default policy is used.</param>
    /// <returns>The Pearson correlation coefficient in the range [-1, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PearsonCorrelation<T>(T[] x, T[] y, ComputingContext? context)
        where T : INumber<T>, IRootFunctions<T>
        => PearsonCorrelation<T>((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y, context);

    /// <summary>
    /// Computes the Pearson product-moment correlation coefficient between two data sets.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>The Pearson correlation coefficient in the range [-1, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PearsonCorrelation<T>(T[] x, T[] y)
        where T : INumber<T>, IRootFunctions<T>
        => PearsonCorrelation<T>((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y);

    /// <summary>
    /// Computes the Spearman rank correlation coefficient between two data sets.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>
    /// The Spearman rank correlation coefficient in the range [-1, 1]. Returns <see cref="INumberBase{T}.Zero"/>
    /// when either ranked data set has zero variance.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    /// <remarks>
    /// The Spearman coefficient assesses monotonic relationships by computing the Pearson
    /// correlation of the rank-transformed data. Tied values are assigned their average rank.
    /// </remarks>
    public static T SpearmanCorrelation<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
        where T : INumber<T>, IRootFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two data sets must be equal.");
        if (x.IsEmpty)
            throw new ArgumentException("The data sets cannot be empty.", nameof(x));

        T[] rankX = AssignAverageRanks(x);
        T[] rankY = AssignAverageRanks(y);
        return PearsonCorrelation<T>(rankX, rankY);
    }

    /// <summary>
    /// Computes the Spearman rank correlation coefficient between two data sets.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>The Spearman rank correlation coefficient in the range [-1, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    public static T SpearmanCorrelation<T>(T[] x, T[] y)
        where T : INumber<T>, IRootFunctions<T>
        => SpearmanCorrelation<T>((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y);

    /// <summary>
    /// Computes the Kendall rank correlation coefficient (tau-b) between two data sets.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>
    /// The Kendall tau-b coefficient in the range [-1, 1], which accounts for tied ranks.
    /// Returns <see cref="INumberBase{T}.Zero"/> when either tie-adjustment denominator is zero.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    /// <remarks>
    /// Tau-b measures the ordinal association between two data sets by comparing the number of
    /// concordant and discordant pairs while correcting for ties in either variable.
    /// </remarks>
    public static T KendallCorrelation<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
        where T : INumber<T>, IRootFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("The lengths of the two data sets must be equal.");
        if (x.IsEmpty)
            throw new ArgumentException("The data sets cannot be empty.", nameof(x));

        long concordant = 0;
        long discordant = 0;
        long tiesX = 0;
        long tiesY = 0;

        int n = x.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                int signX = SignOf(x[i], x[j]);
                int signY = SignOf(y[i], y[j]);

                if (signX == 0 && signY == 0)
                {
                    // Pair tied in both variables; contributes to neither tie correction term.
                }
                else if (signX == 0)
                {
                    tiesX++;
                }
                else if (signY == 0)
                {
                    tiesY++;
                }
                else if (signX == signY)
                {
                    concordant++;
                }
                else
                {
                    discordant++;
                }
            }
        }

        long totalPairs = (long)n * (n - 1) / 2;
        T numerator = T.CreateChecked(concordant - discordant);
        T denominator = T.Sqrt(T.CreateChecked(totalPairs - tiesX) * T.CreateChecked(totalPairs - tiesY));
        return denominator == T.Zero ? T.Zero : numerator / denominator;
    }

    /// <summary>
    /// Computes the Kendall rank correlation coefficient (tau-b) between two data sets.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="x">The first data set.</param>
    /// <param name="y">The second data set.</param>
    /// <returns>The Kendall tau-b coefficient in the range [-1, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when the two data sets have different lengths or are empty.</exception>
    public static T KendallCorrelation<T>(T[] x, T[] y)
        where T : INumber<T>, IRootFunctions<T>
        => KendallCorrelation<T>((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y);

    /// <summary>
    /// Assigns fractional (average) ranks to the elements of a data set, resolving ties by
    /// assigning the average of the ranks the tied elements would otherwise occupy.
    /// </summary>
    private static T[] AssignAverageRanks<T>(ReadOnlySpan<T> values)
        where T : INumber<T>
    {
        int n = values.Length;
        int[] order = new int[n];
        for (int i = 0; i < n; i++)
            order[i] = i;

        T[] buffer = values.ToArray();
        Array.Sort(order, (a, b) => buffer[a].CompareTo(buffer[b]));

        T[] ranks = new T[n];
        int index = 0;
        while (index < n)
        {
            int start = index;
            while (index < n - 1 && buffer[order[index + 1]] == buffer[order[start]])
                index++;

            // Ranks are 1-based; average rank of the tied group is (start + end + 2) / 2.
            T averageRank = T.CreateChecked(start + index + 2) / T.CreateChecked(2);
            for (int k = start; k <= index; k++)
                ranks[order[k]] = averageRank;

            index++;
        }

        return ranks;
    }

    /// <summary>
    /// Returns the sign of the comparison between two values: -1, 0, or 1.
    /// </summary>
    private static int SignOf<T>(T a, T b)
        where T : INumber<T>
        => a < b ? -1 : (a > b ? 1 : 0);

    /// <summary>
    /// Computes the Pearson correlation matrix for a collection of variables.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="variables">A jagged array where each inner array contains the observations for one variable. All variables must have the same number of observations.</param>
    /// <returns>
    /// A square symmetric matrix where the element at position [i, j] is the Pearson correlation
    /// coefficient between variable <c>i</c> and variable <c>j</c>. Diagonal elements are one.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="variables"/> is empty or its inner arrays have inconsistent lengths.</exception>
    public static T[,] CorrelationMatrix<T>(T[][] variables)
        where T : INumber<T>, IRootFunctions<T>
    {
        if (variables is null || variables.Length == 0)
            throw new ArgumentException("At least one variable must be provided.", nameof(variables));

        int variableCount = variables.Length;
        int observationCount = variables[0].Length;
        for (int i = 1; i < variableCount; i++)
        {
            if (variables[i].Length != observationCount)
                throw new ArgumentException("All variables must have the same number of observations.", nameof(variables));
        }

        T[,] matrix = new T[variableCount, variableCount];
        for (int i = 0; i < variableCount; i++)
        {
            matrix[i, i] = T.One;
            for (int j = i + 1; j < variableCount; j++)
            {
                T correlation = PearsonCorrelation<T>(variables[i], variables[j]);
                matrix[i, j] = correlation;
                matrix[j, i] = correlation;
            }
        }

        return matrix;
    }

    /// <summary>
    /// Computes the Pearson correlation matrix for the columns of a two-dimensional data set.
    /// </summary>
    /// <typeparam name="T">The numeric element type, which must implement <see cref="INumber{T}"/> and <see cref="IRootFunctions{T}"/>.</typeparam>
    /// <param name="data">A two-dimensional array where each row is an observation and each column is a variable.</param>
    /// <returns>
    /// A square symmetric matrix where the element at position [i, j] is the Pearson correlation
    /// coefficient between column <c>i</c> and column <c>j</c>. Diagonal elements are one.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="data"/> has no rows or no columns.</exception>
    public static T[,] CorrelationMatrix<T>(T[,] data)
        where T : INumber<T>, IRootFunctions<T>
    {
        int observationCount = data.GetLength(0);
        int variableCount = data.GetLength(1);
        if (observationCount == 0 || variableCount == 0)
            throw new ArgumentException("The data set must contain at least one row and one column.", nameof(data));

        T[][] variables = new T[variableCount][];
        for (int v = 0; v < variableCount; v++)
        {
            T[] column = new T[observationCount];
            for (int o = 0; o < observationCount; o++)
                column[o] = data[o, v];
            variables[v] = column;
        }

        return CorrelationMatrix<T>(variables);
    }
}
