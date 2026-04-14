namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Golub–Kahan bidiagonalization followed by SVD of the reduced bidiagonal factor.
/// </summary>
internal static class BidiagonalSvd
{
    internal static SingularValueDecompositionResult<T> ComputeThin<T>(
        Matrix<T> matrix,
        int m,
        int n,
        T tolerance)
        where T : struct, IFloatingPointIeee754<T>
    {
        int k = Math.Min(m, n);
        var u = Matrix<T>.Eye(m);
        var v = Matrix<T>.Eye(n);
        var working = matrix.Clone();

        Bidiagonalize(working, u, v, m, n);

        var diagonal = new T[k];
        var superdiagonal = new T[Math.Max(0, k - 1)];
        ExtractBidiagonal(working, diagonal, superdiagonal);

        var bMatrix = BuildUpperBidiagonal(diagonal, superdiagonal, k);
        var bSvd = ImplicitQrBidiagonalSvd(diagonal, superdiagonal, k, tolerance)
            ?? MatrixDecomposition.JacobiSvdSquare(bMatrix, tolerance);

        var singularValues = bSvd.SingularValues;
        var uThin = CombineLeftTransform(u, bSvd.U, m, k);
        var vtThin = CombineRightTransform(bSvd.VT, v, k, n);
        SortSingularValuesDescending(singularValues, uThin, vtThin, k);
        return new SingularValueDecompositionResult<T>(uThin, singularValues, vtThin);
    }

    private static void Bidiagonalize<T>(
        Matrix<T> a,
        Matrix<T> u,
        Matrix<T> v,
        int m,
        int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        int k = Math.Min(m, n);
        var workspace = new T[Math.Max(m, n)];

        for (int j = 0; j < k; j++)
        {
            int columnLength = m - j;
            for (int i = 0; i < columnLength; i++)
                workspace[i] = a[j + i, j];

            GenerateHouseholder(workspace.AsSpan(0, columnLength), out T tau, out int length);
            if (tau != T.Zero)
            {
                workspace[0] = T.One;
                ApplyHouseholderLeft(a, u, workspace.AsSpan(0, length), tau, j, j, m, n);
            }

            if (j >= k - 1)
                continue;

            int rowLength = n - j - 1;
            for (int i = 0; i < rowLength; i++)
                workspace[i] = a[j, j + 1 + i];

            GenerateHouseholder(workspace.AsSpan(0, rowLength), out tau, out length);
            if (tau != T.Zero)
            {
                workspace[0] = T.One;
                ApplyHouseholderRight(a, v, workspace.AsSpan(0, length), tau, j, j + 1, m, n);
            }
        }
    }

    private static void GenerateHouseholder<T>(Span<T> x, out T tau, out int length)
        where T : struct, IFloatingPointIeee754<T>
    {
        length = x.Length;
        if (length <= 1)
        {
            tau = T.Zero;
            return;
        }

        T alpha = x[0];
        T scale = T.Zero;
        for (int i = 1; i < length; i++)
            scale += x[i] * x[i];

        if (scale == T.Zero)
        {
            tau = T.Zero;
            return;
        }

        scale = T.Sqrt(scale);
        T beta = alpha >= T.Zero ? -T.Sqrt(alpha * alpha + scale * scale) : T.Sqrt(alpha * alpha + scale * scale);
        tau = (beta - alpha) / beta;
        T divisor = alpha - beta;
        for (int i = 1; i < length; i++)
            x[i] /= divisor;
        x[0] = T.One;
    }

    private static void ApplyHouseholderLeft<T>(
        Matrix<T> a,
        Matrix<T> u,
        ReadOnlySpan<T> v,
        T tau,
        int rowOffset,
        int colOffset,
        int m,
        int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int j = colOffset; j < n; j++)
        {
            T dot = T.Zero;
            for (int i = 0; i < v.Length; i++)
                dot += v[i] * a[rowOffset + i, j];

            dot *= tau;
            for (int i = 0; i < v.Length; i++)
                a[rowOffset + i, j] -= dot * v[i];
        }

        for (int j = 0; j < m; j++)
        {
            T dot = T.Zero;
            for (int i = 0; i < v.Length; i++)
                dot += v[i] * u[j, rowOffset + i];

            dot *= tau;
            for (int i = 0; i < v.Length; i++)
                u[j, rowOffset + i] -= dot * v[i];
        }
    }

    private static void ApplyHouseholderRight<T>(
        Matrix<T> a,
        Matrix<T> v,
        ReadOnlySpan<T> householder,
        T tau,
        int rowOffset,
        int colOffset,
        int m,
        int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = rowOffset; i < m; i++)
        {
            T dot = T.Zero;
            for (int j = 0; j < householder.Length; j++)
                dot += a[i, colOffset + j] * householder[j];

            dot *= tau;
            for (int j = 0; j < householder.Length; j++)
                a[i, colOffset + j] -= dot * householder[j];
        }

        for (int i = 0; i < n; i++)
        {
            T dot = T.Zero;
            for (int j = 0; j < householder.Length; j++)
                dot += v[i, colOffset + j] * householder[j];

            dot *= tau;
            for (int j = 0; j < householder.Length; j++)
                v[i, colOffset + j] -= dot * householder[j];
        }
    }

    private static void ExtractBidiagonal<T>(
        Matrix<T> a,
        T[] diagonal,
        T[] superdiagonal)
        where T : struct, IFloatingPointIeee754<T>
    {
        int k = diagonal.Length;
        for (int i = 0; i < k; i++)
            diagonal[i] = a[i, i];

        for (int i = 0; i < superdiagonal.Length; i++)
            superdiagonal[i] = a[i, i + 1];
    }

    private static Matrix<T> BuildUpperBidiagonal<T>(T[] diagonal, T[] superdiagonal, int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        var b = new Matrix<T>(k, k);
        for (int i = 0; i < k; i++)
            b[i, i] = diagonal[i];

        for (int i = 0; i < superdiagonal.Length; i++)
            b[i, i + 1] = superdiagonal[i];

        return b;
    }

    /// <summary>
    /// Implicit QR sweep on an upper bidiagonal matrix (Golub–Reinsch).
    /// Returns null when convergence is not reached within the iteration budget.
    /// </summary>
    private static SingularValueDecompositionResult<T>? ImplicitQrBidiagonalSvd<T>(
        T[] diagonal,
        T[] superdiagonal,
        int k,
        T tolerance)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (k <= 1)
            return null;

        var d = (T[])diagonal.Clone();
        var e = (T[])superdiagonal.Clone();
        var u = Matrix<T>.Eye(k);
        var v = Matrix<T>.Eye(k);
        T eps = tolerance;

        for (int iter = 0; iter < 75 * k; iter++)
        {
            int start = 0;
            while (start < k - 1 && IsNegligible(e[start], d[start], d[start + 1], eps))
                e[start++] = T.Zero;

            if (start == k - 1)
                break;

            int end = start + 1;
            while (end < k - 1 && !IsNegligible(e[end], d[end], d[end + 1], eps))
                end++;

            WilkinsonQrStep(d, e, u, v, start, end);

            if (iter == 75 * k - 1)
                return null;
        }

        for (int i = 0; i < k; i++)
        {
            if (d[i] >= T.Zero)
                continue;

            d[i] = -d[i];
            for (int row = 0; row < k; row++)
                u[row, i] = -u[row, i];
        }

        Array.Clear(e, 0, e.Length);
        return new SingularValueDecompositionResult<T>(u, d, v.Transpose());
    }

    private static bool IsNegligible<T>(T value, T a, T b, T eps)
        where T : struct, IFloatingPointIeee754<T>
        => T.Abs(value) <= eps * (T.Abs(a) + T.Abs(b));

    private static void WilkinsonQrStep<T>(
        T[] diagonal,
        T[] superdiagonal,
        Matrix<T> u,
        Matrix<T> v,
        int start,
        int end)
        where T : struct, IFloatingPointIeee754<T>
    {
        T a = diagonal[end - 1] * diagonal[end - 1];
        if (end - start > 1)
            a += superdiagonal[end - 2] * superdiagonal[end - 2];

        T b = diagonal[end] * superdiagonal[end - 1];
        T c = diagonal[end] * diagonal[end];
        T shift = WilkinsonShift(a, b, c);

        T x = (diagonal[start] * diagonal[start]) - shift;
        T y = diagonal[start] * superdiagonal[start];
        T z = T.Zero;

        for (int i = start; i < end; i++)
        {
            if (i != start)
            {
                x = superdiagonal[i - 1];
                y = diagonal[i];
                z = i < superdiagonal.Length ? superdiagonal[i] : T.Zero;
            }

            ComputeGivens(x, z, out T cosL, out T sinL);
            if (i > 0)
                superdiagonal[i - 1] = cosL * x - sinL * z;

            T temp = cosL * y + sinL * z;
            z = -sinL * y + cosL * z;
            y = temp;
            diagonal[i] = y;

            ApplyGivensLeft(u, k: u.Rows, i, i + 1, cosL, sinL);

            ComputeGivens(y, z, out T cosR, out T sinR);
            diagonal[i] = cosR * y + sinR * z;
            if (i < superdiagonal.Length)
            {
                y = -sinR * superdiagonal[i];
                superdiagonal[i] = cosR * superdiagonal[i];
            }

            ApplyGivensRight(v, cols: v.Columns, i, i + 1, cosR, sinR);
            x = diagonal[i + 1];
        }
    }

    private static T WilkinsonShift<T>(T a, T b, T c)
        where T : struct, IFloatingPointIeee754<T>
    {
        T delta = (a - c) * T.CreateChecked(0.5);
        T root = T.Sqrt(T.Max(T.Zero, delta * delta + b * b));
        T sign = delta >= T.Zero ? T.One : -T.One;
        return c - b * b / (delta + sign * root);
    }

    private static void ComputeGivens<T>(T a, T b, out T cos, out T sin)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (b == T.Zero)
        {
            cos = T.One;
            sin = T.Zero;
            return;
        }

        if (T.Abs(b) > T.Abs(a))
        {
            T t = -a / b;
            sin = T.One / T.Sqrt(T.One + t * t);
            cos = sin * t;
            return;
        }

        T r = b / a;
        cos = T.One / T.Sqrt(T.One + r * r);
        sin = cos * r;
    }

    private static void ApplyGivensLeft<T>(Matrix<T> u, int k, int colP, int colQ, T cos, T sin)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < k; i++)
        {
            T up = u[i, colP];
            T uq = u[i, colQ];
            u[i, colP] = cos * up - sin * uq;
            u[i, colQ] = sin * up + cos * uq;
        }
    }

    private static void ApplyGivensRight<T>(Matrix<T> v, int cols, int rowP, int rowQ, T cos, T sin)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int j = 0; j < cols; j++)
        {
            T vp = v[rowP, j];
            T vq = v[rowQ, j];
            v[rowP, j] = cos * vp - sin * vq;
            v[rowQ, j] = sin * vp + cos * vq;
        }
    }

    private static Matrix<T> CombineLeftTransform<T>(Matrix<T> left, Matrix<T> innerU, int m, int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        var result = new Matrix<T>(m, k);
        for (int i = 0; i < m; i++)
        {
            for (int l = 0; l < k; l++)
            {
                T sum = T.Zero;
                for (int p = 0; p < k; p++)
                    sum += left[i, p] * innerU[p, l];
                result[i, l] = sum;
            }
        }

        return result;
    }

    private static Matrix<T> CombineRightTransform<T>(Matrix<T> innerVt, Matrix<T> rightV, int k, int n)
        where T : struct, IFloatingPointIeee754<T>
    {
        var result = new Matrix<T>(k, n);
        for (int l = 0; l < k; l++)
        {
            for (int j = 0; j < n; j++)
            {
                T sum = T.Zero;
                for (int p = 0; p < k; p++)
                    sum += innerVt[l, p] * rightV[j, p];
                result[l, j] = sum;
            }
        }

        return result;
    }

    private static void SortSingularValuesDescending<T>(
        T[] singularValues,
        Matrix<T> u,
        Matrix<T> vt,
        int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < k - 1; i++)
        {
            int maxIndex = i;
            for (int j = i + 1; j < k; j++)
            {
                if (singularValues[j] > singularValues[maxIndex])
                    maxIndex = j;
            }

            if (maxIndex == i)
                continue;

            (singularValues[i], singularValues[maxIndex]) = (singularValues[maxIndex], singularValues[i]);
            SwapColumns(u, i, maxIndex, u.Rows);
            SwapRows(vt, i, maxIndex, vt.Columns);
        }
    }

    private static void SwapColumns<T>(Matrix<T> matrix, int colA, int colB, int rows)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int i = 0; i < rows; i++)
            (matrix[i, colA], matrix[i, colB]) = (matrix[i, colB], matrix[i, colA]);
    }

    private static void SwapRows<T>(Matrix<T> matrix, int rowA, int rowB, int cols)
        where T : struct, IFloatingPointIeee754<T>
    {
        for (int j = 0; j < cols; j++)
            (matrix[rowA, j], matrix[rowB, j]) = (matrix[rowB, j], matrix[rowA, j]);
    }
}
