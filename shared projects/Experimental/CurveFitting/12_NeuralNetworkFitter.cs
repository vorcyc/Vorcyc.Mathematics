using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

internal static class NeuralNetworkFitter
{
    private static T ComputeMeanSquaredError<T>(NeuralNetwork_Parallel<T> nn, T[,,] inputs, T[][] targets)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T totalError = T.Zero;
        int sampleSize = inputs.GetLength(0);
        for (int i = 0; i < sampleSize; i++)
        {
            T[,] sample = nn.GetSample(inputs, i);
            T[] output = nn.Forward(sample);
            totalError += (targets[i][0] - output[0]) * (targets[i][0] - output[0]);
        }
        return totalError / T.CreateChecked(sampleSize);
    }    
    
    private static T ComputeMeanSquaredError<T>(NeuralNetwork_Sequential<T> nn, T[,,] inputs, T[][] targets)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        T totalError = T.Zero;
        int sampleSize = inputs.GetLength(0);
        for (int i = 0; i < sampleSize; i++)
        {
            T[,] sample = nn.GetSample(inputs, i);
            T[] output = nn.Forward(sample);
            totalError += (targets[i][0] - output[0]) * (targets[i][0] - output[0]);
        }
        return totalError / T.CreateChecked(sampleSize);
    }

    internal static FitResult<T> Fit_SingleColumn<T>(
        Span<T> xData, Span<T> yData, int epochs = 5000, int hiddenNodes = 10, T? learningRate = null,
        TrainingProgressHandler<T>? trainingProgressCallback = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length)
            throw new ArgumentException("xData and yData must have the same length");

        int sampleSize = xData.Length;
        T[] xArray = xData.ToArray(); // 转换为数组以重复使用
        T[] yArray = yData.ToArray();
        T[,,] inputs = new T[sampleSize, 1, 1];
        T[][] targets = new T[sampleSize][];
        for (int i = 0; i < sampleSize; i++)
        {
            inputs[i, 0, 0] = xArray[i];
            targets[i] = [yArray[i]]; // C# 12 集合表达式
        }

        var nn = new NeuralNetwork_Sequential<T>(1, 1, hiddenNodes, 1);
        T lr = learningRate ?? T.CreateChecked(0.1);
        nn.Train(inputs, targets, lr, epochs, trainingProgressCallback);

        Func<T, T> predict = x =>
        {
            T[,] input = new T[1, 1] { { x } };
            return nn.Forward(input)[0];
        };

        T mse = ComputeMeanSquaredError(nn, inputs, targets);
        return new FitResult<T>(predict, nn.GetParameters(), mse);
    }

    /// <summary>
    /// 使用神经网络拟合多列输入数据到单值输出。
    /// </summary>
    /// <param name="xData">输入数据，每行为多列数据，形状为[样本数]。</param>
    /// <param name="yData">目标数据，形状为[样本数]。</param>
    /// <param name="epochs">训练轮数，默认为5000。</param>
    /// <param name="hiddenNodes">隐藏层节点数，默认为10。</param>
    /// <param name="trainingProgressCallback">训练进度回调，可为空。</param>
    /// <returns>多列输入拟合结果，包括预测函数、参数和均方误差。</returns>
    internal static MultiColumnFitResult<T> Fit_MultiColumn<T>(
        DataRow<T>[] xData, Span<T> yData, int epochs = 5000, int hiddenNodes = 10, T? learningRate = null,
        TrainingProgressHandler<T>? trainingProgressCallback = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (xData.Length != yData.Length)
            throw new ArgumentException("xData and yData must have the same length");

        int sampleSize = xData.Length;
        int features = xData[0].ColumnCount;
        if (xData.Any(row => row.ColumnCount != features))
            throw new ArgumentException("All rows must have the same number of columns");

        T[,,] inputs = new T[sampleSize, 1, features];
        T[][] targets = new T[sampleSize][];
        for (int i = 0; i < sampleSize; i++)
        {
            for (int f = 0; f < features; f++)
                inputs[i, 0, f] = xData[i][f];
            targets[i] = new T[] { yData[i] };
        }

        var nn = new NeuralNetwork_Sequential<T>(1, features, hiddenNodes, 1);
        T lr = learningRate ?? T.CreateChecked(0.1);
        nn.Train(inputs, targets, lr, epochs, trainingProgressCallback);

        Func<DataRow<T>, T> multiPredict = row =>
        {
            if (row.ColumnCount != features)
                throw new ArgumentException($"Row column count ({row.ColumnCount}) must match feature count ({features})");
            T[,] input = new T[1, features];
            for (int f = 0; f < features; f++)
                input[0, f] = row[f];
            return nn.Forward(input)[0];
        };

        T mse = ComputeMeanSquaredError(nn, inputs, targets);
        return new MultiColumnFitResult<T>(multiPredict, nn.GetParameters(), mse );
    }


    internal static void TEST()
    {
        Random rand = new Random();

        int DATA_AMOUNT = 500;

        // 1. 拟合二次函数 (y = x²)
        Console.WriteLine("拟合单列数据 (y = x²)：");
        double[] xData1 = new double[DATA_AMOUNT]; // 数据量增加到 1000
        double[] yData1 = new double[DATA_AMOUNT];
        for (int i = 0; i < DATA_AMOUNT; i++)
        {
            double x = rand.NextDouble() * 2 - 1; // [-1, 1]
            xData1[i] = (x + 1) / 2; // 归一化到 [0, 1]
            yData1[i] = (x * x + 1) / 2; // 归一化到 [0, 1]
        }
        var result1 = NeuralNetworkFitter.Fit_SingleColumn(xData1.AsSpan(), yData1.AsSpan(), 200000, 10,
            trainingProgressCallback: (epoch, total, error) => { if (epoch % 20000 == 0) Console.WriteLine($"Epoch {epoch}/{total}, Error: {error:F6}"); });
        Console.WriteLine($"Mean Squared Error: {result1.MeanSquaredError:F6}");
        Console.WriteLine("\nTest Results (Quadratic):");
        for (int i = 0; i < 5; i++)
        {
            double x = (xData1[i] * 2) - 1;
            double predicted = result1.Predict(xData1[i]);
            Console.WriteLine($"x: {x:F3}, Predicted: {predicted:F3}, Actual: {yData1[i]:F3}");
        }
        Console.WriteLine();

        // 2. 拟合正弦函数 (y = sin(x))
        Console.WriteLine("拟合单列数据 (y = sin(x))：");
        double[] xData2 = new double[DATA_AMOUNT];
        double[] yData2 = new double[DATA_AMOUNT];
        for (int i = 0; i < DATA_AMOUNT; i++)
        {
            double x = rand.NextDouble() * 2 * Math.PI; // [0, 2π]
            xData2[i] = x / (2 * Math.PI); // 归一化到 [0, 1]
            yData2[i] = (Math.Sin(x) + 1) / 2; // 归一化到 [0, 1]
        }
        var result2 = NeuralNetworkFitter.Fit_SingleColumn(xData2.AsSpan(), yData2.AsSpan(), 200000, 15,
            trainingProgressCallback: (epoch, total, error) => { if (epoch % 20000 == 0) Console.WriteLine($"Epoch {epoch}/{total}, Error: {error:F6}"); });
        Console.WriteLine($"Mean Squared Error: {result2.MeanSquaredError:F6}");
        Console.WriteLine("\nTest Results (Sine):");
        for (int i = 0; i < 5; i++)
        {
            double x = xData2[i] * 2 * Math.PI;
            double predicted = result2.Predict(xData2[i]);
            Console.WriteLine($"x: {x:F3}, Predicted: {predicted:F3}, Actual: {yData2[i]:F3}");
        }
        Console.WriteLine();

        // 3. 拟合指数衰减 (y = e^(-x))
        Console.WriteLine("拟合单列数据 (y = e^(-x))：");
        double[] xData3 = new double[DATA_AMOUNT];
        double[] yData3 = new double[DATA_AMOUNT];
        for (int i = 0; i < DATA_AMOUNT; i++)
        {
            double x = rand.NextDouble() * 5; // [0, 5]
            xData3[i] = x / 5; // 归一化到 [0, 1]
            yData3[i] = Math.Exp(-x); // 自然范围 [0, 1]
        }
        var result3 = NeuralNetworkFitter.Fit_SingleColumn(xData3.AsSpan(), yData3.AsSpan(), 200000, 12,
            trainingProgressCallback: (epoch, total, error) => { if (epoch % 20000 == 0) Console.WriteLine($"Epoch {epoch}/{total}, Error: {error:F6}"); });
        Console.WriteLine($"Mean Squared Error: {result3.MeanSquaredError:F6}");
        Console.WriteLine("\nTest Results (Exponential Decay):");
        for (int i = 0; i < 5; i++)
        {
            double x = xData3[i] * 5;
            double predicted = result3.Predict(xData3[i]);
            Console.WriteLine($"x: {x:F3}, Predicted: {predicted:F3}, Actual: {yData3[i]:F3}");
        }
        Console.WriteLine();

        // 4. 拟合多列加权和 (y = 0.3*col1 + 0.5*col2 + 0.2*col3)
        Console.WriteLine("拟合多列数据 (y = 0.3*col1 + 0.5*col2 + 0.2*col3)：");
        DataRow<double>[] multiXData1 = new DataRow<double>[DATA_AMOUNT]; // 数据量增加到 1000
        double[] multiYData1 = new double[DATA_AMOUNT];
        for (int i = 0; i < DATA_AMOUNT; i++)
        {
            double col1 = rand.NextDouble();
            double col2 = rand.NextDouble();
            double col3 = rand.NextDouble();
            multiXData1[i] = new DataRow<double>(col1, col2, col3);
            multiYData1[i] = 0.3 * col1 + 0.5 * col2 + 0.2 * col3; // 范围 [0, 1]
        }
        var result4 = NeuralNetworkFitter.Fit_MultiColumn(multiXData1, multiYData1.AsSpan(), 200000, 20,
            trainingProgressCallback: (epoch, total, error) => { if (epoch % 20000 == 0) Console.WriteLine($"Epoch {epoch}/{total}, Error: {error:F6}"); });
        Console.WriteLine($"Mean Squared Error: {result4.MeanSquaredError:F6}");
        Console.WriteLine("\nTest Results (Multi-Column Weighted Sum):");
        for (int i = 0; i < 5; i++)
        {
            DataRow<double> input = multiXData1[i];
            double predicted = result4.Predict!(input);
            double actual = multiYData1[i];
            double[] cols = input.ToArray();
            Console.WriteLine($"Input: [{cols[0]:F3}, {cols[1]:F3}, {cols[2]:F3}], Predicted: {predicted:F3}, Actual: {actual:F3}");
        }
        Console.WriteLine();

        // 5. 拟合多项式组合 (y = col1² + 2*col2)
        Console.WriteLine("拟合多列数据 (y = col1² + 2*col2)：");
        DataRow<double>[] multiXData2 = new DataRow<double>[DATA_AMOUNT]; // 数据量增加到 1000
        double[] multiYData2 = new double[DATA_AMOUNT];
        for (int i = 0; i < DATA_AMOUNT; i++)
        {
            double col1 = rand.NextDouble(); // [0, 1]
            double col2 = rand.NextDouble(); // [0, 1]
            multiXData2[i] = new DataRow<double>(col1, col2);
            multiYData2[i] = (col1 * col1 + 2 * col2) / 3; // 范围 [0, 3] 归一化到 [0, 1]
        }
        var result5 = NeuralNetworkFitter.Fit_MultiColumn(multiXData2, multiYData2.AsSpan(), 200000, 15,
            trainingProgressCallback: (epoch, total, error) => { if (epoch % 20000 == 0) Console.WriteLine($"Epoch {epoch}/{total}, Error: {error:F6}"); });
        Console.WriteLine($"Mean Squared Error: {result5.MeanSquaredError:F6}");
        Console.WriteLine("\nTest Results (Polynomial Combination):");
        for (int i = 0; i < 5; i++)
        {
            DataRow<double> input = multiXData2[i];
            double predicted = result5.Predict!(input);
            double actual = multiYData2[i];
            double[] cols = input.ToArray();
            Console.WriteLine($"Input: [{cols[0]:F3}, {cols[1]:F3}], Predicted: {predicted:F3}, Actual: {actual:F3}");
        }
    }
}
