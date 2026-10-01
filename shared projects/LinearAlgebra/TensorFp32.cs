namespace Vorcyc.Mathematics.LinearAlgebra;

using Vorcyc.Mathematics.Framework;

///<summary>A 3D tensor with <see cref="float"/> element type.</summary>
public class TensorFp32 : ICloneable<TensorFp32>
{
    private readonly float[] _values;

    ///<summary>Initializes a tensor with the specified dimensions.</summary>
    ///<param name="w">Width.</param>
    ///<param name="h">Height.</param>
    ///<param name="d">Depth.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TensorFp32(int w, int h, int d)
    {
        if (w <= 0 || h <= 0 || d <= 0)
        {
            throw new ArgumentException("Dimensions must be positive and non-zero.");
        }

        this._values = new float[w * h * d];
        this.Width = w;
        this.Height = h;
        this.Depth = d;
    }

    ///<summary>Initializes a tensor with the specified dimensions and initial value.</summary>
    ///<param name="w">Width.</param>
    ///<param name="h">Height.</param>
    ///<param name="d">Depth.</param>
    ///<param name="initialValue">The initial value for all elements.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TensorFp32(int w, int h, int d, float initialValue) : this(w, h, d)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = initialValue;
        }
    }

    ///<summary>Initializes a tensor from a 3D array.</summary>
    ///<param name="array">The 3D array used to initialize the tensor.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TensorFp32(float[,,] array)
    {
        this.Width = array.GetLength(0);
        this.Height = array.GetLength(1);
        this.Depth = array.GetLength(2);
        this._values = new float[Width * Height * Depth];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    this[x, y, z] = array[x, y, z];
                }
            }
        }
    }

    ///<summary>The tensor values.</summary>
    public float[] Values => _values;

    ///<summary>Width.</summary>
    public int Width { get; }

    ///<summary>Height.</summary>
    public int Height { get; }

    ///<summary>Depth.</summary>
    public int Depth { get; }

    /// <summary>
    /// Gets or sets the value at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate (width).</param>
    /// <param name="y">Y coordinate (height).</param>
    /// <param name="z">Z coordinate (depth).</param>
    /// <returns>The value at the specified coordinates.</returns>
    public ref float this[int x, int y, int z]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ValidateIndices(x, y, z);
            return ref this._values[((this.Width * y) + x) * this.Depth + z];
        }
    }

    /// <summary>
    /// Validates the provided indices.
    /// </summary>
    /// <param name="x">X coordinate (width).</param>
    /// <param name="y">Y coordinate (height).</param>
    /// <param name="z">Z coordinate (depth).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateIndices(int x, int y, int z)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
        {
            throw new ArgumentOutOfRangeException("Index out of range.");
        }
    }

    /// <summary>
    /// Fills the tensor with the specified value.
    /// </summary>
    /// <param name="value">The value used to fill the tensor.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(float value)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = value;
        }
    }

    #region operators inline

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TensorFp32 other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("TensorFp32 dimensions must match.");
        }

        int vectorSize = System.Numerics.Vector<float>.Count;
        int i = 0;

        for (; i <= _values.Length - vectorSize; i += vectorSize)
        {
            var v1 = new System.Numerics.Vector<float>(_values, i);
            var v2 = new System.Numerics.Vector<float>(other._values, i);
            (v1 + v2).CopyTo(_values, i);
        }

        for (; i < _values.Length; i++)
        {
            _values[i] += other._values[i];
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Subtract(TensorFp32 other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("TensorFp32 dimensions must match.");
        }

        int vectorSize = System.Numerics.Vector<float>.Count;
        int i = 0;

        for (; i <= _values.Length - vectorSize; i += vectorSize)
        {
            var v1 = new System.Numerics.Vector<float>(_values, i);
            var v2 = new System.Numerics.Vector<float>(other._values, i);
            (v1 - v2).CopyTo(_values, i);
        }

        for (; i < _values.Length; i++)
        {
            _values[i] -= other._values[i];
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Multiply(float scalar)
    {
        int vectorSize = System.Numerics.Vector<float>.Count;
        int i = 0;
        var vScalar = new System.Numerics.Vector<float>(scalar);

        for (; i <= _values.Length - vectorSize; i += vectorSize)
        {
            var v = new System.Numerics.Vector<float>(_values, i);
            (v * vScalar).CopyTo(_values, i);
        }

        for (; i < _values.Length; i++)
        {
            _values[i] *= scalar;
        }
    }

    #endregion

    /// <summary>
    /// Clones the tensor.
    /// </summary>
    /// <returns>A new tensor that is a copy of the current tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TensorFp32 Clone()
    {
        TensorFp32 clone = new(Width, Height, Depth);
        Array.Copy(_values, clone._values, _values.Length);
        return clone;
    }

    /// <summary>
    /// Returns the string representation of the tensor.
    /// </summary>
    /// <returns>The string representation of the tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"Tensor<float> [Width={Width}, Height={Height}, Depth={Depth}]";
    }

    #region operators

    /// <summary>
    /// Adds two tensors.
    /// </summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>The result of adding the two tensors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TensorFp32 operator +(TensorFp32 a, TensorFp32 b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("TensorFp32 dimensions must match.");
        }

        TensorFp32 result = new TensorFp32(a.Width, a.Height, a.Depth);
        for (int i = 0; i < a._values.Length; i++)
        {
            result._values[i] = a._values[i] + b._values[i];
        }
        return result;
    }

    /// <summary>
    /// Subtracts one tensor from another.
    /// </summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>The result of subtracting the second tensor from the first.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TensorFp32 operator -(TensorFp32 a, TensorFp32 b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("TensorFp32 dimensions must match.");
        }

        TensorFp32 result = new TensorFp32(a.Width, a.Height, a.Depth);
        for (int i = 0; i < a._values.Length; i++)
        {
            result._values[i] = a._values[i] - b._values[i];
        }
        return result;
    }

    /// <summary>
    /// Multiplies a tensor by a scalar value.
    /// </summary>
    /// <param name="tensor">The tensor.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The result of multiplying the tensor by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TensorFp32 operator *(TensorFp32 tensor, float scalar)
    {
        TensorFp32 result = new TensorFp32(tensor.Width, tensor.Height, tensor.Depth);
        for (int i = 0; i < tensor._values.Length; i++)
        {
            result._values[i] = tensor._values[i] * scalar;
        }
        return result;
    }

    /// <summary>
    /// Multiplies a tensor by a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="tensor">The tensor.</param>
    /// <returns>The result of multiplying the tensor by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TensorFp32 operator *(float scalar, TensorFp32 tensor)
    {
        return tensor * scalar;
    }

    #endregion

    /// <summary>
    /// Transposes the tensor along the specified axes.
    /// </summary>
    /// <param name="axis1">The first axis.</param>
    /// <param name="axis2">The second axis.</param>
    /// <returns>The transposed tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TensorFp32 Transpose(int axis1, int axis2)
    {
        if (axis1 < 0 || axis1 > 2 || axis2 < 0 || axis2 > 2 || axis1 == axis2)
        {
            throw new ArgumentException("Invalid transpose axis.");
        }

        int[] dims = { Width, Height, Depth };
        int newWidth = dims[axis1];
        int newHeight = dims[axis2];
        int newDepth = dims[3 - axis1 - axis2];

        TensorFp32 result = new TensorFp32(newWidth, newHeight, newDepth);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    int[] indices = { x, y, z };
                    result[indices[axis1], indices[axis2], indices[3 - axis1 - axis2]] = this[x, y, z];
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Computes the sum of all elements in the tensor.
    /// </summary>
    /// <returns>The sum of all elements in the tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Sum()
    {
        float sum = 0;
        for (int i = 0; i < _values.Length; i++)
        {
            sum += _values[i];
        }
        return sum;
    }

    /// <summary>
    /// Computes the dot product of two tensors.
    /// </summary>
    /// <param name="other">The other tensor.</param>
    /// <returns>The dot product of the two tensors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Dot(TensorFp32 other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("TensorFp32 dimensions must match.");
        }

        int vectorSize = System.Numerics.Vector<float>.Count;
        int i = 0;
        var dotProduct = System.Numerics.Vector<float>.Zero;

        for (; i <= _values.Length - vectorSize; i += vectorSize)
        {
            var v1 = new System.Numerics.Vector<float>(_values, i);
            var v2 = new System.Numerics.Vector<float>(other._values, i);
            dotProduct += v1 * v2;
        }

        float result = 0;
        for (int j = 0; j < vectorSize; j++)
        {
            result += dotProduct[j];
        }

        for (; i < _values.Length; i++)
        {
            result += _values[i] * other._values[i];
        }

        return result;
    }

    /// <summary>
    /// Normalizes the tensor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Normalize()
    {
        float norm = Norm();
        if (norm == 0)
        {
            throw new InvalidOperationException("Cannot normalize a tensor with zero norm.");
        }

        int vectorSize = System.Numerics.Vector<float>.Count;
        int i = 0;
        var vNorm = new System.Numerics.Vector<float>(norm);

        for (; i <= _values.Length - vectorSize; i += vectorSize)
        {
            var v = new System.Numerics.Vector<float>(_values, i);
            (v / vNorm).CopyTo(_values, i);
        }

        for (; i < _values.Length; i++)
        {
            _values[i] /= norm;
        }
    }


    /// <summary>
    /// Computes the norm of the tensor.
    /// </summary>
    /// <returns>The norm of the tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Norm()
    {
        int vectorSize = System.Numerics.Vector<float>.Count;
        int i = 0;
        var sumOfSquares = System.Numerics.Vector<float>.Zero;

        for (; i <= _values.Length - vectorSize; i += vectorSize)
        {
            var v = new System.Numerics.Vector<float>(_values, i);
            sumOfSquares += v * v;
        }

        float result = 0;
        for (int j = 0; j < vectorSize; j++)
        {
            result += sumOfSquares[j];
        }

        for (; i < _values.Length; i++)
        {
            result += _values[i] * _values[i];
        }

        return MathF.Sqrt(result);
    }

    /// <summary>
    /// Slices the tensor along the specified axis.
    /// </summary>
    /// <param name="axis">The axis to slice.</param>
    /// <param name="index">The slice index.</param>
    /// <returns>The sliced tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TensorFp32 Slice(int axis, int index)
    {
        if (axis < 0 || axis > 2)
        {
            throw new ArgumentException("Invalid slice axis.");
        }

        int[] dims = { Width, Height, Depth };
        if (index < 0 || index >= dims[axis])
        {
            throw new ArgumentOutOfRangeException("Index out of range.");
        }

        int newWidth = axis == 0 ? 1 : Width;
        int newHeight = axis == 1 ? 1 : Height;
        int newDepth = axis == 2 ? 1 : Depth;

        TensorFp32 result = new TensorFp32(newWidth, newHeight, newDepth);

        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                for (int z = 0; z < newDepth; z++)
                {
                    int[] indices = { x, y, z };
                    indices[axis] = index;
                    result[x, y, z] = this[indices[0], indices[1], indices[2]];
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Checks whether two tensors are equal.
    /// </summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns><see langword="true"/> if the tensors are equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(TensorFp32 a, TensorFp32 b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            return false;
        }

        for (int i = 0; i < a._values.Length; i++)
        {
            if (!a._values[i].Equals(b._values[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks whether two tensors are not equal.
    /// </summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns><see langword="true"/> if the tensors are not equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(TensorFp32 a, TensorFp32 b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        if (obj is TensorFp32 other)
        {
            return this == other;
        }
        return false;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + Width.GetHashCode();
        hash = hash * 31 + Height.GetHashCode();
        hash = hash * 31 + Depth.GetHashCode();
        foreach (var value in _values)
        {
            hash = hash * 31 + value.GetHashCode();
        }
        return hash;
    }
}
