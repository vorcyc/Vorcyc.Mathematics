using Vorcyc.Mathematics.MachineLearning;

namespace MachineLearning_example;




/// <summary>

/// 合成三簇二维分类数据，用于演示。

/// </summary>

internal static class BlobDataset

{

    private static readonly (double Cx, double Cy, int Label)[] Centers =

    [

        (2.0, 2.0, 0),

        (8.0, 8.0, 1),

        (2.0, 8.0, 2)

    ];



    public static (double[,] x, int[] y) CreateThreeBlob(int pointsPerClass = 20, int seed = 42)

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



    public static double[] GetRow(double[,] matrix, int row)

    {

        int cols = matrix.GetLength(1);

        var result = new double[cols];

        for (int j = 0; j < cols; j++)

            result[j] = matrix[row, j];

        return result;

    }



    public static int[] PredictAll(IClassifier<double> classifier, double[,] x) =>
        classifier.PredictBatch(x);

}


