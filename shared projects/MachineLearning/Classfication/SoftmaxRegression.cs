using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;
using Vorcyc.Mathematics.MachineLearning.Serialization;

namespace Vorcyc.Mathematics.MachineLearning.Classfication;

/// <summary>
/// Multi-class Softmax regression (multinomial logistic regression).
/// </summary>
public class SoftmaxRegression<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T[][]? _weights;
    private T[]? _biases;
    private int _numClasses;
    private bool _isFitted;

    /// <summary>
    /// Initializes a Softmax regression model.
    /// </summary>
    /// <param name="learningRate">Learning rate; when default, 0.05 is used.</param>
    /// <param name="epochs">Number of training epochs.</param>
    /// <param name="lambda">L2 regularization coefficient.</param>
    /// <param name="batchSize">Mini-batch size; &lt;=0 means full batch.</param>
    /// <param name="seed">Mini-batch shuffle seed; null uses non-deterministic shuffling.</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public SoftmaxRegression(
        T learningRate = default,
        int epochs = 1000,
        T lambda = default,
        int batchSize = 0,
        int? seed = null,
        ComputingContext? context = null)
    {
        LearningRate = learningRate.Equals(default) ? T.CreateChecked(0.05) : learningRate;
        Epochs = epochs;
        Lambda = lambda.Equals(default) ? T.Zero : lambda;
        BatchSize = batchSize;
        Seed = seed;
        Context = context;
    }

    /// <summary>Learning rate.</summary>
    public T LearningRate { get; }

    /// <summary>Number of training epochs.</summary>
    public int Epochs { get; }

    /// <summary>L2 regularization coefficient.</summary>
    public T Lambda { get; }

    /// <summary>Mini-batch size; &lt;=0 means full-batch gradient descent.</summary>
    public int BatchSize { get; }

    /// <summary>Mini-batch shuffle seed.</summary>
    public int? Seed { get; }

    /// <summary>
    /// Execution policy honored by batch prediction. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>Number of classes.</summary>
    public int NumClasses =>
        _isFitted ? _numClasses : throw new InvalidOperationException("The model has not been fitted yet.");

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>
    /// Fits the multi-class model. Labels must be non-negative integers numbered consecutively from 0.
    /// </summary>
    public void Fit(T[,] x, int[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("Training data cannot be empty.");
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        _numClasses = y.Max() + 1;
        if (y.Min() < 0)
            throw new ArgumentException("Labels must be non-negative integers.");

        _weights = new T[_numClasses][];
        for (int k = 0; k < _numClasses; k++)
            _weights[k] = new T[cols];
        _biases = new T[_numClasses];

        var probabilities = new T[_numClasses];
        var weightGrads = new T[_numClasses][];
        for (int k = 0; k < _numClasses; k++)
            weightGrads[k] = new T[cols];
        var biasGrads = new T[_numClasses];
        var rowScratch = new T[cols];

        int batchSize = BatchSize <= 0 ? rows : Math.Min(BatchSize, rows);

        for (int epoch = 0; epoch < Epochs; epoch++)
        {
            var order = DataSplit.CreateShuffledIndices(rows, Seed.HasValue ? Seed.Value + epoch : null);

            for (int batchStart = 0; batchStart < rows; batchStart += batchSize)
            {
                int batchCount = Math.Min(batchSize, rows - batchStart);
                for (int k = 0; k < _numClasses; k++)
                    Array.Clear(weightGrads[k], 0, cols);
                Array.Clear(biasGrads, 0, _numClasses);

                for (int b = 0; b < batchCount; b++)
                {
                    int i = order[batchStart + b];
                    CopyRow(x, i, cols, rowScratch);
                    ComputeProbabilities(rowScratch, probabilities);
                    int label = y[i];

                    for (int k = 0; k < _numClasses; k++)
                    {
                        T target = k == label ? T.One : T.Zero;
                        T error = probabilities[k] - target;
                        NumericKernels.AddScaled(weightGrads[k], rowScratch, error);
                        biasGrads[k] += error;
                    }
                }

                T invN = T.One / T.CreateChecked(batchCount);
                for (int k = 0; k < _numClasses; k++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        weightGrads[k][j] = weightGrads[k][j] * invN + Lambda * _weights[k][j];
                        _weights[k][j] -= LearningRate * weightGrads[k][j];
                    }
                    _biases[k] -= LearningRate * (biasGrads[k] * invN);
                }
            }
        }

        _isFitted = true;
    }

    /// <summary>
    /// Predicts the class label.
    /// </summary>
    public int Predict(T[] x)
    {
        var probs = PredictProbabilities(x);
        return ClassificationMath.ArgMax(probs);
    }

    /// <inheritdoc />
    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (cols != _weights![0].Length)
            throw new ArgumentException("Feature dimension does not match the model.");
        if (predictions.Length < rows)
            throw new ArgumentException("The predictions span is too short.", nameof(predictions));

        int numClasses = _numClasses;
        if (ComputingContextExecution.UseParallelIndexed(Context, rows, (long)cols * numClasses))
        {
            var buffer = new int[rows];
            ComputingContextExecution.ForEach(
                Context,
                0,
                rows,
                i =>
                {
                    var localProbabilities = new T[numClasses];
                    ComputeProbabilitiesFromRow(x, i, localProbabilities);
                    buffer[i] = ClassificationMath.ArgMax(localProbabilities);
                },
                workPerItem: (long)cols * numClasses);
            new ReadOnlySpan<int>(buffer).CopyTo(predictions);
            return;
        }

        var probabilities = new T[_numClasses];
        for (int i = 0; i < rows; i++)
        {
            ComputeProbabilitiesFromRow(x, i, probabilities);
            predictions[i] = ClassificationMath.ArgMax(probabilities);
        }
    }

    /// <summary>
    /// Returns the per-class probabilities.
    /// </summary>
    public T[] PredictProbabilities(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
        if (x == null || x.Length != _weights![0].Length)
            throw new ArgumentException("Feature dimension does not match the model.", nameof(x));

        var probabilities = new T[_numClasses];
        ComputeProbabilities(x, probabilities);
        return probabilities;
    }

    /// <summary>
    /// Exports a model snapshot (double precision).
    /// </summary>
    public SoftmaxRegressionSnapshot CaptureSnapshot()
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");

        var weights = new double[_numClasses][];
        for (int k = 0; k < _numClasses; k++)
            weights[k] = _weights![k].Select(v => double.CreateChecked(v)).ToArray();

        return new SoftmaxRegressionSnapshot
        {
            NumClasses = _numClasses,
            Weights = weights,
            Biases = _biases!.Select(double.CreateChecked).ToArray(),
            LearningRate = double.CreateChecked(LearningRate),
            Epochs = Epochs,
            Lambda = double.CreateChecked(Lambda)
        };
    }

    /// <summary>
    /// Restores model parameters from a snapshot (for inference, without retraining).
    /// </summary>
    public void RestoreFromSnapshot(SoftmaxRegressionSnapshot snapshot)
    {
        _numClasses = snapshot.NumClasses;
        _weights = new T[_numClasses][];
        for (int k = 0; k < _numClasses; k++)
            _weights[k] = snapshot.Weights[k].Select(v => T.CreateChecked(v)).ToArray();
        _biases = snapshot.Biases.Select(v => T.CreateChecked(v)).ToArray();
        _isFitted = true;
    }

    private static void CopyRow<TNum>(TNum[,] matrix, int row, int cols, Span<TNum> destination)
        where TNum : struct
    {
        for (int j = 0; j < cols; j++)
            destination[j] = matrix[row, j];
    }

    private void ComputeProbabilities(ReadOnlySpan<T> x, Span<T> destination)
    {
        for (int k = 0; k < _numClasses; k++)
            destination[k] = NumericKernels.Dot(_weights![k], x) + _biases![k];
        StableProbabilities.Softmax(destination);
    }

    private void ComputeProbabilitiesFromRow(T[,] x, int row, Span<T> destination)
    {
        for (int k = 0; k < _numClasses; k++)
            destination[k] = NumericKernels.DotRow(x, row, _weights![k]) + _biases![k];
        StableProbabilities.Softmax(destination);
    }
}
