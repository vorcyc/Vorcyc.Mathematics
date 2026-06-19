using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Regression pipeline: a chain of preprocessors and a regressor.
/// </summary>
public sealed class RegressionPipeline<T> : IBatchRegressor<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly List<IPreprocessor<T>> _preprocessors = [];
    private IRegressor<T>? _regressor;

    /// <summary>
    /// Gets or sets the execution context used for batch prediction.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>Adds a preprocessing step.</summary>
    public RegressionPipeline<T> AddPreprocessor(IPreprocessor<T> preprocessor)
    {
        _preprocessors.Add(preprocessor);
        return this;
    }

    /// <summary>Sets the regressor.</summary>
    public RegressionPipeline<T> SetRegressor(IRegressor<T> regressor)
    {
        _regressor = regressor;
        return this;
    }

    /// <inheritdoc />
    public void Fit(T[,] x, T[] y)
    {
        if (_regressor == null)
            throw new InvalidOperationException("The regressor must be set first.");

        var transformed = PipelineCore<T>.FitTransformChain(_preprocessors, x);
        _regressor.Fit(transformed, y);
    }

    /// <inheritdoc />
    public T Predict(T[] sample)
    {
        if (_regressor == null)
            throw new InvalidOperationException("The model has not been fitted yet.");

        return _regressor.Predict(PipelineCore<T>.TransformSample(_preprocessors, sample));
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<T> predictions)
    {
        if (_regressor == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        var transformed = PipelineCore<T>.TransformChain(_preprocessors, x);

        if (_regressor is IBatchRegressor<T> batchRegressor)
        {
            batchRegressor.PredictBatch(transformed, predictions[..rows]);
            return;
        }

        int cols = transformed.GetLength(1);
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, cols))
        {
            var buffer = new T[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    var sample = new T[cols];
                    for (int j = 0; j < cols; j++)
                        sample[j] = transformed[i, j];
                    buffer[i] = _regressor.Predict(sample);
                },
                workPerItem: cols);
            new ReadOnlySpan<T>(buffer).CopyTo(predictions[..rows]);
            return;
        }

        var reusable = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                reusable[j] = transformed[i, j];
            predictions[i] = _regressor.Predict(reusable);
        }
    }
}
