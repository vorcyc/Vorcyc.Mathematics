using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Classfication;

/// <summary>
/// One-hidden-layer network with tanh units and a softmax output, trained by full-batch cross-entropy.
/// </summary>
public sealed class NeuralNetworkClassifier<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly int _hiddenUnits;
    private readonly int _epochs;
    private readonly double _learningRate;
    private readonly int _seed;
    private int[] _labels = [];
    private double[,] _inputWeights = new double[0, 0];
    private double[] _inputBias = [];
    private double[,] _outputWeights = new double[0, 0];
    private double[] _outputBias = [];
    private bool _isFitted;

    public NeuralNetworkClassifier(int hiddenUnits = 16, int epochs = 400, T learningRate = default, int seed = 1, ComputingContext? context = null)
    {
        if (hiddenUnits < 1)
            throw new ArgumentOutOfRangeException(nameof(hiddenUnits));
        if (epochs < 1)
            throw new ArgumentOutOfRangeException(nameof(epochs));
        _hiddenUnits = hiddenUnits;
        _epochs = epochs;
        _learningRate = learningRate.Equals(default) ? 0.05 : double.CreateChecked(learningRate);
        _seed = seed;
        Context = context;
    }

    public ComputingContext? Context { get; set; }

    public MachineLearningTask Task => MachineLearningTask.Classification;

    public void Fit(T[,] x, int[] y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        int rows = x.GetLength(0);
        int features = x.GetLength(1);
        if (rows == 0 || features == 0 || rows != y.Length)
            throw new ArgumentException("Training data is empty or the labels do not match.");

        _labels = y.Distinct().OrderBy(label => label).ToArray();
        var labelIndex = new Dictionary<int, int>();
        for (int i = 0; i < _labels.Length; i++)
            labelIndex[_labels[i]] = i;

        var inputs = new double[rows, features];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < features; j++)
                inputs[i, j] = double.CreateChecked(x[i, j]);

        var random = new Random(_seed);
        _inputWeights = Initialize(random, _hiddenUnits, features);
        _inputBias = new double[_hiddenUnits];
        _outputWeights = Initialize(random, _labels.Length, _hiddenUnits);
        _outputBias = new double[_labels.Length];

        var hidden = new double[_hiddenUnits];
        var output = new double[_labels.Length];
        var outputGradient = new double[_labels.Length];
        var hiddenGradient = new double[_hiddenUnits];

        for (int epoch = 0; epoch < _epochs; epoch++)
        {
            var dInput = new double[_hiddenUnits, features];
            var dInputBias = new double[_hiddenUnits];
            var dOutput = new double[_labels.Length, _hiddenUnits];
            var dOutputBias = new double[_labels.Length];

            for (int row = 0; row < rows; row++)
            {
                Forward(inputs, row, hidden, output);
                for (int c = 0; c < _labels.Length; c++)
                    outputGradient[c] = output[c] - (labelIndex[y[row]] == c ? 1 : 0);

                for (int c = 0; c < _labels.Length; c++)
                {
                    dOutputBias[c] += outputGradient[c];
                    for (int h = 0; h < _hiddenUnits; h++)
                        dOutput[c, h] += outputGradient[c] * hidden[h];
                }

                for (int h = 0; h < _hiddenUnits; h++)
                {
                    double back = 0;
                    for (int c = 0; c < _labels.Length; c++)
                        back += _outputWeights[c, h] * outputGradient[c];
                    hiddenGradient[h] = back * (1 - hidden[h] * hidden[h]);
                    dInputBias[h] += hiddenGradient[h];
                    for (int f = 0; f < features; f++)
                        dInput[h, f] += hiddenGradient[h] * inputs[row, f];
                }
            }

            double rate = _learningRate / rows;
            for (int h = 0; h < _hiddenUnits; h++)
            {
                _inputBias[h] -= rate * dInputBias[h];
                for (int f = 0; f < features; f++)
                    _inputWeights[h, f] -= rate * dInput[h, f];
            }

            for (int c = 0; c < _labels.Length; c++)
            {
                _outputBias[c] -= rate * dOutputBias[c];
                for (int h = 0; h < _hiddenUnits; h++)
                    _outputWeights[c, h] -= rate * dOutput[c, h];
            }
        }

        _isFitted = true;
    }

    public int Predict(T[] sample)
    {
        EnsureFitted();
        if (sample.Length != _inputWeights.GetLength(1))
            throw new ArgumentException("Feature dimension does not match the model.");

        var hidden = new double[_hiddenUnits];
        var output = new double[_labels.Length];
        var row = new double[1, sample.Length];
        for (int j = 0; j < sample.Length; j++)
            row[0, j] = double.CreateChecked(sample[j]);
        Forward(row, 0, hidden, output);
        int best = 0;
        for (int i = 1; i < output.Length; i++)
        {
            if (output[i] > output[best])
                best = i;
        }

        return _labels[best];
    }

    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        EnsureFitted();
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The prediction buffer is shorter than the sample count.");
        for (int i = 0; i < rows; i++)
            predictions[i] = Predict(Array2DHelpers.GetRow(x, i));
    }

    void Forward(double[,] inputs, int row, double[] hidden, double[] output)
    {
        int features = inputs.GetLength(1);
        for (int h = 0; h < hidden.Length; h++)
        {
            double sum = _inputBias[h];
            for (int f = 0; f < features; f++)
                sum += _inputWeights[h, f] * inputs[row, f];
            hidden[h] = Math.Tanh(sum);
        }

        double max = double.NegativeInfinity;
        for (int c = 0; c < output.Length; c++)
        {
            double sum = _outputBias[c];
            for (int h = 0; h < hidden.Length; h++)
                sum += _outputWeights[c, h] * hidden[h];
            output[c] = sum;
            if (sum > max)
                max = sum;
        }

        double normalizer = 0;
        for (int c = 0; c < output.Length; c++)
        {
            output[c] = Math.Exp(output[c] - max);
            normalizer += output[c];
        }

        for (int c = 0; c < output.Length; c++)
            output[c] /= normalizer;
    }

    static double[,] Initialize(Random random, int rows, int cols)
    {
        double limit = Math.Sqrt(6.0 / (rows + cols));
        var weights = new double[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                weights[i, j] = (random.NextDouble() * 2 - 1) * limit;
        return weights;
    }

    void EnsureFitted()
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
    }
}
