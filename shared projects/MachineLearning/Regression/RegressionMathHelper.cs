using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// Shared matrix construction and least-squares solving utilities for regression algorithms.
/// </summary>
internal static class RegressionMathHelper
{
    public static Matrix<T> BuildVandermonde<T>(ReadOnlySpan<T> x, int degree)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = x.Length;
        var matrix = new Matrix<T>(n, degree + 1);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= degree; j++)
                matrix[i, j] = T.Pow(x[i], T.CreateChecked(j));
        }

        return matrix;
    }

    public static Matrix<T> BuildDesignMatrixWithIntercept<T>(T[,] x)
        where T : struct, IFloatingPointIeee754<T>
    {
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        var designMatrix = new Matrix<T>(rows, cols + 1);
        for (int i = 0; i < rows; i++)
        {
            designMatrix[i, 0] = T.One;
            for (int j = 0; j < cols; j++)
                designMatrix[i, j + 1] = x[i, j];
        }

        return designMatrix;
    }

    public static T[] SolveLeastSquares<T>(Matrix<T> designMatrix, ReadOnlySpan<T> y)
        where T : struct, IFloatingPointIeee754<T>
        => designMatrix.SolveLeastSquares(y);

    public static T[] SolveRidgeLeastSquares<T>(
        Matrix<T> designMatrix,
        ReadOnlySpan<T> y,
        T lambda,
        bool regularizeIntercept = true)
        where T : struct, IFloatingPointIeee754<T>
        => designMatrix.SolveRidgeLeastSquares(y, lambda, regularizeIntercept);

    public static T PredictVandermonde<T>(T x, ReadOnlySpan<T> coefficients, int degree)
        where T : struct, IFloatingPointIeee754<T>
    {
        T result = T.Zero;
        for (int i = 0; i <= degree; i++)
            result += coefficients[i] * T.Pow(x, T.CreateChecked(i));
        return result;
    }

    public static T ComputeRSquared<T>(ReadOnlySpan<T> y, Func<int, T> predict)
        where T : struct, IFloatingPointIeee754<T>
    {
        T mean = VectorSpan.Sum(y) / T.CreateChecked(y.Length);
        T ssTot = T.Zero;
        T ssRes = T.Zero;
        for (int i = 0; i < y.Length; i++)
        {
            T residual = y[i] - predict(i);
            T deviation = y[i] - mean;
            ssTot += deviation * deviation;
            ssRes += residual * residual;
        }

        return ssTot != T.Zero ? T.One - (ssRes / ssTot) : T.Zero;
    }
}
