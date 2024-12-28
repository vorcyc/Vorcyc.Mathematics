using System.Numerics;

namespace Vorcyc.Mathematics.LinearAlgebra;

public static  class MatrixOperations
{

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the matrices.</typeparam>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The product of the two matrices.</returns>
    public static T[,] Multiply<T>(T[,] a, T[,] b) where T : INumber<T>
    {
        int aRows = a.GetLength(0);
        int aCols = a.GetLength(1);
        int bRows = b.GetLength(0);
        int bCols = b.GetLength(1);

        if (aCols != bRows)
            throw new ArgumentException("Matrices must have compatible dimensions for multiplication.");

        T[,] result = new T[aRows, bCols];

        for (int i = 0; i < aRows; i++)
        {
            for (int j = 0; j < bCols; j++)
            {
                for (int k = 0; k < aCols; k++)
                {
                    result[i, j] += a[i, k] * b[k, j];
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The product of the two matrices.</returns>
    public static float[][] Multiply(float[][] a, float[][] b)
    {
        int aRows = a.Length;
        int aCols = a[0].Length;
        int bRows = b.Length;
        int bCols = b[0].Length;

        if (aCols != bRows)
            throw new ArgumentException("Matrices must have compatible dimensions for multiplication.");

        float[][] result = new float[aRows][];
        for (int i = 0; i < aRows; i++)
        {
            result[i] = new float[bCols];
            for (int j = 0; j < bCols; j++)
            {
                for (int k = 0; k < aCols; k++)
                {
                    result[i][j] += a[i][k] * b[k][j];
                }
            }
        }

        return result;
    }
}
