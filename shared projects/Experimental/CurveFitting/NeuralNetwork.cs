using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

public delegate void TrainingProgressHandler(int epoch, int totalEpochs, double averageError);

/// <summary>
/// 表示一个用于曲线拟合的多层感知器神经网络，支持时间序列数据。
/// </summary>
internal class NeuralNetwork
{
    private readonly int inputSize;      // 输入维度（时间步数 * 特征数）
    private readonly int hiddenSize;     // 隐藏层节点数
    private readonly int outputSize;     // 输出层节点数
    private readonly double[][] weights1; // 输入到隐藏层权重
    private readonly double[][] weights2; // 隐藏到输出层权重
    private readonly double[] bias1;     // 隐藏层偏置
    private readonly double[] bias2;     // 输出层偏置
    private readonly Random random;      // 随机数生成器

    /// <summary>
    /// 初始化神经网络。
    /// </summary>
    /// <param name="timeSteps">输入的时间步数。</param>
    /// <param name="features">每个时间步的特征数。</param>
    /// <param name="hidden">隐藏层节点数。</param>
    /// <param name="output">输出层节点数。</param>
    public NeuralNetwork(int timeSteps, int features, int hidden, int output)
    {
        inputSize = timeSteps * features;
        hiddenSize = hidden;
        outputSize = output;
        random = new Random();

        weights1 = InitializeWeights(inputSize, hiddenSize);
        weights2 = InitializeWeights(hiddenSize, outputSize);
        bias1 = InitializeBias(hiddenSize);
        bias2 = InitializeBias(outputSize);
    }

    /// <summary>
    /// 初始化权重矩阵，使用随机值。
    /// </summary>
    private double[][] InitializeWeights(int rows, int cols)
    {
        double[][] matrix = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            matrix[i] = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                matrix[i][j] = random.NextDouble() * 0.2 - 0.1;
            }
        }
        return matrix;
    }

    /// <summary>
    /// 初始化偏置向量，使用随机值。
    /// </summary>
    private double[] InitializeBias(int size)
    {
        double[] bias = new double[size];
        for (int i = 0; i < size; i++)
        {
            bias[i] = random.NextDouble() * 0.2 - 0.1;
        }
        return bias;
    }

    /// <summary>
    /// Sigmoid激活函数。
    /// </summary>
    private double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));

    /// <summary>
    /// Sigmoid函数的导数。
    /// </summary>
    private double SigmoidDerivative(double x) => x * (1 - x);

    /// <summary>
    /// 将二维输入展平为一维数组。
    /// </summary>
    private double[] FlattenInput(double[,] input)
    {
        int timeSteps = input.GetLength(0);
        int features = input.GetLength(1);
        double[] flattened = new double[timeSteps * features];
        for (int t = 0; t < timeSteps; t++)
        {
            for (int f = 0; f < features; f++)
            {
                flattened[t * features + f] = input[t, f];
            }
        }
        return flattened;
    }

    /// <summary>
    /// 执行前向传播，预测输出。
    /// </summary>
    /// <param name="inputs">输入数据，形状为[时间步, 特征]。</param>
    /// <returns>网络的输出。</returns>
    /// <exception cref="ArgumentException">输入维度不匹配时抛出。</exception>
    public double[] Forward(double[,] inputs)
    {
        double[] flattenedInputs = FlattenInput(inputs);
        if (flattenedInputs.Length != inputSize)
            throw new ArgumentException($"Input size ({flattenedInputs.Length}) does not match network input size ({inputSize})");

        double[] hidden = ComputeHidden(flattenedInputs);
        return ComputeOutput(hidden);
    }

    /// <summary>
    /// 训练神经网络。
    /// </summary>
    /// <param name="inputs">输入数据，形状为[样本数, 时间步, 特征]。</param>
    /// <param name="targets">目标数据，形状为[样本数, 输出维度]。</param>
    /// <param name="learningRate">学习率。</param>
    /// <param name="epochs">训练轮数。</param>
    /// <param name="progressCallback">训练进度回调函数，可为空。</param>
    /// <exception cref="ArgumentException">输入和目标数据长度不匹配时抛出。</exception>
    public void Train(double[,,] inputs, double[][] targets, double learningRate, int epochs,
        TrainingProgressHandler progressCallback = null)
    {
        if (inputs.GetLength(0) != targets.Length)
            throw new ArgumentException("Inputs and targets must have same length");

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            ShuffleData(inputs, targets);
            double totalError = 0;

            for (int i = 0; i < inputs.GetLength(0); i++)
            {
                double[,] sample = GetSample(inputs, i);
                double[] flattenedInputs = FlattenInput(sample);

                // 前向传播
                double[] hidden = ComputeHidden(flattenedInputs);
                double[] outputs = ComputeOutput(hidden);

                // 计算误差
                totalError += ComputeError(outputs, targets[i]);

                // 反向传播
                double[] outputDeltas = ComputeOutputDeltas(outputs, targets[i]);
                double[] hiddenDeltas = ComputeHiddenDeltas(hidden, outputDeltas);

                // 更新权重和偏置
                UpdateWeightsAndBiases(flattenedInputs, hidden, outputDeltas, hiddenDeltas, learningRate);
            }

            if (progressCallback != null)
            {
                double avgError = totalError / inputs.GetLength(0);
                progressCallback(epoch + 1, epochs, avgError);
            }
        }
    }

    /// <summary>
    /// 计算隐藏层输出。
    /// </summary>
    private double[] ComputeHidden(double[] flattenedInputs)
    {
        double[] hidden = new double[hiddenSize];
        for (int h = 0; h < hiddenSize; h++)
        {
            double sum = bias1[h];
            for (int i = 0; i < inputSize; i++)
            {
                sum += flattenedInputs[i] * weights1[i][h];
            }
            hidden[h] = Sigmoid(sum);
        }
        return hidden;
    }

    /// <summary>
    /// 计算输出层输出。
    /// </summary>
    private double[] ComputeOutput(double[] hidden)
    {
        double[] outputs = new double[outputSize];
        for (int o = 0; o < outputSize; o++)
        {
            double sum = bias2[o];
            for (int h = 0; h < hiddenSize; h++)
            {
                sum += hidden[h] * weights2[h][o];
            }
            outputs[o] = Sigmoid(sum);
        }
        return outputs;
    }

    /// <summary>
    /// 计算样本误差。
    /// </summary>
    private double ComputeError(double[] outputs, double[] target)
    {
        double sampleError = 0;
        for (int o = 0; o < outputSize; o++)
        {
            sampleError += Math.Pow(target[o] - outputs[o], 2) * 0.5;
        }
        return sampleError;
    }

    /// <summary>
    /// 计算输出层误差项。
    /// </summary>
    private double[] ComputeOutputDeltas(double[] outputs, double[] target)
    {
        double[] outputDeltas = new double[outputSize];
        for (int o = 0; o < outputSize; o++)
        {
            double error = target[o] - outputs[o];
            outputDeltas[o] = error * SigmoidDerivative(outputs[o]);
        }
        return outputDeltas;
    }

    /// <summary>
    /// 计算隐藏层误差项。
    /// </summary>
    private double[] ComputeHiddenDeltas(double[] hidden, double[] outputDeltas)
    {
        double[] hiddenDeltas = new double[hiddenSize];
        for (int h = 0; h < hiddenSize; h++)
        {
            double error = 0;
            for (int o = 0; o < outputSize; o++)
            {
                error += outputDeltas[o] * weights2[h][o];
            }
            hiddenDeltas[h] = error * SigmoidDerivative(hidden[h]);
        }
        return hiddenDeltas;
    }

    /// <summary>
    /// 更新权重和偏置。
    /// </summary>
    private void UpdateWeightsAndBiases(double[] flattenedInputs, double[] hidden,
        double[] outputDeltas, double[] hiddenDeltas, double learningRate)
    {
        // 更新隐藏到输出层的权重和偏置
        for (int h = 0; h < hiddenSize; h++)
        {
            for (int o = 0; o < outputSize; o++)
            {
                weights2[h][o] += learningRate * outputDeltas[o] * hidden[h];
            }
        }
        for (int o = 0; o < outputSize; o++)
        {
            bias2[o] += learningRate * outputDeltas[o];
        }

        // 更新输入到隐藏层的权重和偏置
        for (int j = 0; j < inputSize; j++)
        {
            for (int h = 0; h < hiddenSize; h++)
            {
                weights1[j][h] += learningRate * hiddenDeltas[h] * flattenedInputs[j];
            }
        }
        for (int h = 0; h < hiddenSize; h++)
        {
            bias1[h] += learningRate * hiddenDeltas[h];
        }
    }

    /// <summary>
    /// 打乱输入和目标数据。
    /// </summary>
    private void ShuffleData(double[,,] inputs, double[][] targets)
    {
        int n = inputs.GetLength(0);
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            for (int t = 0; t < inputs.GetLength(1); t++)
            {
                for (int f = 0; f < inputs.GetLength(2); f++)
                {
                    double temp = inputs[i, t, f];
                    inputs[i, t, f] = inputs[j, t, f];
                    inputs[j, t, f] = temp;
                }
            }
            double[] tempTarget = targets[i];
            targets[i] = targets[j];
            targets[j] = tempTarget;
        }
    }

    /// <summary>
    /// 从输入数据中提取单个样本。
    /// </summary>
    /// <param name="inputs">输入数据。</param>
    /// <param name="index">样本索引。</param>
    /// <returns>单个样本数据。</returns>
    public double[,] GetSample(double[,,] inputs, int index)
    {
        int timeSteps = inputs.GetLength(1);
        int features = inputs.GetLength(2);
        double[,] sample = new double[timeSteps, features];
        for (int t = 0; t < timeSteps; t++)
        {
            for (int f = 0; f < features; f++)
            {
                sample[t, f] = inputs[index, t, f];
            }
        }
        return sample;
    }
}





public delegate void TrainingProgressHandler<T>(int epoch, int totalEpochs, T averageError)
    where T : unmanaged, IFloatingPointIeee754<T>;



/// <summary>
/// 表示一个用于曲线拟合的多层感知器神经网络，支持时间序列数据。
/// </summary>
public class NeuralNetwork_Sequential<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly int inputSize;      // 输入维度（时间步数 * 特征数）
    private readonly int hiddenSize;     // 隐藏层节点数
    private readonly int outputSize;     // 输出层节点数
    private readonly T[][] weights1; // 输入到隐藏层权重
    private readonly T[][] weights2; // 隐藏到输出层权重
    private readonly T[] bias1;     // 隐藏层偏置
    private readonly T[] bias2;     // 输出层偏置
    private readonly Random random;      // 随机数生成器

    /// <summary>
    /// 初始化神经网络。
    /// </summary>
    /// <param name="timeSteps">输入的时间步数。</param>
    /// <param name="features">每个时间步的特征数。</param>
    /// <param name="hidden">隐藏层节点数。</param>
    /// <param name="output">输出层节点数。</param>
    public NeuralNetwork_Sequential(int timeSteps, int features, int hidden, int output)
    {
        inputSize = timeSteps * features;
        hiddenSize = hidden;
        outputSize = output;
        random = new Random();

        weights1 = InitializeWeights(inputSize, hiddenSize);
        weights2 = InitializeWeights(hiddenSize, outputSize);
        bias1 = InitializeBias(hiddenSize);
        bias2 = InitializeBias(outputSize);
    }

    /// <summary>
    /// 初始化权重矩阵，使用随机值。
    /// </summary>
    private T[][] InitializeWeights(int rows, int cols)
    {
        T[][] matrix = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            matrix[i] = new T[cols];
            for (int j = 0; j < cols; j++)
            {
                matrix[i][j] = T.CreateSaturating(random.NextDouble() * 0.2 - 0.1);
            }
        }
        return matrix;
    }

    /// <summary>
    /// 初始化偏置向量，使用随机值。
    /// </summary>
    private T[] InitializeBias(int size)
    {
        T[] bias = new T[size];
        for (int i = 0; i < size; i++)
        {
            bias[i] = T.CreateSaturating(random.NextDouble() * 0.2 - 0.1);
        }
        return bias;
    }

    /// <summary>
    /// Sigmoid激活函数。
    /// </summary>
    private T Sigmoid(T x) => T.One / (T.One + T.Exp(-x));

    /// <summary>
    /// Sigmoid函数的导数。
    /// </summary>
    private T SigmoidDerivative(T x) => x * (T.One - x);

    /// <summary>
    /// 将二维输入展平为一维数组。
    /// </summary>
    private T[] FlattenInput(T[,] input)
    {
        int timeSteps = input.GetLength(0);
        int features = input.GetLength(1);
        T[] flattened = new T[timeSteps * features];
        for (int t = 0; t < timeSteps; t++)
        {
            for (int f = 0; f < features; f++)
            {
                flattened[t * features + f] = input[t, f];
            }
        }
        return flattened;
    }

    /// <summary>
    /// 执行前向传播，预测输出。
    /// </summary>
    /// <param name="inputs">输入数据，形状为[时间步, 特征]。</param>
    /// <returns>网络的输出。</returns>
    /// <exception cref="ArgumentException">输入维度不匹配时抛出。</exception>
    public T[] Forward(T[,] inputs)
    {
        T[] flattenedInputs = FlattenInput(inputs);
        if (flattenedInputs.Length != inputSize)
            throw new ArgumentException($"Input size ({flattenedInputs.Length}) does not match network input size ({inputSize})");

        T[] hidden = ComputeHidden(flattenedInputs);
        return ComputeOutput(hidden);
    }

    /// <summary>
    /// 训练神经网络。
    /// </summary>
    /// <param name="inputs">输入数据，形状为[样本数, 时间步, 特征]。</param>
    /// <param name="targets">目标数据，形状为[样本数, 输出维度]。</param>
    /// <param name="learningRate">学习率。</param>
    /// <param name="epochs">训练轮数。</param>
    /// <param name="progressCallback">训练进度回调函数，可为空。</param>
    /// <exception cref="ArgumentException">输入和目标数据长度不匹配时抛出。</exception>
    public void Train(T[,,] inputs, T[][] targets, T learningRate, int epochs,
        TrainingProgressHandler<T>? progressCallback = null)
    {
        if (inputs.GetLength(0) != targets.Length)
            throw new ArgumentException("Inputs and targets must have same length");

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            ShuffleData(inputs, targets);
            T totalError = T.Zero;

            for (int i = 0; i < inputs.GetLength(0); i++)
            {
                T[,] sample = GetSample(inputs, i);
                T[] flattenedInputs = FlattenInput(sample);

                // 前向传播
                T[] hidden = ComputeHidden(flattenedInputs);
                T[] outputs = ComputeOutput(hidden);

                // 计算误差
                totalError += ComputeError(outputs, targets[i]);

                // 反向传播
                T[] outputDeltas = ComputeOutputDeltas(outputs, targets[i]);
                T[] hiddenDeltas = ComputeHiddenDeltas(hidden, outputDeltas);

                // 更新权重和偏置
                UpdateWeightsAndBiases(flattenedInputs, hidden, outputDeltas, hiddenDeltas, learningRate);
            }

            if (progressCallback is not null)
            {
                T avgError = totalError / T.CreateChecked(inputs.GetLength(0));
                progressCallback(epoch + 1, epochs, avgError);
            }
        }
    }

    /// <summary>
    /// 计算隐藏层输出。
    /// </summary>
    private T[] ComputeHidden(T[] flattenedInputs)
    {
        T[] hidden = new T[hiddenSize];
        for (int h = 0; h < hiddenSize; h++)
        {
            T sum = bias1[h];
            for (int i = 0; i < inputSize; i++)
            {
                sum += flattenedInputs[i] * weights1[i][h];
            }
            hidden[h] = Sigmoid(sum);
        }
        return hidden;
    }

    /// <summary>
    /// 计算输出层输出。
    /// </summary>
    private T[] ComputeOutput(T[] hidden)
    {
        T[] outputs = new T[outputSize];
        for (int o = 0; o < outputSize; o++)
        {
            T sum = bias2[o];
            for (int h = 0; h < hiddenSize; h++)
            {
                sum += hidden[h] * weights2[h][o];
            }
            outputs[o] = Sigmoid(sum);
        }
        return outputs;
    }

    /// <summary>
    /// 计算样本误差。
    /// </summary>
    private T ComputeError(T[] outputs, T[] target)
    {
        T sampleError = T.Zero;
        for (int o = 0; o < outputSize; o++)
        {
            sampleError += T.Pow(target[o] - outputs[o], T.CreateChecked(2)) * T.CreateChecked(0.5);
        }
        return sampleError;
    }

    /// <summary>
    /// 计算输出层误差项。
    /// </summary>
    private T[] ComputeOutputDeltas(T[] outputs, T[] target)
    {
        T[] outputDeltas = new T[outputSize];
        for (int o = 0; o < outputSize; o++)
        {
            T error = target[o] - outputs[o];
            outputDeltas[o] = error * SigmoidDerivative(outputs[o]);
        }
        return outputDeltas;
    }

    /// <summary>
    /// 计算隐藏层误差项。
    /// </summary>
    private T[] ComputeHiddenDeltas(T[] hidden, T[] outputDeltas)
    {
        T[] hiddenDeltas = new T[hiddenSize];
        for (int h = 0; h < hiddenSize; h++)
        {
            T error = T.Zero;
            for (int o = 0; o < outputSize; o++)
            {
                error += outputDeltas[o] * weights2[h][o];
            }
            hiddenDeltas[h] = error * SigmoidDerivative(hidden[h]);
        }
        return hiddenDeltas;
    }

    /// <summary>
    /// 更新权重和偏置。
    /// </summary>
    private void UpdateWeightsAndBiases(T[] flattenedInputs, T[] hidden,
        T[] outputDeltas, T[] hiddenDeltas, T learningRate)
    {
        // 更新隐藏到输出层的权重和偏置
        for (int h = 0; h < hiddenSize; h++)
        {
            for (int o = 0; o < outputSize; o++)
            {
                weights2[h][o] += learningRate * outputDeltas[o] * hidden[h];
            }
        }
        for (int o = 0; o < outputSize; o++)
        {
            bias2[o] += learningRate * outputDeltas[o];
        }

        // 更新输入到隐藏层的权重和偏置
        for (int j = 0; j < inputSize; j++)
        {
            for (int h = 0; h < hiddenSize; h++)
            {
                weights1[j][h] += learningRate * hiddenDeltas[h] * flattenedInputs[j];
            }
        }
        for (int h = 0; h < hiddenSize; h++)
        {
            bias1[h] += learningRate * hiddenDeltas[h];
        }
    }

    /// <summary>
    /// 打乱输入和目标数据。
    /// </summary>
    private void ShuffleData(T[,,] inputs, T[][] targets)
    {
        int n = inputs.GetLength(0);
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            for (int t = 0; t < inputs.GetLength(1); t++)
            {
                for (int f = 0; f < inputs.GetLength(2); f++)
                {
                    T temp = inputs[i, t, f];
                    inputs[i, t, f] = inputs[j, t, f];
                    inputs[j, t, f] = temp;
                }
            }
            T[] tempTarget = targets[i];
            targets[i] = targets[j];
            targets[j] = tempTarget;
        }
    }

    /// <summary>
    /// 从输入数据中提取单个样本。
    /// </summary>
    /// <param name="inputs">输入数据。</param>
    /// <param name="index">样本索引。</param>
    /// <returns>单个样本数据。</returns>
    public T[,] GetSample(T[,,] inputs, int index)
    {
        int timeSteps = inputs.GetLength(1);
        int features = inputs.GetLength(2);
        T[,] sample = new T[timeSteps, features];
        for (int t = 0; t < timeSteps; t++)
        {
            for (int f = 0; f < features; f++)
            {
                sample[t, f] = inputs[index, t, f];
            }
        }
        return sample;
    }

    // 获取所有参数
    internal T[] GetParameters()
    {
        var parameters = new List<T>();
        foreach (var row in weights1) parameters.AddRange(row);
        foreach (var row in weights2) parameters.AddRange(row);
        parameters.AddRange(bias1);
        parameters.AddRange(bias2);
        return parameters.ToArray();
    }

}/// <summary>



/// <summary>
/// 表示一个用于曲线拟合的多层感知器神经网络，支持时间序列数据。
/// </summary>
public class NeuralNetwork_Parallel<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    private readonly int inputSize;      // 输入维度（时间步数 * 特征数）
    private readonly int hiddenSize;     // 隐藏层节点数
    private readonly int outputSize;     // 输出层节点数
    private readonly T[][] weights1; // 输入到隐藏层权重
    private readonly T[][] weights2; // 隐藏到输出层权重
    private readonly T[] bias1;     // 隐藏层偏置
    private readonly T[] bias2;     // 输出层偏置
    private readonly Random random;      // 随机数生成器

    /// <summary>
    /// 初始化神经网络。
    /// </summary>
    /// <param name="timeSteps">输入的时间步数。</param>
    /// <param name="features">每个时间步的特征数。</param>
    /// <param name="hidden">隐藏层节点数。</param>
    /// <param name="output">输出层节点数。</param>
    public NeuralNetwork_Parallel(int timeSteps, int features, int hidden, int output)
    {
        inputSize = timeSteps * features;
        hiddenSize = hidden;
        outputSize = output;
        random = new Random();

        weights1 = InitializeWeights(inputSize, hiddenSize);
        weights2 = InitializeWeights(hiddenSize, outputSize);
        bias1 = InitializeBias(hiddenSize);
        bias2 = InitializeBias(outputSize);
    }

    /// <summary>
    /// 初始化权重矩阵，使用随机值。
    /// </summary>
    private T[][] InitializeWeights(int rows, int cols)
    {
        T[][] matrix = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            matrix[i] = new T[cols];
            for (int j = 0; j < cols; j++)
            {
                matrix[i][j] = T.CreateSaturating(random.NextDouble() * 0.2 - 0.1);
            }
        }
        return matrix;
    }

    /// <summary>
    /// 初始化偏置向量，使用随机值。
    /// </summary>
    private T[] InitializeBias(int size)
    {
        T[] bias = new T[size];
        for (int i = 0; i < size; i++)
        {
            bias[i] = T.CreateSaturating(random.NextDouble() * 0.2 - 0.1);
        }
        return bias;
    }

    /// <summary>
    /// Sigmoid激活函数。
    /// </summary>
    private T Sigmoid(T x) => T.One / (T.One + T.Exp(-x));

    /// <summary>
    /// Sigmoid函数的导数。
    /// </summary>
    private T SigmoidDerivative(T x) => x * (T.One - x);

    /// <summary>
    /// 将二维输入展平为一维数组。
    /// </summary>
    private T[] FlattenInput(T[,] input)
    {
        int timeSteps = input.GetLength(0);
        int features = input.GetLength(1);
        T[] flattened = new T[timeSteps * features];
        for (int t = 0; t < timeSteps; t++)
        {
            for (int f = 0; f < features; f++)
            {
                flattened[t * features + f] = input[t, f];
            }
        }
        return flattened;
    }

    /// <summary>
    /// 执行前向传播，预测输出。
    /// </summary>
    /// <param name="inputs">输入数据，形状为[时间步, 特征]。</param>
    /// <returns>网络的输出。</returns>
    /// <exception cref="ArgumentException">输入维度不匹配时抛出。</exception>
    public T[] Forward(T[,] inputs)
    {
        T[] flattenedInputs = FlattenInput(inputs);
        if (flattenedInputs.Length != inputSize)
            throw new ArgumentException($"Input size ({flattenedInputs.Length}) does not match network input size ({inputSize})");

        T[] hidden = ComputeHidden(flattenedInputs);
        return ComputeOutput(hidden);
    }

    /// <summary>
    /// 训练神经网络。
    /// </summary>
    /// <param name="inputs">输入数据，形状为[样本数, 时间步, 特征]。</param>
    /// <param name="targets">目标数据，形状为[样本数, 输出维度]。</param>
    /// <param name="learningRate">学习率。</param>
    /// <param name="epochs">训练轮数。</param>
    /// <param name="progressCallback">训练进度回调函数，可为空。</param>
    /// <exception cref="ArgumentException">输入和目标数据长度不匹配时抛出。</exception>
    public void Train(T[,,] inputs, T[][] targets, T learningRate, int epochs,
        TrainingProgressHandler<T>? progressCallback = null)
    {
        if (inputs.GetLength(0) != targets.Length)
            throw new ArgumentException("Inputs and targets must have same length");

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            ShuffleData(inputs, targets);
            T totalError = T.Zero;

            Parallel.For(0, inputs.GetLength(0), i =>
            {
                T[,] sample = GetSample(inputs, i);
                T[] flattenedInputs = FlattenInput(sample);

                // 前向传播
                T[] hidden = ComputeHidden(flattenedInputs);
                T[] outputs = ComputeOutput(hidden);

                // 计算误差
                T sampleError = ComputeError(outputs, targets[i]);
                lock (this)
                {
                    totalError += sampleError;
                }

                // 反向传播
                T[] outputDeltas = ComputeOutputDeltas(outputs, targets[i]);
                T[] hiddenDeltas = ComputeHiddenDeltas(hidden, outputDeltas);

                // 更新权重和偏置
                UpdateWeightsAndBiases(flattenedInputs, hidden, outputDeltas, hiddenDeltas, learningRate);
            });

            if (progressCallback is not null)
            {
                T avgError = totalError / T.CreateChecked(inputs.GetLength(0));
                progressCallback(epoch + 1, epochs, avgError);
            }
        }
    }

    /// <summary>
    /// 计算隐藏层输出。
    /// </summary>
    private T[] ComputeHidden(T[] flattenedInputs)
    {
        T[] hidden = new T[hiddenSize];
        Parallel.For(0, hiddenSize, h =>
        {
            T sum = bias1[h];
            for (int i = 0; i < inputSize; i++)
            {
                sum += flattenedInputs[i] * weights1[i][h];
            }
            hidden[h] = Sigmoid(sum);
        });
        return hidden;
    }

    /// <summary>
    /// 计算输出层输出。
    /// </summary>
    private T[] ComputeOutput(T[] hidden)
    {
        T[] outputs = new T[outputSize];
        Parallel.For(0, outputSize, o =>
        {
            T sum = bias2[o];
            for (int h = 0; h < hiddenSize; h++)
            {
                sum += hidden[h] * weights2[h][o];
            }
            outputs[o] = Sigmoid(sum);
        });
        return outputs;
    }

    /// <summary>
    /// 计算样本误差。
    /// </summary>
    private T ComputeError(T[] outputs, T[] target)
    {
        T sampleError = T.Zero;
        for (int o = 0; o < outputSize; o++)
        {
            sampleError += T.Pow(target[o] - outputs[o], T.CreateChecked(2)) * T.CreateChecked(0.5);
        }
        return sampleError;
    }

    /// <summary>
    /// 计算输出层误差项。
    /// </summary>
    private T[] ComputeOutputDeltas(T[] outputs, T[] target)
    {
        T[] outputDeltas = new T[outputSize];
        for (int o = 0; o < outputSize; o++)
        {
            T error = target[o] - outputs[o];
            outputDeltas[o] = error * SigmoidDerivative(outputs[o]);
        }
        return outputDeltas;
    }

    /// <summary>
    /// 计算隐藏层误差项。
    /// </summary>
    private T[] ComputeHiddenDeltas(T[] hidden, T[] outputDeltas)
    {
        T[] hiddenDeltas = new T[hiddenSize];
        for (int h = 0; h < hiddenSize; h++)
        {
            T error = T.Zero;
            for (int o = 0; o < outputSize; o++)
            {
                error += outputDeltas[o] * weights2[h][o];
            }
            hiddenDeltas[h] = error * SigmoidDerivative(hidden[h]);
        }
        return hiddenDeltas;
    }

    /// <summary>
    /// 更新权重和偏置。
    /// </summary>
    private void UpdateWeightsAndBiases(T[] flattenedInputs, T[] hidden,
        T[] outputDeltas, T[] hiddenDeltas, T learningRate)
    {
        // 更新隐藏到输出层的权重和偏置
        Parallel.For(0, hiddenSize, h =>
        {
            for (int o = 0; o < outputSize; o++)
            {
                weights2[h][o] += learningRate * outputDeltas[o] * hidden[h];
            }
        });
        Parallel.For(0, outputSize, o =>
        {
            bias2[o] += learningRate * outputDeltas[o];
        });

        // 更新输入到隐藏层的权重和偏置
        Parallel.For(0, inputSize, j =>
        {
            for (int h = 0; h < hiddenSize; h++)
            {
                weights1[j][h] += learningRate * hiddenDeltas[h] * flattenedInputs[j];
            }
        });
        Parallel.For(0, hiddenSize, h =>
        {
            bias1[h] += learningRate * hiddenDeltas[h];
        });
    }

    /// <summary>
    /// 打乱输入和目标数据。
    /// </summary>
    private void ShuffleData(T[,,] inputs, T[][] targets)
    {
        int n = inputs.GetLength(0);
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            for (int t = 0; t < inputs.GetLength(1); t++)
            {
                for (int f = 0; f < inputs.GetLength(2); f++)
                {
                    T temp = inputs[i, t, f];
                    inputs[i, t, f] = inputs[j, t, f];
                    inputs[j, t, f] = temp;
                }
            }
            T[] tempTarget = targets[i];
            targets[i] = targets[j];
            targets[j] = tempTarget;
        }
    }

    /// <summary>
    /// 从输入数据中提取单个样本。
    /// </summary>
    /// <param name="inputs">输入数据。</param>
    /// <param name="index">样本索引。</param>
    /// <returns>单个样本数据。</returns>
    public T[,] GetSample(T[,,] inputs, int index)
    {
        int timeSteps = inputs.GetLength(1);
        int features = inputs.GetLength(2);
        T[,] sample = new T[timeSteps, features];
        for (int t = 0; t < timeSteps; t++)
        {
            for (int f = 0; f < features; f++)
            {
                sample[t, f] = inputs[index, t, f];
            }
        }
        return sample;
    }

    // 获取所有参数
    internal T[] GetParameters()
    {
        var parameters = new List<T>();
        foreach (var row in weights1) parameters.AddRange(row);
        foreach (var row in weights2) parameters.AddRange(row);
        parameters.AddRange(bias1);
        parameters.AddRange(bias2);
        return parameters.ToArray();
    }
}