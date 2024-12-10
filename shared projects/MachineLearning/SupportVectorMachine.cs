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
public class SupportVectorMachine
{
    private double[] weights;
    private double bias;
    private double learningRate;
    private int epochs;

    /// <summary>
    /// 初始化 <see cref="SupportVectorMachine"/> 类的新实例。
    /// </summary>
    /// <param name="featureCount">输入数据的特征数量。</param>
    /// <param name="learningRate">训练算法的学习率。</param>
    /// <param name="epochs">训练的轮数。</param>
    public SupportVectorMachine(int featureCount, double learningRate = 0.01, int epochs = 1000)
    {
        weights = new double[featureCount];
        bias = 0.0;
        this.learningRate = learningRate;
        this.epochs = epochs;
    }

    /// <summary>
    /// 使用提供的训练数据训练SVM。
    /// </summary>
    /// <param name="inputs">输入数据，每个元素是一个特征值数组。</param>
    /// <param name="outputs">与输入数据对应的输出标签。</param>
    public void Train(double[][] inputs, int[] outputs)
    {
        for (int epoch = 0; epoch < epochs; epoch++)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                double[] input = inputs[i];
                int output = outputs[i];

                double prediction = PredictRaw(input);
                if (output * prediction <= 0)
                {
                    for (int j = 0; j < weights.Length; j++)
                    {
                        weights[j] += learningRate * output * input[j];
                    }
                    bias += learningRate * output;
                }
            }
        }
    }

    /// <summary>
    /// 预测给定输入数据的类别标签。
    /// </summary>
    /// <param name="input">输入数据，一个特征值数组。</param>
    /// <returns>预测的类别标签（1或-1）。</returns>
    public int Predict(double[] input)
    {
        return PredictRaw(input) >= 0 ? 1 : -1;
    }

    /// <summary>
    /// 计算给定输入数据的原始预测值。
    /// </summary>
    /// <param name="input">输入数据，一个特征值数组。</param>
    /// <returns>原始预测值。</returns>
    private double PredictRaw(double[] input)
    {
        double sum = bias;
        for (int i = 0; i < input.Length; i++)
        {
            sum += weights[i] * input[i];
        }
        return sum;
    }
}
