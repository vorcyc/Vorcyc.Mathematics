using System.Numerics;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// Simple (univariate) linear regression utility.
/// </summary>
/// <typeparam name="T">Floating-point type that must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
/// <remarks>
/// This class implements univariate linear regression and fits the straight-line model <c>y = slope * x + intercept</c>
/// using ordinary least squares. It provides the ability to fit the model from data points or arrays, predict y or x values,
/// and access the slope, intercept, and R².
/// <para>Optimizations include:</para>
/// <list type="bullet">
///   <item><description>Improved performance via the SIMD-optimized <see cref="Basic.Sum{T}(Span{T})"/> method.</description></item>
///   <item><description>Numerical stability checks to prevent division by zero.</description></item>
///   <item><description>R² computation added for model evaluation.</description></item>
/// </list>
/// </remarks>
/// <example>
/// The following example shows how to use the <see cref="SimpleLinearRegression{T}"/> class:
/// <code>
/// var data = new Point&lt;double&gt;[]
/// {
///     new Point&lt;double&gt;(1.0, 2.0),
///     new Point&lt;double&gt;(2.0, 3.0),
///     new Point&lt;double&gt;(3.0, 5.0),
///     new Point&lt;double&gt;(4.0, 4.0),
///     new Point&lt;double&gt;(5.0, 6.0)
/// };
/// var regression = new SimpleLinearRegression&lt;double&gt;();
/// var (slope, intercept) = regression.Fit(data);
/// Console.WriteLine($"Slope: {slope}, Intercept: {intercept}");
/// var x = 6.0;
/// var y = regression.GetY(x);
/// Console.WriteLine($"For x = {x}, predicted y = {y}");
/// Console.WriteLine($"R²: {regression.RSquared}");
/// </code>
/// </example>
public class SimpleLinearRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private T _slope;         // Slope
    private T _intercept;     // Intercept
    private bool _isFitted;   // Whether the model has been fitted
    private T _rSquared;      // Coefficient of determination R²

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleLinearRegression{T}"/> class.
    /// </summary>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public SimpleLinearRegression(ComputingContext? context = null)
    {
        _isFitted = false;
        Context = context;
    }

    /// <summary>
    /// Execution policy honored by <see cref="PredictBatch(Span{T}, ComputingContext)"/>. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Gets the slope of the linear regression model.
    /// </summary>
    public T Slope => _isFitted ? _slope : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>
    /// Gets the intercept of the linear regression model.
    /// </summary>
    public T Intercept => _isFitted ? _intercept : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>
    /// Gets the coefficient of determination R² of the model, indicating how well the model explains the data.
    /// </summary>
    public T RSquared => _isFitted ? _rSquared : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>
    /// Gets the machine learning task type.
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// Fits the linear regression model from an array of data points and returns the slope and intercept.
    /// </summary>
    /// <param name="data">An array containing the data points.</param>
    /// <returns>A tuple containing the slope and intercept.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="data"/> is null or contains fewer than 2 data points.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a division by zero occurs during the computation.</exception>
    public (T slope, T intercept) Fit(Point<T>[] data)
    {
        if (data == null || data.Length < 2)
            throw new ArgumentException("The data point array cannot be null and must contain at least 2 points.", nameof(data));

        T sumX = T.Zero, sumY = T.Zero, sumXY = T.Zero, sumX2 = T.Zero;
        T n = T.CreateChecked(data.Length);

        foreach (var point in data)
        {
            sumX += point.X;
            sumY += point.Y;
            sumXY += point.X * point.Y;
            sumX2 += point.X * point.X;
        }

        var result = ComputeCoefficients(sumX, sumY, sumXY, sumX2, n);
        _rSquared = ComputeRSquared(data.AsSpan(), sumY / n); // Compute R² after fitting
        return result;
    }

    /// <summary>
    /// Fits the linear regression model from the independent and dependent variable arrays and returns the slope and intercept.
    /// </summary>
    /// <param name="x">The array of the independent variable.</param>
    /// <param name="y">The array of the dependent variable.</param>
    /// <returns>A tuple containing the slope and intercept.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="x"/> or <paramref name="y"/> is empty, the lengths do not match, or there are fewer than 2 elements.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a division by zero occurs during the computation.</exception>
    public (T slope, T intercept) Fit(Span<T> x, Span<T> y)
    {
        if (x.IsEmpty || y.IsEmpty || x.Length != y.Length || x.Length < 2)
            throw new ArgumentException("The independent and dependent variable arrays cannot be empty, must have equal length, and must contain at least 2 elements.", nameof(x));

        T sumX = x.Sum();
        T sumY = y.Sum();
        T sumXY = T.Zero, sumX2 = T.Zero;
        T n = T.CreateChecked(x.Length);

        for (int i = 0; i < x.Length; i++)
        {
            sumXY += x[i] * y[i];
            sumX2 += x[i] * x[i];
        }

        var result = ComputeCoefficients(sumX, sumY, sumXY, sumX2, n);
        _rSquared = ComputeRSquared(x, y, sumY / n); // Compute R² after fitting
        return result;
    }

    /// <summary>
    /// Computes the x value for a given y value.
    /// </summary>
    /// <param name="y">The value of the dependent variable.</param>
    /// <returns>The predicted x value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the model has not been fitted or the slope is zero.</exception>
    public T GetX(T y)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (_slope == T.Zero)
            throw new InvalidOperationException("The slope is zero; cannot compute the x value.");

        return (y - _intercept) / _slope;
    }

    /// <summary>
    /// Computes the y value for a given x value.
    /// </summary>
    /// <param name="x">The value of the independent variable.</param>
    /// <returns>The predicted y value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the model has not been fitted.</exception>
    public T GetY(T x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");

        return _slope * x + _intercept;
    }

    /// <summary>
    /// Computes the y values for a batch of x values, honoring <see cref="Context"/> for SIMD/parallel execution.
    /// </summary>
    /// <param name="x">The independent variable values to evaluate.</param>
    /// <param name="context">Optional execution policy overriding <see cref="Context"/>; when null, <see cref="Context"/>, the ambient scope, or the default context is used.</param>
    /// <returns>An array of predicted y values, one per element of <paramref name="x"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the model has not been fitted.</exception>
    public T[] PredictBatch(Span<T> x, ComputingContext? context = null)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");

        int count = x.Length;
        var predictions = new T[count];
        if (count == 0)
            return predictions;

        T slope = _slope;
        T intercept = _intercept;
        var effectiveContext = context ?? Context;

        if (ComputingContextExecution.UseParallelIndexed(effectiveContext, count, 1))
        {
            var inputs = x.ToArray();
            ComputingContextExecution.ForEach(
                effectiveContext,
                0,
                count,
                i => predictions[i] = slope * inputs[i] + intercept,
                workPerItem: 1);
            return predictions;
        }

        for (int i = 0; i < count; i++)
            predictions[i] = slope * x[i] + intercept;
        return predictions;
    }

    /// <summary>
    /// Computes the regression coefficients and updates the model state.
    /// </summary>
    /// <param name="sumX">The sum of x.</param>
    /// <param name="sumY">The sum of y.</param>
    /// <param name="sumXY">The sum of x*y.</param>
    /// <param name="sumX2">The sum of x².</param>
    /// <param name="n">The number of data points.</param>
    /// <returns>A tuple of the slope and intercept.</returns>
    private (T slope, T intercept) ComputeCoefficients(T sumX, T sumY, T sumXY, T sumX2, T n)
    {
        T denominator = n * sumX2 - sumX * sumX;
        if (T.Abs(denominator) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("The data points are too collinear or the variance is too small to compute the regression coefficients.");

        T avgX = sumX / n;
        T avgY = sumY / n;

        _slope = (n * sumXY - sumX * sumY) / denominator;
        _intercept = avgY - _slope * avgX;
        _isFitted = true; // Set to true to ensure subsequent calls are valid

        return (_slope, _intercept);
    }

    /// <summary>
    /// Computes the R² value (based on <see cref="Point{T}"/> data).
    /// </summary>
    private T ComputeRSquared(ReadOnlySpan<Point<T>> data, T avgY)
    {
        T ssTot = T.Zero, ssRes = T.Zero;
        for (int i = 0; i < data.Length; i++)
        {
            T yPred = _slope * data[i].X + _intercept; // Compute directly using the coefficients
            T yActual = data[i].Y;
            ssTot += (yActual - avgY) * (yActual - avgY);
            ssRes += (yActual - yPred) * (yActual - yPred);
        }
        return ssTot != T.Zero ? T.One - (ssRes / ssTot) : T.Zero;
    }

    /// <summary>
    /// Computes the R² value (based on the x and y arrays).
    /// </summary>
    private T ComputeRSquared(Span<T> x, Span<T> y, T avgY)
    {
        T ssTot = T.Zero, ssRes = T.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            T yPred = _slope * x[i] + _intercept; // Compute directly using the coefficients
            T yActual = y[i];
            ssTot += (yActual - avgY) * (yActual - avgY);
            ssRes += (yActual - yPred) * (yActual - yPred);
        }
        return ssTot != T.Zero ? T.One - (ssRes / ssTot) : T.Zero;
    }
}