using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Classification pipeline: a chain of preprocessors plus a classifier.
/// </summary>
public sealed class ClassificationPipeline<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly List<IPreprocessor<T>> _preprocessors = [];
    private IClassifier<T>? _classifier;

    /// <summary>
    /// Gets or sets the execution context used for batch prediction.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>Adds a preprocessing step.</summary>
    public ClassificationPipeline<T> AddPreprocessor(IPreprocessor<T> preprocessor)
    {
        _preprocessors.Add(preprocessor);
        return this;
    }

    /// <summary>Sets the classifier.</summary>
    public ClassificationPipeline<T> SetClassifier(IClassifier<T> classifier)
    {
        _classifier = classifier;
        return this;
    }

    /// <inheritdoc />
    public void Fit(T[,] x, int[] y)
    {
        if (_classifier == null)
            throw new InvalidOperationException("The classifier must be set first.");

        var transformed = PipelineCore<T>.FitTransformChain(_preprocessors, x);
        _classifier.Fit(transformed, y);
    }

    /// <inheritdoc />
    public int Predict(T[] sample)
    {
        if (_classifier == null)
            throw new InvalidOperationException("The model has not been fitted yet.");

        return _classifier.Predict(PipelineCore<T>.TransformSample(_preprocessors, sample));
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        if (_classifier == null)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        var transformed = PipelineCore<T>.TransformChain(_preprocessors, x);

        if (_classifier is IBatchClassifier<T> batchClassifier)
        {
            batchClassifier.PredictBatch(transformed, predictions[..rows]);
            return;
        }

        int cols = transformed.GetLength(1);
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, cols))
        {
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    var sample = new T[cols];
                    for (int j = 0; j < cols; j++)
                        sample[j] = transformed[i, j];
                    buffer[i] = _classifier.Predict(sample);
                },
                workPerItem: cols);
            new ReadOnlySpan<int>(buffer).CopyTo(predictions[..rows]);
            return;
        }

        var reusable = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                reusable[j] = transformed[i, j];
            predictions[i] = _classifier.Predict(reusable);
        }
    }
}
