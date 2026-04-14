namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Matrix decomposition utilities.
/// </summary>
public static class MatrixDecomposition
{
    /// <summary>
    /// Computes the eigendecomposition of a real symmetric matrix using the Jacobi method.
    /// Eigenvectors are stored as <em>columns</em> of <see cref="SymmetricEigendecompositionResult{T}.Eigenvectors"/>.
    /// Eigenpairs are sorted by descending eigenvalue.
    /// </summary>
    public static SymmetricEigendecompositionResult<T> SymmetricEigendecomposition<T>(
        Matrix<T> matrix,
        T? tolerance = null,
        int maxIterations = 100)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (matrix.Rows != matrix.Columns)
            throw new ArgumentException("矩阵必须是方阵。", nameof(matrix));

        int n = matrix.Rows;
        var working = matrix.Clone();
        var eigenvectors = Matrix<T>.Eye(n);
        T tol = tolerance ?? T.CreateChecked(1e-12);
        T two = T.CreateChecked(2);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            if (!TryFindPivot(working, n, tol, out int p, out int q))
                break;

            T app = working[p, p];
            T aqq = working[q, q];
            T apq = working[p, q];
            T phi = T.Atan2(two * apq, aqq - app) * T.CreateChecked(0.5);
            T c = T.Cos(phi);
            T s = T.Sin(phi);

            ApplyJacobiRotation(working, eigenvectors, p, q, c, s);
        }

        var eigenvalues = new T[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = working[i, i];

        SortEigenpairsDescending(eigenvalues, eigenvectors, n);
        return new SymmetricEigendecompositionResult<T>(eigenvalues, eigenvectors);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFindPivot<T>(Matrix<T> matrix, int n, T tolerance, out int pivotP, out int pivotQ)
        where T : struct, IFloatingPointIeee754<T>
    {
        pivotP = 0;
        pivotQ = 1;
        T max = T.Abs(matrix[0, 1]);

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                T value = T.Abs(matrix[i, j]);
                if (value > max)
                {
                    max = value;
                    pivotP = i;
                    pivotQ = j;
                }
            }
        }

        return max > tolerance;
    }

    private static void ApplyJacobiRotation<T>(
        Matrix<T> matrix,
        Matrix<T> eigenvectors,
        int p,
        int q,
        T c,
        T s)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = matrix.Rows;
        T two = T.CreateChecked(2);

        for (int k = 0; k < n; k++)
        {
            if (k == p || k == q)
                continue;

            T apk = matrix[p, k];
            T aqk = matrix[q, k];
            matrix[p, k] = matrix[k, p] = c * apk - s * aqk;
            matrix[q, k] = matrix[k, q] = s * apk + c * aqk;
        }

        T app = matrix[p, p];
        T aqq = matrix[q, q];
        T apq = matrix[p, q];
        matrix[p, p] = c * c * app - two * s * c * apq + s * s * aqq;
        matrix[q, q] = s * s * app + two * s * c * apq + c * c * aqq;
        matrix[p, q] = matrix[q, p] = T.Zero;

        for (int k = 0; k < n; k++)
        {
            T vkp = eigenvectors[k, p];
            T vkq = eigenvectors[k, q];
            eigenvectors[k, p] = c * vkp - s * vkq;
            eigenvectors[k, q] = s * vkp + c * vkq;
        }
    }

    private static void SortEigenpairsDescending<T>(T[] eigenvalues, Matrix<T> eigenvectors, int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < n - 1; i++)
        {
            int maxIndex = i;
            for (int j = i + 1; j < n; j++)
            {
                if (eigenvalues[j] > eigenvalues[maxIndex])
                    maxIndex = j;
            }

            if (maxIndex == i)
                continue;

            (eigenvalues[i], eigenvalues[maxIndex]) = (eigenvalues[maxIndex], eigenvalues[i]);
            SwapColumns(eigenvectors, i, maxIndex, n);
        }
    }

    private static void SwapColumns<T>(Matrix<T> matrix, int colA, int colB, int rows)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < rows; i++)
        {
            (matrix[i, colA], matrix[i, colB]) = (matrix[i, colB], matrix[i, colA]);
        }
    }

    private const int BidiagonalSvdElementThreshold = 256;
    private const int BidiagonalSvdDimensionThreshold = 48;

    /// <summary>
    /// Computes a thin SVD: A ≈ U·diag(Σ)·Vᵀ, where k = min(m, n).
    /// </summary>
    public static SingularValueDecompositionResult<T> SingularValueDecomposition<T>(
        Matrix<T> matrix,
        T? tolerance = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        int m = matrix.Rows;
        int n = matrix.Columns;
        if (m == 0 || n == 0)
            throw new ArgumentException("矩阵不能为空。", nameof(matrix));

        T tol = tolerance ?? T.CreateChecked(1e-12);
        if (ShouldUseBidiagonalSvd(m, n))
        {
            return m >= n
                ? BidiagonalSvd.ComputeThin(matrix, m, n, tol)
                : TransposeThinSvd(BidiagonalSvd.ComputeThin(matrix.Transpose(), n, m, tol));
        }

        return m >= n
            ? ComputeThinSvdTall(matrix, m, n, tol)
            : ComputeThinSvdWide(matrix, m, n, tol);
    }

    private static bool ShouldUseBidiagonalSvd(int m, int n)
        => m * n > BidiagonalSvdElementThreshold || Math.Min(m, n) > BidiagonalSvdDimensionThreshold;

    private static SingularValueDecompositionResult<T> TransposeThinSvd<T>(
        SingularValueDecompositionResult<T> svdAt)
        where T : struct, IFloatingPointIeee754<T>
        => new(svdAt.VT.Transpose(), svdAt.SingularValues, svdAt.U.Transpose());

    /// <summary>
    /// Computes the Moore–Penrose pseudoinverse A⁺ (n×m).
    /// </summary>
    public static Matrix<T> Pseudoinverse<T>(Matrix<T> matrix, T? tolerance = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        var svd = SingularValueDecomposition(matrix, tolerance);
        int k = svd.SingularValues.Length;
        int m = matrix.Rows;
        int n = matrix.Columns;
        T tol = tolerance ?? T.CreateChecked(1e-12);
        var pinv = new Matrix<T>(n, m);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                T sum = T.Zero;
                for (int t = 0; t < k; t++)
                {
                    T sigma = svd.SingularValues[t];
                    if (sigma <= tol)
                        continue;

                    sum += svd.VT[t, i] * (T.One / sigma) * svd.U[j, t];
                }

                pinv[i, j] = sum;
            }
        }

        return pinv;
    }

    /// <summary>
    /// Reconstructs A from thin SVD factors.
    /// </summary>
    public static Matrix<T> Reconstruct<T>(SingularValueDecompositionResult<T> svd)
        where T : struct, IFloatingPointIeee754<T>
    {
        int m = svd.U.Rows;
        int n = svd.VT.Columns;
        int k = svd.SingularValues.Length;
        var result = new Matrix<T>(m, n);

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                T sum = T.Zero;
                for (int t = 0; t < k; t++)
                    sum += svd.U[i, t] * svd.SingularValues[t] * svd.VT[t, j];
                result[i, j] = sum;
            }
        }

        return result;
    }

    private static SingularValueDecompositionResult<T> ComputeThinSvdTall<T>(
        Matrix<T> matrix,
        int m,
        int n,
        T tolerance)
        where T : struct, IFloatingPointIeee754<T>
    {
        matrix.QRDecomposition(out Matrix<T> qFull, out Matrix<T> rFull);
        ExtractThinQr(qFull, rFull, m, n, out Matrix<T> qThin, out Matrix<T> r);

        var rSvd = JacobiSvdSquare(r, tolerance);
        var u = qThin * rSvd.U;
        return new SingularValueDecompositionResult<T>(u, rSvd.SingularValues, rSvd.VT);
    }

    private static SingularValueDecompositionResult<T> ComputeThinSvdWide<T>(
        Matrix<T> matrix,
        int m,
        int n,
        T tolerance)
        where T : struct, IFloatingPointIeee754<T>
        => TransposeThinSvd(ComputeThinSvdTall(matrix.Transpose(), n, m, tolerance));

    private static void ExtractThinQr<T>(
        Matrix<T> qFull,
        Matrix<T> rFull,
        int m,
        int n,
        out Matrix<T> qThin,
        out Matrix<T> r)
        where T : struct, IFloatingPointIeee754<T>
    {
        qThin = new Matrix<T>(m, n);
        r = new Matrix<T>(n, n);

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
                qThin[i, j] = qFull[i, j];
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                r[i, j] = j >= i ? rFull[i, j] : T.Zero;
        }
    }

    /// <summary>
    /// One-sided Jacobi SVD for a small square matrix (avoids forming AᵀA).
    /// </summary>
    internal static SingularValueDecompositionResult<T> JacobiSvdSquare<T>(
        Matrix<T> matrix,
        T tolerance,
        int maxSweeps = 32)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = matrix.Rows;
        if (matrix.Columns != n)
            throw new ArgumentException("Jacobi SVD requires a square matrix.", nameof(matrix));

        var working = matrix.Clone();
        var v = Matrix<T>.Eye(n);

        for (int sweep = 0; sweep < maxSweeps; sweep++)
        {
            bool changed = false;
            for (int p = 0; p < n; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    if (OrthogonalizeColumns(working, v, p, q, tolerance, n))
                        changed = true;
                }
            }

            if (!changed)
                break;
        }

        var singularValues = new T[n];
        var u = new Matrix<T>(n, n);
        for (int j = 0; j < n; j++)
        {
            T normSquared = ColumnNormSquared(working, j, n);
            T norm = T.Sqrt(normSquared);
            singularValues[j] = norm;
            for (int i = 0; i < n; i++)
                u[i, j] = norm > tolerance ? working[i, j] / norm : T.Zero;
        }

        SortSingularValuesDescending(singularValues, u, v, n);
        return new SingularValueDecompositionResult<T>(u, singularValues, v.Transpose());
    }

    private static bool OrthogonalizeColumns<T>(
        Matrix<T> matrix,
        Matrix<T> v,
        int p,
        int q,
        T tolerance,
        int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        T alpha = ColumnNormSquared(matrix, p, n);
        T beta = ColumnNormSquared(matrix, q, n);
        T gamma = ColumnDot(matrix, p, q, n);
        T threshold = tolerance * T.Sqrt(alpha * beta);
        if (T.Abs(gamma) <= threshold)
            return false;

        T mu = beta - alpha;
        T radicand = mu * mu + T.CreateChecked(4) * gamma * gamma;
        T nu = mu >= T.Zero ? T.Sqrt(radicand) : -T.Sqrt(radicand);
        T cos = T.Sqrt(T.CreateChecked(0.5) * (T.One + mu / nu));
        T sin = T.Sqrt(T.CreateChecked(0.5) * (T.One - mu / nu));
        if (gamma < T.Zero)
            sin = -sin;

        ApplyRightRotation(matrix, p, q, cos, sin, n);
        ApplyRightRotation(v, p, q, cos, sin, n);
        return true;
    }

    private static T ColumnNormSquared<T>(Matrix<T> matrix, int column, int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T value = matrix[i, column];
            sum += value * value;
        }

        return sum;
    }

    private static T ColumnDot<T>(Matrix<T> matrix, int p, int q, int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        for (int i = 0; i < n; i++)
            sum += matrix[i, p] * matrix[i, q];
        return sum;
    }

    private static void ApplyRightRotation<T>(
        Matrix<T> matrix,
        int p,
        int q,
        T cos,
        T sin,
        int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < n; i++)
        {
            T vp = matrix[i, p];
            T vq = matrix[i, q];
            matrix[i, p] = cos * vp - sin * vq;
            matrix[i, q] = sin * vp + cos * vq;
        }
    }

    private static void SortSingularValuesDescending<T>(
        T[] singularValues,
        Matrix<T> u,
        Matrix<T> v,
        int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < n - 1; i++)
        {
            int maxIndex = i;
            for (int j = i + 1; j < n; j++)
            {
                if (singularValues[j] > singularValues[maxIndex])
                    maxIndex = j;
            }

            if (maxIndex == i)
                continue;

            (singularValues[i], singularValues[maxIndex]) = (singularValues[maxIndex], singularValues[i]);
            SwapColumns(u, i, maxIndex, n);
            SwapColumns(v, i, maxIndex, n);
        }
    }
}

/// <summary>
/// Result of a symmetric eigendecomposition.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly record struct SymmetricEigendecompositionResult<T>(
    T[] Eigenvalues,
    Matrix<T> Eigenvectors)
    where T : struct, IFloatingPointIeee754<T>;

/// <summary>
/// Thin singular value decomposition result: A ≈ U·diag(<see cref="SingularValues"/>)·Vᵀ.
/// </summary>
public readonly record struct SingularValueDecompositionResult<T>(
    Matrix<T> U,
    T[] SingularValues,
    Matrix<T> VT)
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Gets k = min(m, n).</summary>
    public int Rank => SingularValues.Length;
}
