using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Kernel function type for a support vector machine.
/// </summary>
public enum SupportVectorMachineKernelType
{
    Linear,
    Polynomial,
    Gaussian,
    RBF,
    DotProduct,
    Sigmoid
}

/// <summary>
/// Kernel-perceptron-style support vector machine: the linear mode uses a weight vector, while non-linear modes use dual coefficients and support vectors.
/// </summary>
public class SupportVectorMachine<TSelf> : IMachineLearning
    where TSelf : struct, IFloatingPointIeee754<TSelf>
{
    private readonly TSelf _learningRate;
    private readonly int _epochs;
    private readonly SupportVectorMachineKernelType _kernelType;
    private readonly TSelf _gamma;
    private readonly int _polynomialDegree;
    private readonly TSelf _sigmoidAlpha;
    private readonly TSelf _sigmoidConstant;
    private readonly bool _isLinearKernel;

    private TSelf[] _weights = [];
    private TSelf _bias;
    private TSelf[][] _trainingInputs = [];
    private int[] _trainingLabels = [];
    private TSelf[] _alphas = [];

    public MachineLearningTask Task => MachineLearningTask.Classification | MachineLearningTask.Regression;

    /// <summary>
    /// Initializes a support vector machine.
    /// </summary>
    public SupportVectorMachine(
        int featureCount,
        TSelf? learningRate = null,
        int epochs = 1000,
        SupportVectorMachineKernelType kernelType = SupportVectorMachineKernelType.Linear,
        TSelf? gamma = null,
        int polynomialDegree = 3,
        TSelf? sigmoidAlpha = null,
        TSelf? sigmoidConstant = null,
        ComputingContext? context = null)
    {
        _learningRate = learningRate ?? TSelf.CreateChecked(0.01);
        _epochs = epochs;
        _kernelType = kernelType;
        _gamma = gamma ?? TSelf.CreateChecked(1.0);
        _polynomialDegree = polynomialDegree;
        _sigmoidAlpha = sigmoidAlpha ?? TSelf.CreateChecked(0.01);
        _sigmoidConstant = sigmoidConstant ?? TSelf.CreateChecked(1.0);
        _isLinearKernel = kernelType is SupportVectorMachineKernelType.Linear
            or SupportVectorMachineKernelType.DotProduct;
        _weights = new TSelf[featureCount];
        Context = context;
    }

    /// <summary>
    /// Execution policy context. When null, the ambient <see cref="ComputingScope"/>
    /// and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Trains the model. Labels must be -1 or 1.
    /// </summary>
    public void Train(TSelf[][] inputs, int[] outputs)
    {
        if (inputs == null || outputs == null)
            throw new ArgumentException("Training data cannot be null.");
        if (inputs.Length == 0 || inputs.Length != outputs.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");
        if (inputs[0].Length != _weights.Length)
            throw new ArgumentException("Feature dimension does not match the model.");

        _trainingInputs = inputs;
        _trainingLabels = (int[])outputs.Clone();
        _alphas = new TSelf[inputs.Length];
        _bias = TSelf.Zero;
        Array.Clear(_weights, 0, _weights.Length);

        for (int epoch = 0; epoch < _epochs; epoch++)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                TSelf label = TSelf.CreateChecked(outputs[i]);
                TSelf prediction = PredictRaw(inputs[i]);
                if (label * prediction > TSelf.Zero)
                    continue;

                if (_isLinearKernel)
                {
                    for (int j = 0; j < _weights.Length; j++)
                        _weights[j] += _learningRate * label * inputs[i][j];
                }
                else
                {
                    _alphas[i] += TSelf.One;
                }

                _bias += _isLinearKernel ? _learningRate * label : label;
            }
        }
    }

    /// <summary>
    /// Predicts the class label (1 or -1).
    /// </summary>
    public int Predict(Span<TSelf> input)
    {
        var buffer = input.ToArray();
        return PredictRaw(buffer) >= TSelf.Zero ? 1 : -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TSelf PredictRaw(TSelf[] input)
    {
        if (_isLinearKernel)
        {
            TSelf sum = _bias;
            for (int i = 0; i < input.Length; i++)
                sum += _weights[i] * input[i];
            return sum;
        }

        TSelf total = _bias;
        for (int i = 0; i < _trainingInputs.Length; i++)
        {
            if (_alphas[i] == TSelf.Zero)
                continue;
            TSelf label = TSelf.CreateChecked(_trainingLabels[i]);
            total += _alphas[i] * label * Kernel(_trainingInputs[i], input);
        }
        return total;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TSelf Kernel(TSelf[] x, TSelf[] y) =>
        _kernelType switch
        {
            SupportVectorMachineKernelType.Linear or SupportVectorMachineKernelType.DotProduct => DotProduct(x, y),
            SupportVectorMachineKernelType.Polynomial => PolynomialKernel(x, y),
            SupportVectorMachineKernelType.Gaussian or SupportVectorMachineKernelType.RBF => GaussianKernel(x, y),
            SupportVectorMachineKernelType.Sigmoid => SigmoidKernel(x, y),
            _ => throw new ArgumentOutOfRangeException(nameof(_kernelType))
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TSelf DotProduct(TSelf[] x, TSelf[] y)
    {
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
            sum += x[i] * y[i];
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TSelf PolynomialKernel(TSelf[] x, TSelf[] y)
    {
        TSelf dot = DotProduct(x, y);
        return TSelf.Pow(_gamma * dot + TSelf.One, TSelf.CreateChecked(_polynomialDegree));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TSelf GaussianKernel(TSelf[] x, TSelf[] y)
    {
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            TSelf diff = x[i] - y[i];
            sum += diff * diff;
        }
        return TSelf.Exp(-_gamma * sum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TSelf SigmoidKernel(TSelf[] x, TSelf[] y) =>
        TSelf.Tanh(_sigmoidAlpha * DotProduct(x, y) + _sigmoidConstant);

    /// <summary>
    /// Exports trained parameters and support vectors (double precision).
    /// </summary>
    public Serialization.SupportVectorMachineSnapshot CaptureSnapshot()
    {
        var trainingInputs = new double[_trainingInputs.Length][];
        for (int i = 0; i < _trainingInputs.Length; i++)
            trainingInputs[i] = _trainingInputs[i].Select(v => double.CreateChecked(v)).ToArray();

        return new Serialization.SupportVectorMachineSnapshot
        {
            FeatureCount = _weights.Length,
            LearningRate = double.CreateChecked(_learningRate),
            Epochs = _epochs,
            KernelType = _kernelType,
            Gamma = double.CreateChecked(_gamma),
            PolynomialDegree = _polynomialDegree,
            SigmoidAlpha = double.CreateChecked(_sigmoidAlpha),
            SigmoidConstant = double.CreateChecked(_sigmoidConstant),
            Weights = _weights.Select(v => double.CreateChecked(v)).ToArray(),
            Bias = double.CreateChecked(_bias),
            TrainingInputs = trainingInputs,
            TrainingLabels = _trainingLabels.Length == 0 ? [] : (int[])_trainingLabels.Clone(),
            Alphas = _alphas.Length == 0
                ? []
                : _alphas.Select(v => double.CreateChecked(v)).ToArray()
        };
    }

    /// <summary>
    /// Restores trained parameters from a snapshot (for inference).
    /// </summary>
    public void RestoreFromSnapshot(Serialization.SupportVectorMachineSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));
        if (snapshot.FeatureCount != _weights.Length)
            throw new ArgumentException(
                $"Snapshot feature count ({snapshot.FeatureCount}) does not match this model ({_weights.Length}).");
        if (snapshot.KernelType != _kernelType)
            throw new ArgumentException("Snapshot kernel type does not match this model.");

        _weights = snapshot.Weights.Select(TSelf.CreateChecked).ToArray();
        _bias = TSelf.CreateChecked(snapshot.Bias);
        _trainingInputs = new TSelf[snapshot.TrainingInputs.Length][];
        for (int i = 0; i < snapshot.TrainingInputs.Length; i++)
            _trainingInputs[i] = snapshot.TrainingInputs[i].Select(TSelf.CreateChecked).ToArray();
        _trainingLabels = snapshot.TrainingLabels.Length == 0
            ? []
            : (int[])snapshot.TrainingLabels.Clone();
        _alphas = snapshot.Alphas.Length == 0
            ? []
            : snapshot.Alphas.Select(TSelf.CreateChecked).ToArray();
    }
}
