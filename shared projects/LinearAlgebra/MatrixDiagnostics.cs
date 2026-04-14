namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;

/// <summary>
/// Matrix numerical diagnostics.
/// </summary>
public static class MatrixDiagnostics
{
    /// <summary>
    /// Estimates κ₂(A) ≈ σ_max / σ_min from a thin SVD.
    /// </summary>
    public static T ConditionNumber<T>(Matrix<T> matrix, T? tolerance = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        var svd = MatrixDecomposition.SingularValueDecomposition(matrix, tolerance);
        T sigmaMax = svd.SingularValues[0];
        T sigmaMin = svd.SingularValues[^1];
        T tol = tolerance ?? T.CreateChecked(1e-12);

        if (sigmaMin <= tol)
            return T.CreateChecked(double.PositiveInfinity);

        return sigmaMax / sigmaMin;
    }

    /// <summary>
    /// Returns true when the estimated condition number exceeds <paramref name="threshold"/>.
    /// </summary>
    public static bool IsIllConditioned<T>(Matrix<T> matrix, T threshold, T? tolerance = null)
        where T : struct, IFloatingPointIeee754<T>
        => ConditionNumber(matrix, tolerance) > threshold;

    /// <summary>
    /// Checks whether a square matrix is symmetric within <paramref name="tolerance"/>.
    /// </summary>
    public static bool IsSymmetric<T>(Matrix<T> matrix, T? tolerance = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (matrix.Rows != matrix.Columns)
            return false;

        T tol = tolerance ?? T.CreateChecked(1e-10);
        int n = matrix.Rows;
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (T.Abs(matrix[i, j] - matrix[j, i]) > tol)
                    return false;
            }
        }

        return true;
    }
}
