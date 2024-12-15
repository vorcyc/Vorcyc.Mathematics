﻿namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Text;

/// <summary>
/// 表示一个二维矩阵。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
public class Matrix<T> where T : INumber<T>
{
    private T[,] _data;

    /// <summary>
    /// 获取矩阵的行数。
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// 获取矩阵的列数。
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// 使用指定的行数和列数构造一个矩阵。
    /// </summary>
    /// <param name="rows">行数。</param>
    /// <param name="columns">列数。</param>
    public Matrix(int rows, int columns )
    {

        Guard.AgainstNonPositive(rows, "Number of rows");
        Guard.AgainstNonPositive(columns, "Number of columns");

        Rows = rows;
        Columns = columns;
        _data = new T[rows, columns];
    }

    /// <summary>
    /// 使用 T[,] 数据构造一个矩阵。
    /// </summary>
    /// <param name="data">二维数组形式的数据。</param>
    public Matrix(T[,] data)
    {
        Rows = data.GetLength(0);
        Columns = data.GetLength(1);
        _data = new T[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                _data[i, j] = T.CreateChecked(data[i, j]);
            }
        }
    }

    /// <summary>
    /// 获取或设置指定位置的元素。
    /// </summary>
    /// <param name="row">行索引。</param>
    /// <param name="col">列索引。</param>
    /// <returns>指定位置的元素。</returns>
    public T this[int row, int col]
    {
        get { return _data[row, col]; }
        set { _data[row, col] = value; }
    }

    /// <summary>
    /// 隐式转换为 T[,]。
    /// </summary>
    /// <param name="matrix">要转换的矩阵。</param>
    public static implicit operator T[,](Matrix<T> matrix)
    {
        T[,] result = new T[matrix.Rows, matrix.Columns];
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i, j] = matrix[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// 矩阵加法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的和。</returns>
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

    /// <summary>
    /// 矩阵减法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的差。</returns>
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

    /// <summary>
    /// 矩阵乘法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的乘积。</returns>
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

    /// <summary>
    /// 矩阵乘法运算符，矩阵乘以标量。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="scalar">标量。</param>
    /// <returns>矩阵与标量的乘积。</returns>
    public static Matrix<T> operator *(Matrix<T> matrix, T scalar)
    {
        Matrix<T> result = new(matrix.Rows, matrix.Columns);
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i, j] = matrix[i, j] * scalar;
            }
        }
        return result;
    }

    /// <summary>
    /// 矩阵转置。
    /// </summary>
    /// <returns>转置后的矩阵。</returns>
    public Matrix<T> Transpose()
    {
        Matrix<T> result = new(Columns, Rows);
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result[j, i] = _data[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// 计算矩阵的行列式。
    /// </summary>
    /// <returns>矩阵的行列式。</returns>
    public T Determinant()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix must be square to calculate determinant.");

        return CalculateDeterminant(_data);
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

    /// <summary>
    /// 计算矩阵的逆矩阵。
    /// </summary>
    /// <returns>矩阵的逆矩阵。</returns>
    public Matrix<T> Inverse()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix must be square to calculate inverse.");

        T det = Determinant();

        if (T.Abs(det) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        Matrix<T> result = new(Rows, Columns);
        T[,] adjoint = Adjoint(_data);
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

    /// <summary>
    /// 创建矩阵的深拷贝。
    /// </summary>
    /// <returns>矩阵的深拷贝。</returns>
    public Matrix<T> Clone()
    {
        Matrix<T> clone = new(Rows, Columns);
        Array.Copy(_data, clone._data, _data.Length);
        return clone;
    }

    /// <summary>
    /// 返回矩阵的字符串表示形式。
    /// </summary>
    /// <returns>矩阵的字符串表示形式。</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                sb.AppendFormat("{0:0.##}\t", _data[i, j]);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
