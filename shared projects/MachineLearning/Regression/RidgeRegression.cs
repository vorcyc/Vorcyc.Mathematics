using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// Ridge regression supporting L2-regularized least squares on a one-dimensional polynomial basis.
/// </summary>
public class RidgeRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T _lambda;
    private readonly int _degree;
    private T[]? _coefficients;
    private T _rSquared;

    /// <summary>
    /// Initializes a ridge regression model.
    /// </summary>
    /// <param name="lambda">Regularization parameter.</param>
    /// <param name="degree">Polynomial degree; defaults to 1 (linear).</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public RidgeRegression(T lambda, int degree = 1, ComputingContext? context = null)
    {
        if (lambda < T.Zero)
            throw new ArgumentException("The regularization parameter must be non-negative.", nameof(lambda));
        if (degree < 0)
            throw new ArgumentException("The polynomial degree must be non-negative.", nameof(degree));

        _lambda = lambda;
        _degree = degree;
        Context = context;
    }

    /// <summary>
    /// Execution policy honored by <see cref="PredictBatch(Span{T}, ComputingContext)"/>. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>Fitted coefficients.</summary>
    public IReadOnlyList<T> Coefficients =>
        _coefficients ?? throw new InvalidOperationException("The model has not been fitted yet.");

    /// <summary>Coefficient of determination R².</summary>
    public T RSquared => _coefficients == null
        ? throw new InvalidOperationException("The model has not been fitted yet.")
        : _rSquared;

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// Fits the ridge regression model.
    /// </summary>
    public void Fit(Span<T> x, Span<T> y)
    {
        if (x.Length == 0 || y.Length == 0)
            throw new ArgumentException("The input arrays cannot be empty.");
        if (x.Length != y.Length)
            throw new ArgumentException("The independent and dependent variable arrays must have the same length.");
        if (x.Length <= _degree)
            throw new ArgumentException("The number of data points must be greater than the polynomial degree.");

        var designMatrix = RegressionMathHelper.BuildVandermonde(x, _degree);
        _coefficients = RegressionMathHelper.SolveRidgeLeastSquares(
            designMatrix, y, _lambda, regularizeIntercept: false);
        var xValues = x.ToArray();
        _rSquared = RegressionMathHelper.ComputeRSquared(y, i => Predict(xValues[i]));
    }

    /// <summary>
    /// Predicts the output for the given input.
    /// </summary>
    public T Predict(T x)
    {
        if (_coefficients == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        return RegressionMathHelper.PredictVandermonde(x, _coefficients, _degree);
    }

    /// <summary>
    /// Predicts the outputs for a batch of inputs, honoring <see cref="Context"/> for SIMD/parallel execution.
    /// </summary>
    /// <param name="x">The independent variable values to evaluate.</param>
    /// <param name="context">Optional execution policy overriding <see cref="Context"/>; when null, <see cref="Context"/>, the ambient scope, or the default context is used.</param>
    /// <returns>An array of predicted outputs, one per element of <paramref name="x"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the model has not been fitted yet.</exception>
    public T[] PredictBatch(Span<T> x, ComputingContext? context = null)
    {
        if (_coefficients == null)
            throw new InvalidOperationException("The model has not been fitted yet.");

        int count = x.Length;
        var predictions = new T[count];
        if (count == 0)
            return predictions;

        var coefficients = _coefficients;
        int degree = _degree;
        var effectiveContext = context ?? Context;

        if (ComputingContextExecution.UseParallelIndexed(effectiveContext, count, degree + 1))
        {
            var inputs = x.ToArray();
            ComputingContextExecution.ForEach(
                effectiveContext,
                0,
                count,
                i => predictions[i] = RegressionMathHelper.PredictVandermonde(inputs[i], coefficients, degree),
                workPerItem: degree + 1);
            return predictions;
        }

        for (int i = 0; i < count; i++)
            predictions[i] = RegressionMathHelper.PredictVandermonde(x[i], coefficients, degree);
        return predictions;
    }
}
