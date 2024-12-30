namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Represents a mathematical vector with elements of type <see cref="float"/>.
/// </summary>
public class Vector
{
    /// <summary>
    /// Gets the elements of the vector.
    /// </summary>
    public float[] Elements { get; }

    /// <summary>
    /// Gets the dimension of the vector.
    /// </summary>
    public int Dimension => Elements.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector"/> class with the specified elements.
    /// </summary>
    /// <param name="elements">The elements of the vector.</param>
    public Vector(params float[] elements)
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
    public static Vector Add(Vector v1, Vector v2)
    {
        if (v1.Dimension != v2.Dimension)
            throw new ArgumentException("Vectors must be of the same dimension");

        float[] result = new float[v1.Dimension];
        int simdLength = System.Numerics.Vector<float>.Count;
        int i = 0;

        // SIMD部分
        for (; i <= v1.Dimension - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<float>(v1.Elements, i);
            var vec2 = new System.Numerics.Vector<float>(v2.Elements, i);
            (vec1 + vec2).CopyTo(result, i);
        }

        // 处理剩余部分
        for (; i < v1.Dimension; i++)
        {
            result[i] = v1.Elements[i] + v2.Elements[i];
        }

        return new Vector(result);
    }

    /// <summary>
    /// Subtracts the second vector from the first vector.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The difference of the two vectors.</returns>
    /// <exception cref="ArgumentException">Thrown when the vectors are not of the same dimension.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector Subtract(Vector v1, Vector v2)
    {
        if (v1.Dimension != v2.Dimension)
            throw new ArgumentException("Vectors must be of the same dimension");

        float[] result = new float[v1.Dimension];
        int simdLength = System.Numerics.Vector<float>.Count;
        int i = 0;

        // SIMD部分
        for (; i <= v1.Dimension - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<float>(v1.Elements, i);
            var vec2 = new System.Numerics.Vector<float>(v2.Elements, i);
            (vec1 - vec2).CopyTo(result, i);
        }

        // 处理剩余部分
        for (; i < v1.Dimension; i++)
        {
            result[i] = v1.Elements[i] - v2.Elements[i];
        }

        return new Vector(result);
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The dot product of the two vectors.</returns>
    /// <exception cref="ArgumentException">Thrown when the vectors are not of the same dimension.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DotProduct(Vector v1, Vector v2)
    {
        if (v1.Dimension != v2.Dimension)
            throw new ArgumentException("Vectors must be of the same dimension");

        float result = 0;
        int simdLength = System.Numerics.Vector<float>.Count;
        int i = 0;

        // SIMD部分
        var simdResult = new System.Numerics.Vector<float>(0);
        for (; i <= v1.Dimension - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<float>(v1.Elements, i);
            var vec2 = new System.Numerics.Vector<float>(v2.Elements, i);
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
    public float Magnitude()
    {
        float sumOfSquares = 0;
        int simdLength = System.Numerics.Vector<float>.Count;
        int i = 0;

        // SIMD部分
        var simdResult = new System.Numerics.Vector<float>(0);
        for (; i <= Dimension - simdLength; i += simdLength)
        {
            var vec = new System.Numerics.Vector<float>(Elements, i);
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

        return MathF.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// Normalizes the vector.
    /// </summary>
    /// <returns>The normalized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector Normalize()
    {
        float magnitude = Magnitude();
        float[] result = new float[Dimension];
        int simdLength = System.Numerics.Vector<float>.Count;
        int i = 0;

        // SIMD部分
        var simdMagnitude = new System.Numerics.Vector<float>(magnitude);
        for (; i <= Dimension - simdLength; i += simdLength)
        {
            var vec = new System.Numerics.Vector<float>(Elements, i);
            (vec / simdMagnitude).CopyTo(result, i);
        }

        // 处理剩余部分
        for (; i < Dimension; i++)
        {
            result[i] = Elements[i] / magnitude;
        }

        return new Vector(result);
    }

    /// <summary>
    /// Adds two vectors using the + operator.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator +(Vector v1, Vector v2) => Add(v1, v2);

    /// <summary>
    /// Subtracts the second vector from the first vector using the - operator.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The difference of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator -(Vector v1, Vector v2) => Subtract(v1, v2);

    /// <summary>
    /// Calculates the dot product of two vectors using the * operator.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The dot product of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float operator *(Vector v1, Vector v2) => DotProduct(v1, v2);


    /// <summary>
    /// 矩阵与向量乘法运算符。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="vector">向量。</param>
    /// <returns>矩阵与向量的乘积。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector operator *(Matrix matrix, Vector vector)
    {
        if (matrix.Columns != vector.Dimension)
            throw new ArgumentException("Matrix columns must match vector dimension for multiplication.");

        float[] result = new float[matrix.Rows];
        for (int i = 0; i < matrix.Rows; i++)
        {
            result[i] = 0f;
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i] += matrix[i, j] * vector.Elements[j];
            }
        }
        return new Vector(result);
    }
}
