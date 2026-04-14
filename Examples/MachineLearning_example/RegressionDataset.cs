namespace MachineLearning_example;



/// <summary>

/// 合成回归数据，用于演示 KNN / 线性模型。

/// </summary>

internal static class RegressionDataset

{

    /// <summary>

    /// 生成近似线性关系的多维回归数据。

    /// </summary>

    public static (double[,] x, double[] y) CreateLinearSynthetic(int rows = 120, int cols = 4, int seed = 42)

    {

        if (rows <= 0 || cols <= 0)

            throw new ArgumentOutOfRangeException(nameof(rows));



        var random = new Random(seed);

        var x = new double[rows, cols];

        var y = new double[rows];

        var coefficients = new double[cols];

        for (int j = 0; j < cols; j++)

            coefficients[j] = random.NextDouble() * 2.0 - 1.0;

        double intercept = random.NextDouble() * 3.0;



        for (int i = 0; i < rows; i++)

        {

            double target = intercept;

            for (int j = 0; j < cols; j++)

            {

                x[i, j] = random.NextDouble() * 10.0;

                target += coefficients[j] * x[i, j];

            }

            y[i] = target + (random.NextDouble() - 0.5) * 0.5;

        }



        return (x, y);

    }

}


