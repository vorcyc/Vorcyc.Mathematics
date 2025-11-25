using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning;

//double[][] inputs =
//{
//                new double[] { 0, 0 },
//                new double[] { 1, 0 },
//                new double[] { 0, 1 },
//                new double[] { 1, 1 }
//            };

//int[] outputs =
//{
//                -1, // Class 1
//                -1, // Class 1
//                -1, // Class 1
//                 1  // Class 2
//            };

//// 创建并训练支持向量机
//var svm = new SupportVectorMachine(featureCount: 2);
//svm.Train(inputs, outputs);

//// 预测
//double[] newInput = { 0.8, 0.8 };
//int prediction = svm.Predict(newInput);

//Console.WriteLine($"预测结果: {prediction}");


/// <summary>
/// 表示支持向量机的核函数类型。
/// </summary>
public enum SupportVectorMachineKernelType
{
    /// <summary>
    /// 线性核函数。
    /// </summary>
    Linear,
    /// <summary>
    /// 多项式核函数。
    /// </summary>
    Polynomial,
    /// <summary>
    /// 高斯核函数。
    /// </summary>
    Gaussian,
    /// <summary>
    /// 径向基函数（RBF）核函数。等效于 <see cref="SupportVectorMachineKernelType.Gaussian"/>。
    /// </summary>
    RBF,
    /// <summary>
    /// 点乘核函数。等效于 <see cref="SupportVectorMachineKernelType.Linear"/>。
    /// </summary>
    DotProduct,
    /// <summary>
    /// Sigmoid 核函数。
    /// </summary>
    Sigmoid
}

/// <summary>
/// 一个简单的线性支持向量机（SVM）实现，支持选择核函数。
/// </summary>
public class SupportVectorMachine<TSelf> : IMachineLearning
    where TSelf : struct, IFloatingPointIeee754<TSelf>
{
    private TSelf[] _weights;
    private TSelf _bias;
    private TSelf _learningRate;
    private int _epochs;
    private Func<TSelf[], TSelf[], TSelf> _kernelFunction;

    public MachineLearningTask Task => MachineLearningTask.Classification | MachineLearningTask.Regression;

    /// <summary>
    /// 初始化 <see cref="SupportVectorMachine{TSelf}"/> 类的新实例。
    /// </summary>
    /// <param name="featureCount">输入数据的特征数量。</param>
    /// <param name="learningRate">训练算法的学习率。为 null 时默认为 0.01 </param>
    /// <param name="epochs">训练的轮数。</param>
    /// <param name="kernelType">核函数类型。默认为线性核函数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SupportVectorMachine(int featureCount, TSelf? learningRate = null, int epochs = 1000, SupportVectorMachineKernelType kernelType = SupportVectorMachineKernelType.Linear)
    {
        _weights = new TSelf[featureCount];
        _bias = TSelf.Zero;
        this._learningRate = learningRate is null ? TSelf.CreateChecked(0.01) : learningRate.Value;
        this._epochs = epochs;
        this._kernelFunction = GetKernelFunction(kernelType);
    }

    /// <summary>
    /// 使用提供的训练数据训练SVM。
    /// </summary>
    /// <param name="inputs">输入数据，每个元素是一个特征值数组。</param>
    /// <param name="outputs">与输入数据对应的输出标签。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Train(TSelf[][] inputs, int[] outputs)
    {
        for (int epoch = 0; epoch < _epochs; epoch++)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                TSelf[] input = inputs[i];
                TSelf output = TSelf.CreateChecked(outputs[i]);

                TSelf prediction = PredictRaw(input);
                if (output * prediction <= TSelf.Zero)
                {
                    for (int j = 0; j < _weights.Length; j++)
                    {
                        _weights[j] += _learningRate * output * input[j];
                    }
                    _bias += _learningRate * output;
                }
            }
        }
    }

    /// <summary>
    /// 预测给定输入数据的类别标签。
    /// </summary>
    /// <param name="input">输入数据，一个特征值数组。</param>
    /// <returns>预测的类别标签（1或-1）。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Predict(Span<TSelf> input)
    {
        return PredictRaw(input.ToArray()) >= TSelf.Zero ? 1 : -1;
    }

    /// <summary>
    /// 计算给定输入数据的原始预测值。
    /// </summary>
    /// <param name="input">输入数据，一个特征值数组。</param>
    /// <returns>原始预测值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TSelf PredictRaw(TSelf[] input)
    {
        TSelf sum = _bias;
        for (int i = 0; i < input.Length; i++)
        {
            sum += _weights[i] * _kernelFunction(_weights, input);
        }
        return sum;
    }

    /// <summary>
    /// 获取指定类型的核函数。
    /// </summary>
    /// <param name="kernelType">核函数类型。</param>
    /// <returns>核函数的委托。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Func<TSelf[], TSelf[], TSelf> GetKernelFunction(SupportVectorMachineKernelType kernelType) =>
        kernelType switch
        {
            SupportVectorMachineKernelType.Linear => LinearKernel,
            SupportVectorMachineKernelType.Polynomial => PolynomialKernel,
            SupportVectorMachineKernelType.Gaussian => GaussianKernel,
            SupportVectorMachineKernelType.RBF => GaussianKernel, // RBF 和 Gaussian 核函数相同
            SupportVectorMachineKernelType.DotProduct => LinearKernel, // 就是线性的
            SupportVectorMachineKernelType.Sigmoid => SigmoidKernel,
            _ => throw new ArgumentException("Unsupported kernel type", nameof(kernelType)),
        };

    /// <summary>
    /// 线性核函数。
    /// </summary>
    /// <param name="x">第一个输入向量。</param>
    /// <param name="y">第二个输入向量。</param>
    /// <returns>核函数的计算结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TSelf LinearKernel(TSelf[] x, TSelf[] y)
    {
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            sum += x[i] * y[i];
        }
        return sum;
    }

    /// <summary>
    /// 多项式核函数。
    /// </summary>
    /// <param name="x">第一个输入向量。</param>
    /// <param name="y">第二个输入向量。</param>
    /// <returns>核函数的计算结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TSelf PolynomialKernel(TSelf[] x, TSelf[] y)
    {
        TSelf dotProduct = LinearKernel(x, y);
        TSelf degree = TSelf.CreateChecked(3); // 多项式的度数，可以根据需要调整
        return TSelf.Pow(dotProduct + TSelf.One, degree);
    }

    /// <summary>
    /// 高斯核函数（RBF）。
    /// </summary>
    /// <param name="x">第一个输入向量。</param>
    /// <param name="y">第二个输入向量。</param>
    /// <returns>核函数的计算结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TSelf GaussianKernel(TSelf[] x, TSelf[] y)
    {
        TSelf sum = TSelf.Zero;
        for (int i = 0; i < x.Length; i++)
        {
            TSelf diff = x[i] - y[i];
            sum += diff * diff;
        }
        TSelf sigma = TSelf.CreateChecked(1.0); // 高斯核的带宽参数，可以根据需要调整
        return TSelf.Exp(-sum / (TSelf.CreateChecked(2.0) * sigma * sigma));
    }

    /// <summary>
    /// Sigmoid 核函数。
    /// </summary>
    /// <param name="x">第一个输入向量。</param>
    /// <param name="y">第二个输入向量。</param>
    /// <returns>核函数的计算结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TSelf SigmoidKernel(TSelf[] x, TSelf[] y)
    {
        TSelf dotProduct = LinearKernel(x, y);
        TSelf alpha = TSelf.CreateChecked(0.01); // Sigmoid 核的参数，可以根据需要调整
        TSelf constant = TSelf.CreateChecked(1.0);
        return TSelf.Tanh(alpha * dotProduct + constant);
    }
}