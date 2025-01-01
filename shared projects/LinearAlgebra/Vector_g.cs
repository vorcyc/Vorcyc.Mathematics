using System.Numerics;

namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Represents a mathematical vector with elements of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the vector, which must implement <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
public class Vector<T>
    where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>
    /// Gets the elements of the vector.
    /// </summary>
    public T[] Elements { get; }

    /// <summary>
    /// Gets the dimension of the vector.
    /// </summary>
    public int Dimension => Elements.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector{T}"/> class with the specified elements.
    /// </summary>
    /// <param name="elements">The elements of the vector.</param>
    public Vector(params T[] elements)
    {
        Elements = elements;
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    /// <exception cref="ArgumentException">Thrown when the vectors are not of the same dimension.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> Add(Vector<T> v1, Vector<T> v2)
    {
        if (v1.Dimension != v2.Dimension)
            throw new ArgumentException("Vectors must be of the same dimension");

        T[] result = new T[v1.Dimension];
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        for (; i <= v1.Dimension - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<T>(v1.Elements, i);
            var vec2 = new System.Numerics.Vector<T>(v2.Elements, i);
            (vec1 + vec2).CopyTo(result, i);
        }

        // 处理剩余部分
        for (; i < v1.Dimension; i++)
        {
            result[i] = v1.Elements[i] + v2.Elements[i];
        }

        return new Vector<T>(result);
    }

    /// <summary>
    /// Subtracts the second vector from the first vector.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The difference of the two vectors.</returns>
    /// <exception cref="ArgumentException">Thrown when the vectors are not of the same dimension.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> Subtract(Vector<T> v1, Vector<T> v2)
    {
        if (v1.Dimension != v2.Dimension)
            throw new ArgumentException("Vectors must be of the same dimension");

        T[] result = new T[v1.Dimension];
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        for (; i <= v1.Dimension - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<T>(v1.Elements, i);
            var vec2 = new System.Numerics.Vector<T>(v2.Elements, i);
            (vec1 - vec2).CopyTo(result, i);
        }

        // 处理剩余部分
        for (; i < v1.Dimension; i++)
        {
            result[i] = v1.Elements[i] - v2.Elements[i];
        }

        return new Vector<T>(result);
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The dot product of the two vectors.</returns>
    /// <exception cref="ArgumentException">Thrown when the vectors are not of the same dimension.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DotProduct(Vector<T> v1, Vector<T> v2)
    {
        if (v1.Dimension != v2.Dimension)
            throw new ArgumentException("Vectors must be of the same dimension");

        T result = T.Zero;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdResult = new System.Numerics.Vector<T>(T.Zero);
        for (; i <= v1.Dimension - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<T>(v1.Elements, i);
            var vec2 = new System.Numerics.Vector<T>(v2.Elements, i);
            simdResult += vec1 * vec2;
        }

        // 处理剩余部分
        for (; i < v1.Dimension; i++)
        {
            result += v1.Elements[i] * v2.Elements[i];
        }

        // 汇总SIMD结果
        for (int j = 0; j < simdLength; j++)
        {
            result += simdResult[j];
        }

        return result;
    }

    /// <summary>
    /// Calculates the magnitude (length) of the vector.
    /// </summary>
    /// <returns>The magnitude of the vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Magnitude()
    {
        T sumOfSquares = T.Zero;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdResult = new System.Numerics.Vector<T>(T.Zero);
        for (; i <= Dimension - simdLength; i += simdLength)
        {
            var vec = new System.Numerics.Vector<T>(Elements, i);
            simdResult += vec * vec;
        }

        // 处理剩余部分
        for (; i < Dimension; i++)
        {
            sumOfSquares += Elements[i] * Elements[i];
        }

        // 汇总SIMD结果
        for (int j = 0; j < simdLength; j++)
        {
            sumOfSquares += simdResult[j];
        }

        return T.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// Normalizes the vector.
    /// </summary>
    /// <returns>The normalized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector<T> Normalize()
    {
        T magnitude = Magnitude();
        T[] result = new T[Dimension];
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdMagnitude = new System.Numerics.Vector<T>(magnitude);
        for (; i <= Dimension - simdLength; i += simdLength)
        {
            var vec = new System.Numerics.Vector<T>(Elements, i);
            (vec / simdMagnitude).CopyTo(result, i);
        }

        // 处理剩余部分
        for (; i < Dimension; i++)
        {
            result[i] = Elements[i] / magnitude;
        }

        return new Vector<T>(result);
    }

    /// <summary>
    /// Adds two vectors using the + operator.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> operator +(Vector<T> v1, Vector<T> v2) => Add(v1, v2);

    /// <summary>
    /// Subtracts the second vector from the first vector using the - operator.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The difference of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> operator -(Vector<T> v1, Vector<T> v2) => Subtract(v1, v2);

    /// <summary>
    /// Calculates the dot product of two vectors using the * operator.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The dot product of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T operator *(Vector<T> v1, Vector<T> v2) => DotProduct(v1, v2);


    /// <summary>
    /// 矩阵与向量乘法运算符。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="vector">向量。</param>
    /// <returns>矩阵与向量的乘积。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> operator *(Matrix<T> matrix, Vector<T> vector)
    {
        if (matrix.Columns != vector.Dimension)
            throw new ArgumentException("Matrix columns must match vector dimension for multiplication.");

        T[] result = new T[matrix.Rows];
        for (int i = 0; i < matrix.Rows; i++)
        {
            result[i] = T.Zero;
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i] += matrix[i, j] * vector.Elements[j];
            }
        }
        return new Vector<T>(result);
    }
}
