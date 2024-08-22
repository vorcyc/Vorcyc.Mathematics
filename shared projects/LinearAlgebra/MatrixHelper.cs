/*
 *  Based on the Accord.NET Framework project.
 */

namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Collections;
using System.Runtime.CompilerServices;

public static class MatrixHelper
{

    internal static int GetLength<T>(T[][] values, int dimension)
    {
        if (dimension == 1)
            return values.Length;
        return values[0].Length;
    }

    internal static int GetLength<T>(T[,] values, int dimension)
    {
        if (dimension == 1)
            return values.GetLength(0);
        return values.GetLength(1);
    }


    /// <summary>
    ///   Gets the total length over all dimensions of an array.
    /// </summary>
    /// 
    public static int GetTotalLength(this Array array, bool deep = true, bool rectangular = true)
    {
        if (deep && IsJagged(array))
        {
            if (rectangular)
            {
                int rest = GetTotalLength(array.GetValue(0) as Array, deep);
                return array.Length * rest;
            }
            else
            {
                int sum = 0;
                for (int i = 0; i < array.Length; i++)
                    sum += GetTotalLength(array.GetValue(i) as Array, deep);
                return sum;
            }
        }

        return array.Length;
    }


    /// <summary>
    ///   Gets the length of each dimension of an array.
    /// </summary>
    /// 
    /// <param name="array">The array.</param>
    /// <param name="deep">Pass true to retrieve all dimensions of the array,
    ///   even if it contains nested arrays (as in jagged matrices)</param>
    /// <param name="max">Gets the maximum length possible for each dimension (in case
    ///   the jagged matrices has different lengths).</param>
    /// 
    public static int[] GetLength(this Array array, bool deep = true, bool max = false)
    {
        if (array.Rank == 0)
            return new int[0];

        if (deep && IsJagged(array))
        {
            if (array.Length == 0)
                return new int[0];

            int[] rest;
            if (!max)
            {
                rest = GetLength(array.GetValue(0) as Array, deep);
            }
            else
            {
                // find the max
                rest = GetLength(array.GetValue(0) as Array, deep);
                for (int i = 1; i < array.Length; i++)
                {
                    int[] r = GetLength(array.GetValue(i) as Array, deep);

                    for (int j = 0; j < r.Length; j++)
                    {
                        if (r[j] > rest[j])
                            rest[j] = r[j];
                    }
                }
            }

            return array.Length.Concatenate(rest);
        }

        int[] vector = new int[array.Rank];
        for (int i = 0; i < vector.Length; i++)
            vector[i] = array.GetUpperBound(i) + 1;
        return vector;
    }


    /// <summary>
    ///   Determines whether an array is a jagged array 
    ///   (containing inner arrays as its elements).
    /// </summary>
    /// 
    public static bool IsJagged(this Array array)
    {
        if (array.Length == 0)
            return array.Rank == 1;
        return array.Rank == 1 && array.GetValue(0) is Array;
    }

    /// <summary>
    ///   Determines whether an array is an multidimensional array.
    /// </summary>
    /// 
    public static bool IsMatrix(this Array array)
    {
        return array.Rank > 1;
    }

    /// <summary>
    ///   Determines whether an array is a vector.
    /// </summary>
    /// 
    public static bool IsVector(this Array array)
    {
        return array.Rank == 1 && !IsJagged(array);
    }


    #region Combine
    /// <summary>
    ///   Combines two vectors horizontally.
    /// </summary>
    /// 
    public static T[] Concatenate<T>(this T[] a, params T[] b)
    {
        T[] r = new T[a.Length + b.Length];
        for (int i = 0; i < a.Length; i++)
            r[i] = a[i];
        for (int i = 0; i < b.Length; i++)
            r[i + a.Length] = b[i];

        return r;
    }

    /// <summary>
    ///   Combines a vector and a element horizontally.
    /// </summary>
    /// 
    public static T[] Concatenate<T>(this T[] vector, T element)
    {
        T[] r = new T[vector.Length + 1];
        for (int i = 0; i < vector.Length; i++)
            r[i] = vector[i];

        r[vector.Length] = element;

        return r;
    }

    /// <summary>
    ///   Combines a vector and a element horizontally.
    /// </summary>
    /// 
    public static T[] Concatenate<T>(this T element, T[] vector)
    {
        T[] r = new T[vector.Length + 1];

        r[0] = element;

        for (int i = 0; i < vector.Length; i++)
            r[i + 1] = vector[i];

        return r;
    }

    ///// <summary>
    /////   Combines a matrix and a vector horizontally.
    ///// </summary>
    ///// 
    //public static T[,] Concatenate<T>(this T[,] matrix, T[] vector)
    //{
    //    return matrix.InsertColumn(vector);
    //}

    /// <summary>
    ///   Combines two matrices horizontally.
    /// </summary>
    /// 
    public static T[,] Concatenate<T>(this T[,] a, T[,] b)
    {
        return Concatenate(new[] { a, b });
    }

    /// <summary>
    ///   Combines two matrices horizontally.
    /// </summary>
    /// 
    public static T[][] Concatenate<T>(this T[][] a, T[][] b)
    {
        return Concatenate(new[] { a, b });
    }

    /// <summary>
    ///   Combines a matrix and a vector horizontally.
    /// </summary>
    /// 
    public static T[,] Concatenate<T>(params T[][,] matrices)
    {
        int rows = 0;
        int cols = 0;

        for (int i = 0; i < matrices.Length; i++)
        {
            cols += matrices[i].GetLength(1);
            if (matrices[i].GetLength(0) > rows)
                rows = matrices[i].GetLength(0);
        }

        T[,] r = new T[rows, cols];


        int c = 0;
        for (int k = 0; k < matrices.Length; k++)
        {
            int currentRows = matrices[k].GetLength(0);
            int currentCols = matrices[k].GetLength(1);

            for (int j = 0; j < currentCols; j++)
            {
                for (int i = 0; i < currentRows; i++)
                {
                    r[i, c] = matrices[k][i, j];
                }
                c++;
            }
        }

        return r;
    }

    /// <summary>
    ///   Combines a matrix and a vector horizontally.
    /// </summary>
    /// 
    public static T[][] Concatenate<T>(params T[][][] matrices)
    {
        int rows = 0;
        int cols = 0;

        for (int i = 0; i < matrices.Length; i++)
        {
            cols += matrices[i][0].Length;
            if (matrices[i].Length > rows)
                rows = matrices[i].Length;
        }

        T[][] r = new T[rows][];
        for (int i = 0; i < r.Length; i++)
            r[i] = new T[cols];


        int c = 0;
        for (int k = 0; k < matrices.Length; k++)
        {
            int currentRows = matrices[k].Length;
            int currentCols = matrices[k][0].Length;

            for (int j = 0; j < currentCols; j++)
            {
                for (int i = 0; i < currentRows; i++)
                {
                    r[i][c] = matrices[k][i][j];
                }
                c++;
            }
        }

        return r;
    }

    /// <summary>
    ///   Combine vectors horizontally.
    /// </summary>
    /// 
    public static T[] Concatenate<T>(this T[][] vectors)
    {
        int size = 0;
        for (int i = 0; i < vectors.Length; i++)
            size += vectors[i].Length;

        T[] r = new T[size];

        int c = 0;
        for (int i = 0; i < vectors.Length; i++)
            for (int j = 0; j < vectors[i].Length; j++)
                r[c++] = vectors[i][j];

        return r;
    }

    ///// <summary>
    /////   Combines vectors vertically.
    ///// </summary>
    ///// 
    //public static T[,] Stack<T>(this T[] a, T[] b)
    //{
    //    return Stack(new[] { a, b });
    //}

    /// <summary>
    ///   Combines vectors vertically.
    /// </summary>
    /// 
    public static T[][] Stack<T>(this T[][] a, T[][] b)
    {
        return Stack(new T[][][] { a, b });
    }

    ///// <summary>
    /////   Combines vectors vertically.
    ///// </summary>
    ///// 
    //public static T[,] Stack<T>(params T[][] vectors)
    //{
    //    return vectors.ToMatrix();
    //}

    /// <summary>
    ///   Combines vectors vertically.
    /// </summary>
    /// 
    public static T[,] Stack<T>(params T[] elements)
    {
        return elements.Transpose();
    }

    /// <summary>
    ///   Combines vectors vertically.
    /// </summary>
    /// 
    public static T[,] Stack<T>(this T[] vector, T element)
    {
        return vector.Concatenate(element).Transpose();
    }

    /// <summary>
    ///   Combines matrices vertically.
    /// </summary>
    /// 
    public static T[,] Stack<T>(params T[][,] matrices)
    {
        int rows = 0;
        int cols = 0;

        for (int i = 0; i < matrices.Length; i++)
        {
            rows += matrices[i].GetLength(0);
            if (matrices[i].GetLength(1) > cols)
                cols = matrices[i].GetLength(1);
        }

        T[,] r = new T[rows, cols];

        int c = 0;
        for (int i = 0; i < matrices.Length; i++)
        {
            for (int j = 0; j < matrices[i].GetLength(0); j++)
            {
                for (int k = 0; k < matrices[i].GetLength(1); k++)
                    r[c, k] = matrices[i][j, k];
                c++;
            }
        }

        return r;
    }

    /// <summary>
    ///   Combines matrices vertically.
    /// </summary>
    /// 
    public static T[,] Stack<T>(this T[,] matrix, T[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        T[,] r = new T[rows + 1, cols];

        Array.Copy(matrix, r, matrix.Length);

        for (int i = 0; i < vector.Length; i++)
            r[rows, i] = vector[i];

        return r;
    }

    /// <summary>
    ///   Combines matrices vertically.
    /// </summary>
    public static T[][] Stack<T>(params T[][][] matrices)
    {
        int rows = 0;
        int cols = 0;

        for (int i = 0; i < matrices.Length; i++)
        {
            rows += matrices[i].Length;
            if (matrices[i].Length == 0)
                continue;

            if (matrices[i][0].Length > cols)
                cols = matrices[i][0].Length;
        }

        T[][] r = new T[rows][];
        for (int i = 0; i < rows; i++)
            r[i] = new T[cols];

        int c = 0;
        for (int i = 0; i < matrices.Length; i++)
        {
            for (int j = 0; j < matrices[i].Length; j++)
            {
                for (int k = 0; k < matrices[i][j].Length; k++)
                    r[c][k] = matrices[i][j][k];
                c++;
            }
        }

        return r;
    }
    #endregion

    #region Transpose

    /// <summary>
    ///   Gets the transpose of a matrix.
    /// </summary>
    /// 
    /// <param name="matrix">A matrix.</param>
    /// 
    /// <returns>The transpose of the given matrix.</returns>
    /// 
    public static T[,] Transpose<T>(this T[,] matrix)
    {
        return Transpose(matrix, false);
    }

    /// <summary>
    ///   Gets the transpose of a matrix.
    /// </summary>
    /// 
    /// <param name="matrix">A matrix.</param>
    /// 
    /// <param name="inPlace">True to store the transpose over the same input
    ///   <paramref name="matrix"/>, false otherwise. Default is false.</param>
    ///   
    /// <returns>The transpose of the given matrix.</returns>
    /// 
    public static T[,] Transpose<T>(this T[,] matrix, bool inPlace)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        if (inPlace)
        {
            if (rows != cols)
                throw new ArgumentException("Only square matrices can be transposed in place.", "matrix");

#if DEBUG
            T[,] expected = matrix.Transpose();
#endif

            for (int i = 0; i < rows; i++)
            {
                for (int j = i; j < cols; j++)
                {
                    T element = matrix[j, i];
                    matrix[j, i] = matrix[i, j];
                    matrix[i, j] = element;
                }
            }

            //#if DEBUG
            //            if (!expected.IsEqual(matrix))
            //                throw new Exception();
            //#endif

            return matrix;
        }
        else
        {
            T[,] result = new T[cols, rows];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    result[j, i] = matrix[i, j];

            return result;
        }
    }



    /// <summary>
    ///   Gets the transpose of a row vector.
    /// </summary>
    /// 
    /// <param name="rowVector">A row vector.</param>
    /// 
    /// <returns>The transpose of the given vector.</returns>
    /// 
    public static T[,] Transpose<T>(this T[] rowVector)
    {
        var result = new T[rowVector.Length, 1];
        for (int i = 0; i < rowVector.Length; i++)
            result[i, 0] = rowVector[i];
        return result;
    }

    /// <summary>
    ///   Gets the transpose of a row vector.
    /// </summary>
    /// 
    /// <param name="rowVector">A row vector.</param>
    /// <param name="result">The matrix where to store the transpose.</param>
    /// 
    /// <returns>The transpose of the given vector.</returns>
    /// 
    public static T[,] Transpose<T>(this T[] rowVector, T[,] result)
    {
        for (int i = 0; i < rowVector.Length; i++)
            result[i, 0] = rowVector[i];
        return result;
    }


    /// <summary>
    ///   Gets the generalized transpose of a tensor.
    /// </summary>
    /// 
    /// <param name="array">A tensor.</param>
    /// <param name="order">The new order for the tensor's dimensions.</param>
    /// 
    /// <returns>The transpose of the given tensor.</returns>
    /// 
    public static Array Transpose(this Array array, int[] order)
    {
        return transpose(array, order);
    }

    /// <summary>
    ///   Gets the generalized transpose of a tensor.
    /// </summary>
    /// 
    /// <param name="array">A tensor.</param>
    /// <param name="order">The new order for the tensor's dimensions.</param>
    /// 
    /// <returns>The transpose of the given tensor.</returns>
    /// 
    public static T Transpose<T>(this T array, int[] order)
        where T : class, IList
    {
        Array arr = array as Array;

        if (arr == null)
            throw new ArgumentException("The given object must inherit from System.Array.", "array");

        return transpose(arr, order) as T;
    }

    private static Array transpose(Array array, int[] order)
    {
        if (array.Length == 1 || array.Length == 0)
            return array;

        // Get the number of samples at each dimension
        int[] size = new int[array.Rank];
        for (int i = 0; i < size.Length; i++)
            size[i] = array.GetLength(i);

        Array r = Array.CreateInstance(array.GetType().GetElementType(), size.Get(order));

        // Generate all indices for accessing the matrix 
        foreach (int[] pos in Combinatorics.Sequences(size, true))
        {
            int[] newPos = pos.Get(order);
            object value = array.GetValue(pos);
            r.SetValue(value, newPos);
        }

        return r;
    }

    #endregion



    #region Get
    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="startRow">Start row index</param>
    /// <param name="endRow">End row index</param>
    /// <param name="startColumn">Start column index</param>
    /// <param name="endColumn">End column index</param>
    /// 
    public static T[,] Get<T>(this T[,] source,
        int startRow, int endRow, int startColumn, int endColumn)
    {
        return get(source, null, startRow, endRow, startColumn, endColumn);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="destination">The matrix where results should be stored.</param>
    /// <param name="startRow">Start row index</param>
    /// <param name="endRow">End row index</param>
    /// <param name="startColumn">Start column index</param>
    /// <param name="endColumn">End column index</param>
    /// 
    public static T[,] Get<T>(this T[,] source, T[,] destination,
        int startRow, int endRow, int startColumn, int endColumn)
    {
        if (destination == null)
            throw new ArgumentNullException("destination");

        int rows = source.Rows();
        int cols = source.Columns();

        startRow = index(startRow, rows);
        startColumn = index(startColumn, cols);

        endRow = end(endRow, rows);
        endColumn = end(endColumn, cols);

        if (destination.GetLength(0) < endRow - startRow)
            throw new DimensionMismatchException("destination",
                "The destination matrix must be big enough to accommodate the results.");

        if (destination.GetLength(1) < endColumn - startColumn)
            throw new DimensionMismatchException("destination",
                "The destination matrix must be big enough to accommodate the results.");

        return get(source, destination, startRow, endRow, startColumn, endColumn);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="rowIndexes">Array of row indices. Pass null to select all indices.</param>
    /// <param name="columnIndexes">Array of column indices. Pass null to select all indices.</param>
    /// <param name="result">An optional matrix where the results should be stored.</param>
    /// 
    public static T[,] Get<T>(this T[,] source, int[] rowIndexes, int[] columnIndexes, T[,] result = null)
    {
        return get(source, result, rowIndexes, columnIndexes);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="rowMask">Array of row indicators. Pass null to select all indices.</param>
    /// <param name="columnMask">Array of column indicators. Pass null to select all indices.</param>
    /// <param name="result">An optional matrix where the results should be stored.</param>
    /// 
    public static T[,] Get<T>(this T[,] source, bool[] rowMask, bool[] columnMask, T[,] result = null)
    {
        return get(source, result, rowMask.Find(x => x), columnMask.Find(x => x));
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="destination">The matrix where results should be stored.</param>
    /// <param name="rowIndexes">Array of row indices. Pass null to select all indices.</param>
    /// <param name="columnIndexes">Array of column indices. Pass null to select all indices.</param>
    /// 
    public static T[,] Get<T>(this T[,] source, T[,] destination, int[] rowIndexes, int[] columnIndexes)
    {
        if (destination == null)
            throw new ArgumentNullException("destination");

        return get(source, destination, rowIndexes, columnIndexes);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="rowIndexes">Array of row indices</param>
    /// 
    public static T[,] Get<T>(this T[,] source, int[] rowIndexes)
    {
        return get(source, null, rowIndexes, null);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="startRow">Starting row index</param>
    /// <param name="endRow">End row index</param>
    /// <param name="columnIndexes">Array of column indices</param>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    public static T[,] Get<T>(this T[,] source, int startRow, int endRow, int[] columnIndexes)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        int rows = source.Rows();
        int cols = source.Columns();

        startRow = index(startRow, rows);
        endRow = end(endRow, rows);

        int newRows = endRow - startRow;
        int newCols = cols;

        if ((startRow > endRow) || (startRow < 0) || (startRow > rows) || (endRow < 0) || (endRow > rows))
        {
            throw new ArgumentException("Argument out of range.");
        }

        T[,] destination;

        if (columnIndexes != null)
        {
            newCols = columnIndexes.Length;
            for (int j = 0; j < columnIndexes.Length; j++)
                if ((columnIndexes[j] < 0) || (columnIndexes[j] >= cols))
                    throw new ArgumentException("Argument out of range.");

            destination = new T[newRows, newCols];

            for (int i = startRow; i < endRow; i++)
                for (int j = 0; j < columnIndexes.Length; j++)
                    destination[i - startRow, j] = source[i, columnIndexes[j]];
        }
        else
        {
            if (startRow == 0 && endRow == rows)
                return source;

            destination = new T[newRows, newCols];

            for (int i = startRow; i < endRow; i++)
                for (int j = 0; j < newCols; j++)
                    destination[i - startRow, j] = source[i, j];
        }

        return destination;
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="rowIndexes">Array of row indices</param>
    /// <param name="startColumn">Start column index</param>
    /// <param name="endColumn">End column index</param>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    public static T[,] Get<T>(this T[,] source, int[] rowIndexes, int startColumn, int endColumn)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        int rows = source.Rows();
        int cols = source.Columns();

        startColumn = index(startColumn, cols);
        endColumn = end(endColumn, cols);

        int newRows = rows;
        int newCols = endColumn - startColumn;

        if ((startColumn > endColumn) || (startColumn < 0) || (startColumn > cols) || (endColumn < 0) || (endColumn > cols))
        {
            throw new ArgumentException("Argument out of range.");
        }

        T[,] destination;

        if (rowIndexes != null)
        {
            newRows = rowIndexes.Length;
            for (int j = 0; j < rowIndexes.Length; j++)
                if ((rowIndexes[j] < 0) || (rowIndexes[j] >= rows))
                    throw new ArgumentException("Argument out of range.");

            destination = new T[newRows, newCols];

            for (int i = 0; i < rowIndexes.Length; i++)
                for (int j = startColumn; j < endColumn; j++)
                    destination[i, j - startColumn] = source[rowIndexes[i], j];
        }
        else
        {
            if (startColumn == 0 && endColumn == cols)
                return source;

            destination = new T[newRows, newCols];

            for (int i = 0; i < newRows; i++)
                for (int j = startColumn; j < endColumn; j++)
                    destination[i, j - startColumn] = source[i, j];
        }

        return destination;
    }

    private static int end(int end, int length)
    {
        if (end <= 0)
            end = length + end;
        return end;
    }

    private static int index(int end, int length)
    {
        if (end < 0)
            end = length + end;
        return end;
    }





    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="startRow">Start row index</param>
    /// <param name="endRow">End row index</param>
    /// <param name="startColumn">Start column index</param>
    /// <param name="endColumn">End column index</param>
    /// 
    public static T[][] Get<T>(this T[][] source,
        int startRow, int endRow, int startColumn, int endColumn)
    {
        return get(source, null, startRow, endRow, startColumn, endColumn, false);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="destination">The matrix where the values should be stored.</param>
    /// <param name="values">The values to be stored.</param>
    /// <param name="startRow">Start row index in the destination matrix.</param>
    /// <param name="endRow">End row index in the destination matrix.</param>
    /// <param name="startColumn">Start column index in the destination matrix.</param>
    /// <param name="endColumn">End column index in the destination matrix.</param>
    /// 
    public static T[][] Set<T>(this T[][] destination,
        int startRow, int endRow, int startColumn, int endColumn, T[][] values)
    {
        return set(destination, values, startRow, endRow, startColumn, endColumn);
    }

    /// <summary>
    ///   Sets elements from a matrix to a given value.
    /// </summary>
    /// 
    /// <param name="values">The matrix of values to be changed.</param>
    /// <param name="match">The function used to determine whether an 
    ///   element in the matrix should be changed or not.</param>
    /// <param name="value">The values to set the elements to.</param>
    /// 
    public static T[][] Set<T>(this T[][] values, Func<T, bool> match, T value)
    {
        for (int i = 0; i < values.Length; i++)
        {
            for (int j = 0; j < values[i].Length; j++)
            {
                if (match(values[i][j]))
                    values[i][j] = value;
            }
        }

        return values;
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="rowIndexes">Array of row indices. Pass null to select all indices.</param>
    /// <param name="columnIndexes">Array of column indices. Pass null to select all indices.</param>
    /// <param name="reuseMemory">Set to true to avoid memory allocations 
    ///   when possible. This might result on the shallow copies of some
    ///   elements. Default is false (default is to always provide a true,
    ///   deep copy of every element in the matrices, using more memory).</param>
    /// <param name="result">An optional matrix where the results should be stored.</param>
    /// 
    public static T[][] Get<T>(this T[][] source,
        int[] rowIndexes, int[] columnIndexes, bool reuseMemory = false, T[][] result = null)
    {
        return get(source, result, rowIndexes, columnIndexes, reuseMemory);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="rowMask">Array of row indicators. Pass null to select all indices.</param>
    /// <param name="columnMask">Array of column indicators. Pass null to select all indices.</param>
    /// <param name="reuseMemory">Set to true to avoid memory allocations 
    ///   when possible. This might result on the shallow copies of some
    ///   elements. Default is false (default is to always provide a true,
    ///   deep copy of every element in the matrices, using more memory).</param>
    /// <param name="result">An optional matrix where the results should be stored.</param>
    /// 
    public static T[][] Get<T>(this T[][] source,
        bool[] rowMask, bool[] columnMask, bool reuseMemory = false, T[][] result = null)
    {
        return get(source, result, rowMask.Find(x => x), columnMask.Find(x => x), reuseMemory);
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="indexes">Array of indices.</param>
    /// <param name="transpose">True to return a transposed matrix; false otherwise.</param>
    /// 
    public static T[][] Get<T>(this T[][] source, int[] indexes, bool transpose = false)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        if (indexes == null)
            throw new ArgumentNullException("indexes");

        int rows = source.Length;
        if (rows == 0) return new T[0][];
        int cols = source[0].Length;

        T[][] destination;

        if (transpose)
        {
            destination = new T[cols][];

            for (int j = 0; j < destination.Length; j++)
            {
                destination[j] = new T[indexes.Length];
                for (int i = 0; i < indexes.Length; i++)
                    destination[j][i] = source[indexes[i]][j];
            }
        }
        else
        {
            destination = new T[indexes.Length][];

            for (int i = 0; i < indexes.Length; i++)
                destination[i] = source[indexes[i]];
        }

        return destination;
    }


    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="rowIndexes">Array of row indices</param>
    /// <param name="startColumn">Start column index</param>
    /// <param name="endColumn">End column index</param>
    /// <param name="reuseMemory">Set to true to avoid memory allocations 
    ///   when possible. This might result on the shallow copies of some
    ///   elements. Default is false (default is to always provide a true,
    ///   deep copy of every element in the matrices, using more memory).</param>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    public static T[][] Get<T>(this T[][] source, int[] rowIndexes,
        int startColumn, int endColumn, bool reuseMemory = false)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        int rows = source.Length;
        if (rows == 0)
            return new T[0][];
        int cols = source[0].Length;

        startColumn = index(startColumn, cols);
        endColumn = end(endColumn, cols);

        int newRows = rows;
        int newCols = endColumn - startColumn;

        if ((startColumn > endColumn) || (startColumn < 0) || (startColumn > cols) || (endColumn < 0) || (endColumn > cols))
        {
            throw new ArgumentException("Argument out of range.");
        }

        T[][] destination;

        bool canReuseMemory = startColumn == 0 && endColumn == cols;

        if (rowIndexes != null)
        {
            newRows = rowIndexes.Length;
            for (int j = 0; j < rowIndexes.Length; j++)
                if ((rowIndexes[j] < 0) || (rowIndexes[j] >= rows))
                    throw new ArgumentException("Argument out of range.");

            destination = new T[newRows][];

            if (canReuseMemory && reuseMemory)
            {
                for (int i = 0; i < rowIndexes.Length; i++)
                    destination[i] = source[rowIndexes[i]];
            }
            else
            {
                for (int i = 0; i < rowIndexes.Length; i++)
                {
                    var row = destination[i] = new T[newCols];
                    for (int j = startColumn; j < endColumn; j++)
                        row[j - startColumn] = source[rowIndexes[i]][j];
                }
            }
        }
        else
        {
            if (startColumn == 0 && endColumn == cols)
                return source;

            destination = new T[newRows][];

            for (int i = 0; i < destination.Length; i++)
            {
                var row = destination[i] = new T[newCols];
                for (int j = startColumn; j < endColumn; j++)
                    row[j - startColumn] = source[i][j];
            }
        }

        return destination;
    }

    /// <summary>
    ///   Returns a sub matrix extracted from the current matrix.
    /// </summary>
    /// 
    /// <param name="source">The matrix to return the submatrix from.</param>
    /// <param name="startRow">Starting row index</param>
    /// <param name="endRow">End row index</param>
    /// <param name="columnIndexes">Array of column indices</param>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    public static T[][] Get<T>(this T[][] source, int startRow, int endRow, int[] columnIndexes)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        int rows = source.Length;
        if (rows == 0)
            return new T[0][];
        int cols = source[0].Length;

        startRow = index(startRow, rows);
        endRow = end(endRow, rows);

        int newRows = endRow - startRow;
        int newCols = cols;

        if ((startRow > endRow) || (startRow < 0) || (startRow > rows) || (endRow < 0) || (endRow > rows))
        {
            throw new ArgumentException("Argument out of range");
        }


        T[][] destination;

        if (columnIndexes != null)
        {
            newCols = columnIndexes.Length;
            for (int j = 0; j < columnIndexes.Length; j++)
                if ((columnIndexes[j] < 0) || (columnIndexes[j] >= cols))
                    throw new ArgumentException("Argument out of range.");

            destination = new T[newRows][];
            for (int i = 0; i < destination.Length; i++)
                destination[i] = new T[newCols];

            for (int i = startRow; i < endRow; i++)
            {
                for (int j = 0; j < columnIndexes.Length; j++)
                    destination[i - startRow][j] = source[i][columnIndexes[j]];
            }
        }
        else
        {
            if (startRow == 0 && endRow == rows)
                return source;

            destination = new T[newRows][];
            for (int i = 0; i < destination.Length; i++)
                destination[i] = new T[newCols];

            for (int i = startRow; i < endRow; i++)
                for (int j = 0; j < newCols; j++)
                    destination[i - startRow][j] = source[i][j];
        }

        return destination;
    }




    /// <summary>
    ///   Returns a subvector extracted from the current vector.
    /// </summary>
    /// 
    /// <param name="source">The vector to return the subvector from.</param>
    /// <param name="indexes">Array of indices.</param>
    /// <param name="inPlace">True to return the results in place, changing the
    ///   original <paramref name="source"/> vector; false otherwise.</param>
    /// 
    public static T[] Get<T>(this T[] source, int[] indexes, bool inPlace = false)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        if (indexes == null)
            throw new ArgumentNullException("indexes");

        if (inPlace && source.Length != indexes.Length)
            throw new DimensionMismatchException("Source and indexes arrays must have the same dimension for in-place operations.");

        var destination = new T[indexes.Length];
        for (int i = 0; i < indexes.Length; i++)
        {
            int j = indexes[i];
            if (j >= 0)
                destination[i] = source[j];
            else
                destination[i] = source[source.Length + j];
        }

        if (inPlace)
        {
            for (int i = 0; i < destination.Length; i++)
                source[i] = destination[i];
        }

        return destination;
    }

    /// <summary>
    ///   Returns a subvector extracted from the current vector.
    /// </summary>
    /// 
    /// <param name="source">The vector to return the subvector from.</param>
    /// <param name="indexes">Array of indices.</param>
    /// 
    public static T[] Get<T>(this T[] source, IList<int> indexes)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        if (indexes == null)
            throw new ArgumentNullException("indexes");

        var destination = new T[indexes.Count];

        int i = 0;
        foreach (var j in indexes)
            destination[i++] = source[j];

        return destination;
    }

    /// <summary>
    ///   Returns a subvector extracted from the current vector.
    /// </summary>
    /// 
    /// <param name="source">The vector to return the subvector from.</param>
    /// <param name="startRow">Starting index.</param>
    /// <param name="endRow">End index.</param>
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    public static T[] Get<T>(this T[] source, int startRow, int endRow)
    {
        startRow = index(startRow, source.Length);
        endRow = end(endRow, source.Length);

        var destination = new T[endRow - startRow];
        for (int i = startRow; i < endRow; i++)
            destination[i - startRow] = source[i];
        return destination;
    }

    /// <summary>
    ///   Returns a value extracted from the current vector.
    /// </summary>
    /// 
    public static T Get<T>(this T[] source, int index)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        if (index >= source.Length)
            throw new ArgumentOutOfRangeException("index");

        index = MatrixHelper.index(index, source.Length);

        return source[index];
    }



    /// <summary>
    ///   Returns a subvector extracted from the current vector.
    /// </summary>
    /// 
    /// <param name="source">The vector to return the subvector from.</param>
    /// <param name="indexes">Array of indices.</param>
    /// 
    public static List<T> Get<T>(this List<T> source, int[] indexes)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        if (indexes == null)
            throw new ArgumentNullException("indexes");

        var destination = new List<T>();
        for (int i = 0; i < indexes.Length; i++)
            destination.Add(source[indexes[i]]);

        return destination;
    }




    /// <summary>
    ///   Extracts a selected area from a matrix.
    /// </summary>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    private static T[,] get<T>(this T[,] source, T[,] destination,
        int startRow, int endRow, int startColumn, int endColumn)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        int rows = source.Rows();
        int cols = source.Columns();

        startRow = index(startRow, rows);
        startColumn = index(startColumn, cols);

        endRow = end(endRow, rows);
        endColumn = end(endColumn, cols);

        if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) ||
            (startRow > rows) || (endRow < 0) || (endRow > rows) ||
            (startColumn < 0) || (startColumn > cols) || (endColumn < 0) ||
            (endColumn > cols))
        {
            throw new ArgumentException("Argument out of range.");
        }

        if (destination == null)
            destination = new T[endRow - startRow, endColumn - startColumn];

        for (int i = startRow; i < endRow; i++)
            for (int j = startColumn; j < endColumn; j++)
                destination[i - startRow, j - startColumn] = source[i, j];

        return destination;
    }

    /// <summary>
    ///   Extracts a selected area from a matrix.
    /// </summary>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    private static T[,] get<T>(this T[,] source, T[,] destination, int[] rowIndexes, int[] columnIndexes)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        int rows = source.GetLength(0);
        int cols = source.GetLength(1);

        int newRows = rows;
        int newCols = cols;

        if (rowIndexes == null && columnIndexes == null)
        {
            return source;
        }

        if (rowIndexes != null)
        {
            newRows = rowIndexes.Length;
            for (int i = 0; i < rowIndexes.Length; i++)
                if ((rowIndexes[i] < 0) || (rowIndexes[i] >= rows))
                    throw new ArgumentException("Argument out of range.");
        }
        if (columnIndexes != null)
        {
            newCols = columnIndexes.Length;
            for (int i = 0; i < columnIndexes.Length; i++)
                if ((columnIndexes[i] < 0) || (columnIndexes[i] >= cols))
                    throw new ArgumentException("Argument out of range.");
        }


        if (destination != null)
        {
            if (destination.GetLength(0) < newRows || destination.GetLength(1) < newCols)
                throw new DimensionMismatchException("destination",
                "The destination matrix must be big enough to accommodate the results.");
        }
        else
        {
            destination = new T[newRows, newCols];
        }

        if (columnIndexes == null)
        {
            for (int i = 0; i < rowIndexes.Length; i++)
                for (int j = 0; j < cols; j++)
                    destination[i, j] = source[rowIndexes[i], j];
        }
        else if (rowIndexes == null)
        {
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columnIndexes.Length; j++)
                    destination[i, j] = source[i, columnIndexes[j]];
        }
        else
        {
            for (int i = 0; i < rowIndexes.Length; i++)
                for (int j = 0; j < columnIndexes.Length; j++)
                    destination[i, j] = source[rowIndexes[i], columnIndexes[j]];
        }

        return destination;
    }

    /// <summary>
    ///   Extracts a selected area from a matrix.
    /// </summary>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    private static T[][] get<T>(this T[][] source, T[][] destination,
        int[] rowIndexes, int[] columnIndexes, bool reuseMemory)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        if (source.Length == 0)
            return new T[0][];

        int rows = source.Length;
        int cols = source[0].Length;

        int newRows = rows;
        int newCols = cols;

        if (rowIndexes == null && columnIndexes == null)
        {
            return source;
        }

        if (rowIndexes != null)
        {
            newRows = rowIndexes.Length;
            for (int i = 0; i < rowIndexes.Length; i++)
                if ((rowIndexes[i] < 0) || (rowIndexes[i] >= rows))
                    throw new ArgumentException("Argument out of range.");
        }

        if (columnIndexes != null)
        {
            newCols = columnIndexes.Length;
            for (int i = 0; i < columnIndexes.Length; i++)
                if ((columnIndexes[i] < 0) || (columnIndexes[i] >= cols))
                    throw new ArgumentException("Argument out of range.");
        }


        if (destination != null)
        {
            if (destination.Length < newRows)
                throw new DimensionMismatchException("destination",
                "The destination matrix must be big enough to accommodate the results.");
        }
        else
        {
            destination = new T[newRows][];
            if (columnIndexes != null && !reuseMemory)
            {
                for (int i = 0; i < destination.Length; i++)
                    destination[i] = new T[newCols];
            }
        }


        if (columnIndexes == null)
        {
            if (reuseMemory)
            {
                for (int i = 0; i < rowIndexes.Length; i++)
                    destination[i] = source[rowIndexes[i]];
            }
            else
            {
                for (int i = 0; i < rowIndexes.Length; i++)
                    destination[i] = (T[])source[rowIndexes[i]].Clone();
            }
        }
        else if (rowIndexes == null)
        {
            for (int i = 0; i < source.Length; i++)
                for (int j = 0; j < columnIndexes.Length; j++)
                    destination[i][j] = source[i][columnIndexes[j]];
        }
        else
        {
            for (int i = 0; i < rowIndexes.Length; i++)
                for (int j = 0; j < columnIndexes.Length; j++)
                    destination[i][j] = source[rowIndexes[i]][columnIndexes[j]];
        }

        return destination;
    }

    /// <summary>
    ///   Extracts a selected area from a matrix.
    /// </summary>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    private static T[][] get<T>(this T[][] source, T[][] destination,
        int startRow, int endRow, int startColumn, int endColumn, bool reuseMemory)
    {
        int rows = source.Length;
        int cols = source[0].Length;

        startRow = index(startRow, rows);
        startColumn = index(startColumn, cols);

        endRow = end(endRow, rows);
        endColumn = end(endColumn, cols);

        if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) ||
            (startRow > rows) || (endRow < 0) || (endRow > rows) ||
            (startColumn < 0) || (startColumn > cols) || (endColumn < 0) ||
            (endColumn > cols))
        {
            throw new ArgumentException("Argument out of range.");
        }

        bool canAvoidAllocation = startColumn == 0 && endColumn == rows;

        int newCols = endColumn - startColumn;

        if (destination == null)
        {
            destination = new T[endRow - startRow][];

            if (!canAvoidAllocation || !reuseMemory)
            {
                for (int i = 0; i < destination.Length; i++)
                    destination[i] = new T[newCols];
            }
        }

        if (reuseMemory && canAvoidAllocation)
        {
            for (int i = startRow; i < endRow; i++)
                destination[i - startRow] = source[i];
        }
        else
        {
            for (int i = startRow; i < endRow; i++)
                for (int j = startColumn; j < endColumn; j++)
                    destination[i - startRow][j - startColumn] = source[i][j];
        }

        return destination;
    }

    /// <summary>
    ///   Extracts a selected area from a matrix.
    /// </summary>
    /// 
    /// <remarks>
    ///   Routine adapted from Lutz Roeder's Mapack for .NET, September 2000.
    /// </remarks>
    /// 
    private static T[][] set<T>(this T[][] destination, T[][] source,
        int startRow, int endRow, int startColumn, int endColumn)
    {
        int rows = destination.Length;
        int cols = destination[0].Length;

        startRow = index(startRow, rows);
        startColumn = index(startColumn, cols);

        endRow = end(endRow, rows);
        endColumn = end(endColumn, cols);

        if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) ||
            (startRow > rows) || (endRow < 0) || (endRow > rows) ||
            (startColumn < 0) || (startColumn > cols) || (endColumn < 0) ||
            (endColumn > cols))
        {
            throw new ArgumentException("Argument out of range.");
        }

        for (int i = startRow; i < endRow; i++)
            for (int j = startColumn; j < endColumn; j++)
                destination[i - startRow][j - startColumn] = source[i][j];

        return destination;
    }



    #endregion


    /// <summary>
    ///   Gets the number of rows in a vector.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the elements in the column vector.</typeparam>
    /// <param name="vector">The vector whose number of rows must be computed.</param>
    /// 
    /// <returns>The number of rows in the column vector.</returns>
    /// 
    public static int Rows<T>(this T[] vector)
    {
        return vector.Length;
    }

    /// <summary>
    ///   Gets the number of rows in a multidimensional matrix.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The matrix whose number of rows must be computed.</param>
    /// 
    /// <returns>The number of rows in the matrix.</returns>
    /// 
    public static int Rows<T>(this T[,] matrix)
    {
        return matrix.GetLength(0);
    }

    /// <summary>
    ///   Gets the number of columns in a multidimensional matrix.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The matrix whose number of columns must be computed.</param>
    /// 
    /// <returns>The number of columns in the matrix.</returns>
    /// 
    public static int Columns<T>(this T[,] matrix)
    {
        return matrix.GetLength(1);
    }


    #region Element search

    /// <summary>
    ///   Gets the number of elements matching a certain criteria.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="data">The array to search inside.</param>
    /// <param name="func">The search criteria.</param>
    /// 
    public static int Count<T>(this T[] data, Func<T, bool> func)
    {
        int count = 0;
        for (int i = 0; i < data.Length; i++)
            if (func(data[i])) count++;
        return count;
    }

    /// <summary>
    ///   Gets the indices of the first element matching a certain criteria.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the array.</typeparam>
    /// 
    /// <param name="data">The array to search inside.</param>
    /// <param name="func">The search criteria.</param>
    /// 
    public static int First<T>(this T[] data, Func<T, bool> func)
    {
        return Find(data, func, firstOnly: true)[0];
    }

    /// <summary>
    ///   Gets the indices of the first element matching a certain criteria, or null if the element could not be found.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the array.</typeparam>
    /// 
    /// <param name="data">The array to search inside.</param>
    /// <param name="func">The search criteria.</param>
    /// 
    public static int? FirstOrNull<T>(this T[] data, Func<T, bool> func)
    {
        int[] r = Find(data, func, firstOnly: true);
        if (r.Length == 0)
            return null;
        return r[0];
    }

    /// <summary>
    ///   Searches for the specified value and returns the index of the first occurrence within the array.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the array.</typeparam>
    /// 
    /// <param name="data">The array to search.</param>
    /// <param name="value">The value to be searched.</param>
    /// 
    /// <returns>The index of the searched value within the array, or -1 if not found.</returns>
    /// 
    public static int IndexOf<T>(this T[] data, T value)
    {
        return Array.IndexOf(data, value);
    }

    /// <summary>
    ///   Gets the indices of all elements matching a certain criteria.
    /// </summary>
    /// 
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="data">The array to search inside.</param>
    /// <param name="func">The search criteria.</param>
    ///
#if NET45 || NET46 || NET462 || ns_2_1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static int[] Find<T>(this T[] data, Func<T, bool> func)
    {
        List<int> idx = new List<int>();

        for (int i = 0; i < data.Length; i++)
            if (func(data[i]))
                idx.Add(i);

        return idx.ToArray();
    }

    /// <summary>
    ///   Gets the indices of all elements matching a certain criteria.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="data">The array to search inside.</param>
    /// <param name="func">The search criteria.</param>
    /// <param name="firstOnly">
    ///    Set to true to stop when the first element is
    ///    found, set to false to get all elements.
    /// </param>
    public static int[] Find<T>(this T[] data, Func<T, bool> func, bool firstOnly)
    {
        List<int> idx = new List<int>();
        for (int i = 0; i < data.Length; i++)
        {
            if (func(data[i]))
            {
                if (firstOnly)
                    return new int[] { i };
                else idx.Add(i);
            }
        }
        return idx.ToArray();
    }

    /// <summary>
    ///   Gets the indices of all elements matching a certain criteria.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="data">The array to search inside.</param>
    /// <param name="func">The search criteria.</param>
    public static int[][] Find<T>(this T[,] data, Func<T, bool> func)
    {
        return Find(data, func, false);
    }

    /// <summary>
    ///   Gets the indices of all elements matching a certain criteria.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="data">The array to search inside.</param>
    /// <param name="func">The search criteria.</param>
    /// <param name="firstOnly">
    ///    Set to true to stop when the first element is
    ///    found, set to false to get all elements.
    /// </param>
    public static int[][] Find<T>(this T[,] data, Func<T, bool> func, bool firstOnly)
    {
        List<int[]> idx = new List<int[]>();
        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                if (func(data[i, j]))
                {
                    if (firstOnly)
                        return new int[][] { new int[] { i, j } };
                    else idx.Add(new int[] { i, j });
                }
            }
        }
        return idx.ToArray();
    }
    #endregion
}