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
/// 一个简单的线性支持向量机（SVM）实现。
/// </summary>
public class SupportVectorMachine<TSelf>
    where TSelf : struct, IFloatingPointIeee754<TSelf>
{
    private TSelf[] _weights;
    private TSelf _bias;
    private TSelf _learningRate;
    private int _epochs;

    /// <summary>
    /// 初始化 <see cref="SupportVectorMachine{TSelf}"/> 类的新实例。
    /// </summary>
    /// <param name="featureCount">输入数据的特征数量。</param>
    /// <param name="learningRate">训练算法的学习率。为 null 时默认为 0.01 </param>
    /// <param name="epochs">训练的轮数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SupportVectorMachine(int featureCount, TSelf? learningRate = null, int epochs = 1000)
    {
        _weights = new TSelf[featureCount];
        _bias = TSelf.Zero;
        this._learningRate = learningRate is null ? TSelf.CreateChecked(0.01) : learningRate.Value;
        this._epochs = epochs;
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
    public int Predict(TSelf[] input)
    {
        return PredictRaw(input) >= TSelf.Zero ? 1 : -1;
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
            sum += _weights[i] * input[i];
        }
        return sum;
    }
}
