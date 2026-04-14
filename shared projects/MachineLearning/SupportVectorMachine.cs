using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 表示支持向量机的核函数类型。
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
/// 核感知机式支持向量机：线性模式使用权重向量，非线性模式使用对偶系数与支持向量。
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
    /// 初始化支持向量机。
    /// </summary>
    public SupportVectorMachine(
        int featureCount,
        TSelf? learningRate = null,
        int epochs = 1000,
        SupportVectorMachineKernelType kernelType = SupportVectorMachineKernelType.Linear,
        TSelf? gamma = null,
        int polynomialDegree = 3,
        TSelf? sigmoidAlpha = null,
        TSelf? sigmoidConstant = null)
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
    }

    /// <summary>
    /// 训练模型。标签应为 -1 或 1。
    /// </summary>
    public void Train(TSelf[][] inputs, int[] outputs)
    {
        if (inputs == null || outputs == null)
            throw new ArgumentException("训练数据不能为 null。");
        if (inputs.Length == 0 || inputs.Length != outputs.Length)
            throw new ArgumentException("样本数与标签数不匹配。");
        if (inputs[0].Length != _weights.Length)
            throw new ArgumentException("特征维度与模型不匹配。");

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
    /// 预测类别标签（1 或 -1）。
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
}
