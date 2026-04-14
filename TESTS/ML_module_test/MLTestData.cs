using Vorcyc.Mathematics.MachineLearning;

namespace ML_module_test;


/// <summary>
/// ML 模块测试共用数据集与工具。
/// </summary>
internal static class MLTestData
{
    private static readonly (double Cx, double Cy, int Label)[] Centers =
    [
        (2.0, 2.0, 0),
        (8.0, 8.0, 1),
        (2.0, 8.0, 2)
    ];

    /// <summary>
    /// 生成三簇二维 blob 分类数据。
    /// </summary>
    /// <param name="pointsPerClass">每类样本数。</param>
    /// <param name="seed">随机种子，默认 42。</param>
    public static (double[,] x, int[] y) CreateThreeBlobDataset(int pointsPerClass = 10, int seed = 42)
    {
        if (pointsPerClass <= 0)
            throw new ArgumentOutOfRangeException(nameof(pointsPerClass));

        var random = new Random(seed);
        int total = pointsPerClass * Centers.Length;
        var x = new double[total, 2];
        var y = new int[total];
        int index = 0;

        foreach (var (cx, cy, label) in Centers)
        {
            for (int i = 0; i < pointsPerClass; i++)
            {
                x[index, 0] = cx + (random.NextDouble() - 0.5) * 0.8;
                x[index, 1] = cy + (random.NextDouble() - 0.5) * 0.8;
                y[index] = label;
                index++;
            }
        }

        return (x, y);
    }

    public static int[] PredictAll(IClassifier<double> classifier, double[,] x) =>
        classifier.PredictBatch(x);

    public static double[] GetRow(double[,] matrix, int row)
    {
        int cols = matrix.GetLength(1);
        var result = new double[cols];
        for (int j = 0; j < cols; j++)
            result[j] = matrix[row, j];
        return result;
    }
}
