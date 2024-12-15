using System.Text;

namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Represents 2D matrix.
/// </summary>
public class Matrix
{
    private readonly float[][] _matrix;

    /// <summary>
    /// Gets or sets number of rows.
    /// </summary>
    public int Rows { get; set; }

    /// <summary>
    /// Gets or sets number of columns.
    /// </summary>
    public int Columns { get; set; }

    /// <summary>
    /// Constructs <see cref="Matrix"/> with given number of <paramref name="rows"/> and <paramref name="columns"/>.
    /// </summary>
    public Matrix(int rows, int columns = 0)
    {
        if (columns == 0) columns = rows;

        Guard.AgainstNonPositive(rows, "Number of rows");
        Guard.AgainstNonPositive(columns, "Number of columns");

        _matrix = new float[rows][];

        for (var i = 0; i < rows; i++)
        {
            _matrix[i] = new float[columns];
        }

        Rows = rows;
        Columns = columns;
    }

  

    /// <summary>
    /// Gets reference to underlying 2D array.
    /// </summary>
    public float[][] As2dArray() => _matrix;

    /// <summary>
    /// Gets transposed matrix.
    /// </summary>
    public Matrix T
    {
        get
        {
            var transposed = new Matrix(Columns, Rows);

            for (var i = 0; i < Columns; i++)
            {
                for (var j = 0; j < Rows; j++)
                {
                    transposed[i][j] = _matrix[j][i];
                }
            }

            return transposed;
        }
    }

    /// <summary>
    /// Returns companion matrix.
    /// </summary>
    /// <param name="a">Input array</param>
    public static Matrix Companion(float[] a)
    {
        if (a.Length < 2)
        {
            throw new ArgumentException("The size of input array must be at least 2!");
        }

        if (Math.Abs(a[0]) < 1e-30)
        {
            throw new ArgumentException("The first coefficient must not be zero!");
        }

        var size = a.Length - 1;

        var companion = new Matrix(size);

        for (var i = 0; i < size; i++)
        {
            companion[0][i] = -a[i + 1] / a[0];
        }

        for (var i = 1; i < size; i++)
        {
            companion[i][i - 1] = 1;
        }

        return companion;
    }

    /// <summary>
    /// Returns identity matrix of given <paramref name="size"/>.
    /// </summary>
    public static Matrix Eye(int size)
    {
        var eye = new Matrix(size);

        for (var i = 0; i < size; i++)
        {
            eye[i][i] = 1;
        }

        return eye;
    }

    /// <summary>
    /// Returns sum of matrices <paramref name="m1"/> and <paramref name="m2"/>.
    /// </summary>
    public static Matrix operator +(Matrix m1, Matrix m2)
    {
        Guard.AgainstInequality(m1.Rows, m2.Rows, "Number of rows in first matrix", "number of rows in second matrix");
        Guard.AgainstInequality(m1.Columns, m2.Columns, "Number of columns in first matrix", "number of columns in second matrix");

        var result = new Matrix(m1.Rows, m1.Columns);

        for (var i = 0; i < m1.Rows; i++)
        {
            for (var j = 0; j < m1.Columns; j++)
            {
                result[i][j] = m1[i][j] + m2[i][j];
            }
        }

        return result;
    }

    /// <summary>
    /// Subtracts matrix <paramref name="m2"/> from matrix <paramref name="m1"/>.
    /// </summary>
    public static Matrix operator -(Matrix m1, Matrix m2)
    {
        Guard.AgainstInequality(m1.Rows, m2.Rows, "Number of rows in first matrix", "number of rows in second matrix");
        Guard.AgainstInequality(m1.Columns, m2.Columns, "Number of columns in first matrix", "number of columns in second matrix");

        var result = new Matrix(m1.Rows, m1.Columns);

        for (var i = 0; i < m1.Rows; i++)
        {
            for (var j = 0; j < m1.Columns; j++)
            {
                result[i][j] = m1[i][j] - m2[i][j];
            }
        }

        return result;
    }

    /// <summary>
    /// Multiplies matrix <paramref name="m1"/> by matrix <paramref name="m2"/>.
    /// </summary>
    public static Matrix operator *(Matrix m1, Matrix m2)
    {
        Guard.AgainstInequality(m1.Columns, m2.Rows, "Number of columns in first matrix", "number of rows in second matrix");

        var result = new Matrix(m1.Rows, m2.Columns);

        for (var i = 0; i < m1.Rows; i++)
        {
            for (var j = 0; j < m2.Columns; j++)
            {
                for (var k = 0; k < m1.Columns; k++)
                {
                    result[i][j] += m1[i][k] * m2[k][j];
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Multiplies matrix <paramref name="m"/> by scalar <paramref name="scalar"/>.
    /// </summary>
    public static Matrix operator *(Matrix m, float scalar)
    {
        var result = new Matrix(m.Rows, m.Columns);

        for (var i = 0; i < m.Rows; i++)
        {
            for (var j = 0; j < m.Columns; j++)
            {
                result[i][j] = m[i][j] * scalar;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets row by its index.
    /// </summary>
    /// <param name="i">Row index</param>
    public float[] this[int i] => _matrix[i];

    /// <summary>
    /// Calculates the determinant of the matrix.
    /// </summary>
    public float Determinant()
    {
        if (Rows != Columns)
        {
            throw new InvalidOperationException("Determinant can only be calculated for square matrices.");
        }

        return CalculateDeterminant(_matrix);
    }

    private float CalculateDeterminant(float[][] matrix)
    {
        int n = matrix.Length;

        if (n == 1)
        {
            return matrix[0][0];
        }

        if (n == 2)
        {
            return matrix[0][0] * matrix[1][1] - matrix[0][1] * matrix[1][0];
        }

        float determinant = 0;

        for (int p = 0; p < n; p++)
        {
            float[][] subMatrix = new float[n - 1][];

            for (int i = 0; i < n - 1; i++)
            {
                subMatrix[i] = new float[n - 1];
            }

            for (int i = 1; i < n; i++)
            {
                int subMatrixColumn = 0;

                for (int j = 0; j < n; j++)
                {
                    if (j == p)
                    {
                        continue;
                    }

                    subMatrix[i - 1][subMatrixColumn] = matrix[i][j];
                    subMatrixColumn++;
                }
            }

            determinant += (float)Math.Pow(-1, p) * matrix[0][p] * CalculateDeterminant(subMatrix);
        }

        return determinant;
    }

    /// <summary>
    /// Calculates the inverse of the matrix.
    /// </summary>
    public Matrix Inverse()
    {
        if (Rows != Columns)
        {
            throw new InvalidOperationException("Inverse can only be calculated for square matrices.");
        }

        float determinant = Determinant();

        if (Math.Abs(determinant) < 1e-30)
        {
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
        }

        float[][] adjoint = Adjoint(_matrix);
        Matrix inverse = new Matrix(Rows, Columns);

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                inverse[i][j] = adjoint[i][j] / determinant;
            }
        }

        return inverse;
    }

    private float[][] Adjoint(float[][] matrix)
    {
        int n = matrix.Length;
        float[][] adjoint = new float[n][];

        for (int i = 0; i < n; i++)
        {
            adjoint[i] = new float[n];
        }

        if (n == 1)
        {
            adjoint[0][0] = 1;
            return adjoint;
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                float[][] subMatrix = new float[n - 1][];

                for (int k = 0; k < n - 1; k++)
                {
                    subMatrix[k] = new float[n - 1];
                }

                int rowIndex = 0;

                for (int k = 0; k < n; k++)
                {
                    if (k == i)
                    {
                        continue;
                    }

                    int colIndex = 0;

                    for (int l = 0; l < n; l++)
                    {
                        if (l == j)
                        {
                            continue;
                        }

                        subMatrix[rowIndex][colIndex] = matrix[k][l];
                        colIndex++;
                    }

                    rowIndex++;
                }

                adjoint[j][i] = (float)Math.Pow(-1, i + j) * CalculateDeterminant(subMatrix);
            }
        }

        return adjoint;
    }

    /// <summary>
    /// Creates a deep copy of the matrix.
    /// </summary>
    public Matrix Clone()
    {
        Matrix clone = new Matrix(Rows, Columns);

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                clone[i][j] = _matrix[i][j];
            }
        }

        return clone;
    }

    /// <summary>
    /// Returns a string representation of the matrix.
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                sb.Append(_matrix[i][j].ToString("F2")).Append(" ");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
