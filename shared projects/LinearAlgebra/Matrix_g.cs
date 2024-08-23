namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;

public class Matrix<T>
    where T : INumber<T>
{
    private T[,] data;
    public int Rows { get; }
    public int Columns { get; }

    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        data = new T[rows, columns];
    }

    public T this[int row, int col]
    {
        get { return data[row, col]; }
        set { data[row, col] = value; }
    }

    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for addition.");

        Matrix<T> result = new(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < a.Columns; j++)
            {
                result[i, j] = a[i, j] + b[i, j];
            }
        }
        return result;
    }

    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for subtraction.");

        Matrix<T> result = new(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < a.Columns; j++)
            {
                result[i, j] = a[i, j] - b[i, j];
            }
        }
        return result;
    }

    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("Matrices must have compatible dimensions for multiplication.");

        Matrix<T> result = new(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < b.Columns; j++)
            {
                for (int k = 0; k < a.Columns; k++)
                {
                    result[i, j] += a[i, k] * b[k, j];
                }
            }
        }
        return result;
    }

    public Matrix<T> Transpose()
    {
        Matrix<T> result = new(Columns, Rows);
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result[j, i] = data[i, j];
            }
        }
        return result;
    }

    public T Determinant()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix must be square to calculate determinant.");

        return CalculateDeterminant(data);
    }

    private T CalculateDeterminant(T[,] matrix)
    {
        int n = matrix.GetLength(0);
        if (n == 1)
            return matrix[0, 0];
        if (n == 2)
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

        T det = T.Zero;
        for (int p = 0; p < n; p++)
        {
            T[,] subMatrix = new T[n - 1, n - 1];
            for (int i = 1; i < n; i++)
            {
                int subCol = 0;
                for (int j = 0; j < n; j++)
                {
                    if (j == p) continue;
                    subMatrix[i - 1, subCol] = matrix[i, j];
                    subCol++;
                }
            }
            det += matrix[0, p] * CalculateDeterminant(subMatrix) * (p % 2 == 0 ? T.One : -T.One);
        }
        return det;
    }

    public Matrix<T> Inverse()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix must be square to calculate inverse.");

        T det = Determinant();

        if (T.Abs(det) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        Matrix<T> result = new(Rows, Columns);
        T[,] adjoint = Adjoint(data);
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result[i, j] = adjoint[i, j] / det;
            }
        }
        return result;
    }

    private T[,] Adjoint(T[,] matrix)
    {
        int n = matrix.GetLength(0);
        T[,] adjoint = new T[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                T[,] subMatrix = new T[n - 1, n - 1];
                int subRow = 0;
                for (int row = 0; row < n; row++)
                {
                    if (row == i) continue;
                    int subCol = 0;
                    for (int col = 0; col < n; col++)
                    {
                        if (col == j) continue;
                        subMatrix[subRow, subCol] = matrix[row, col];
                        subCol++;
                    }
                    subRow++;
                }
                adjoint[j, i] = CalculateDeterminant(subMatrix) * ((i + j) % 2 == 0 ? T.One : -T.One);
            }
        }
        return adjoint;
    }

    public Matrix<T> Clone()
    {
        Matrix<T> clone = new(Rows, Columns);
        Array.Copy(data, clone.data, data.Length);
        return clone;
    }

    public override string ToString()
    {
        string result = "";
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result += $"{data[i, j]:0.##}\t";
            }
            result += "\n";
        }
        return result;
    }




}
