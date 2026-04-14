namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// <see cref="T:T[,]"/> 特征矩阵的共享切片工具。
/// </summary>
internal static class Array2DHelpers
{
    public static T[] GetRow<T>(T[,] matrix, int row) where T : struct
    {
        int cols = matrix.GetLength(1);
        var result = new T[cols];
        for (int j = 0; j < cols; j++)
            result[j] = matrix[row, j];
        return result;
    }

    public static T[,] CopyRows<T>(T[,] source, int[] indices) where T : struct
    {
        int cols = source.GetLength(1);
        var result = new T[indices.Length, cols];
        for (int i = 0; i < indices.Length; i++)
        {
            int row = indices[i];
            for (int j = 0; j < cols; j++)
                result[i, j] = source[row, j];
        }
        return result;
    }

    public static T[] CopyLabels<T>(T[] source, int[] indices) where T : struct
    {
        var result = new T[indices.Length];
        for (int i = 0; i < indices.Length; i++)
            result[i] = source[indices[i]];
        return result;
    }

    public static int[] CopyIntLabels(int[] source, int[] indices)
    {
        var result = new int[indices.Length];
        for (int i = 0; i < indices.Length; i++)
            result[i] = source[indices[i]];
        return result;
    }
}
