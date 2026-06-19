namespace MachineLearning_example;

/// <summary>
/// 鍚堟垚鍥炲綊鏁版嵁锛岀敤浜庢紨绀?KNN / 绾挎€фā鍨嬨€?
/// </summary>
internal static class RegressionDataset
{
    /// <summary>
    /// 鐢熸垚杩戜技绾挎€у叧绯荤殑澶氱淮鍥炲綊鏁版嵁銆?
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
