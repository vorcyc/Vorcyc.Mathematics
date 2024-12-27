namespace Vorcyc.Mathematics.LinearAlgebra;


using System.Numerics;
using System.Runtime.CompilerServices;


///<summary>3-dimensional tensor of <see cref="IBinaryFloatingPointIeee754{TSelf}"/> data type.</summary>
public class Tensor<T> :ICloneable<Tensor<T>>
    where T : IBinaryFloatingPointIeee754<T>
{
    private readonly T[] _values;

    ///<summary>Initializes the Tensor with specified size.</summary>
    ///<param name="w">Width.</param>
    ///<param name="h">Height.</param>
    ///<param name="d">Depth.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor(int w, int h, int d)
    {
        if (w <= 0 || h <= 0 || d <= 0)
        {
            throw new ArgumentException("Dimensions must be positive non-zero values.");
        }

        this._values = new T[w * h * d];
        this.Width = w;
        this.Height = h;
        this.Depth = d;
    }

    ///<summary>Initializes the Tensor with specified size and initial value.</summary>
    ///<param name="w">Width.</param>
    ///<param name="h">Height.</param>
    ///<param name="d">Depth.</param>
    ///<param name="initialValue">Initial value for all elements.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor(int w, int h, int d, T initialValue) : this(w, h, d)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = initialValue;
        }
    }

    ///<summary>Initializes the Tensor from a 3D array.</summary>
    ///<param name="array">3D array to initialize the tensor.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor(T[,,] array)
    {
        this.Width = array.GetLength(0);
        this.Height = array.GetLength(1);
        this.Depth = array.GetLength(2);
        this._values = new T[Width * Height * Depth];

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

    ///<summary>Values.</summary>
    public T[] Values => _values;

    ///<summary>Width.</summary>
    public int Width { get; }

    ///<summary>Height.</summary>
    public int Height { get; }

    ///<summary>Depth.</summary>
    public int Depth { get; }

    /// <summary>
    /// Gets or sets the value at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate (Width).</param>
    /// <param name="y">Y coordinate (Height).</param>
    /// <param name="z">Z coordinate (Depth).</param>
    /// <returns>The value at the specified coordinates.</returns>
    public T this[int x, int y, int z]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ValidateIndices(x, y, z);
            return this._values[((this.Width * y) + x) * this.Depth + z];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ValidateIndices(x, y, z);
            this._values[((this.Width * y) + x) * this.Depth + z] = value;
        }
    }

    /// <summary>
    /// Validates the provided indices.
    /// </summary>
    /// <param name="x">X coordinate (Width).</param>
    /// <param name="y">Y coordinate (Height).</param>
    /// <param name="z">Z coordinate (Depth).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateIndices(int x, int y, int z)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
        {
            throw new ArgumentOutOfRangeException("Indices are out of range.");
        }
    }

    /// <summary>
    /// Fills the tensor with the specified value.
    /// </summary>
    /// <param name="value">The value to fill the tensor with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(T value)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = value;
        }
    }

    #region operators inline

    /// <summary>
    /// Adds another tensor to this tensor.
    /// </summary>
    /// <param name="other">The tensor to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Tensor<T> other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] += other._values[i];
        }
    }

    /// <summary>
    /// Subtracts another tensor from this tensor.
    /// </summary>
    /// <param name="other">The tensor to subtract.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Subtract(Tensor<T> other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] -= other._values[i];
        }
    }

    /// <summary>
    /// Multiplies this tensor by a scalar value.
    /// </summary>
    /// <param name="scalar">The scalar value to multiply by.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Multiply(T scalar)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] *= scalar;
        }
    }

    #endregion

    /// <summary>
    /// Clones the tensor.
    /// </summary>
    /// <returns>A new tensor that is a copy of this tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor<T> Clone()
    {
        Tensor<T> clone = new Tensor<T>(Width, Height, Depth);
        Array.Copy(_values, clone._values, _values.Length);
        return clone;
    }

    /// <summary>
    /// Returns a string representation of the tensor.
    /// </summary>
    /// <returns>A string representation of the tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"Tensor<{typeof(T).Name}> [Width={Width}, Height={Height}, Depth={Depth}]";
    }

    #region operators

    /// <summary>
    /// Adds two tensors.
    /// </summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>The result of adding the two tensors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Tensor<T> operator +(Tensor<T> a, Tensor<T> b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        Tensor<T> result = new Tensor<T>(a.Width, a.Height, a.Depth);
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
    /// <returns>The result of subtracting the second tensor from the first tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Tensor<T> operator -(Tensor<T> a, Tensor<T> b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        Tensor<T> result = new Tensor<T>(a.Width, a.Height, a.Depth);
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
    public static Tensor<T> operator *(Tensor<T> tensor, T scalar)
    {
        Tensor<T> result = new Tensor<T>(tensor.Width, tensor.Height, tensor.Depth);
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
    public static Tensor<T> operator *(T scalar, Tensor<T> tensor)
    {
        return tensor * scalar;
    }

    #endregion

    /// <summary>
    /// Transposes the tensor along the specified axes.
    /// </summary>
    /// <param name="axis1">The first axis to transpose.</param>
    /// <param name="axis2">The second axis to transpose.</param>
    /// <returns>A new tensor that is the transpose of this tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor<T> Transpose(int axis1, int axis2)
    {
        if (axis1 < 0 || axis1 > 2 || axis2 < 0 || axis2 > 2 || axis1 == axis2)
        {
            throw new ArgumentException("Invalid axes for transposition.");
        }

        int[] dims = { Width, Height, Depth };
        int newWidth = dims[axis1];
        int newHeight = dims[axis2];
        int newDepth = dims[3 - axis1 - axis2];

        Tensor<T> result = new Tensor<T>(newWidth, newHeight, newDepth);

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
    public T Sum()
    {
        return Statistics.Sum(_values.AsSpan());
    }

    /// <summary>
    /// Computes the dot product of two tensors.
    /// </summary>
    /// <param name="other">The other tensor.</param>
    /// <returns>The dot product of the two tensors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Tensor<T> other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        T dotProduct = T.Zero;
        for (int i = 0; i < _values.Length; i++)
        {
            dotProduct += _values[i] * other._values[i];
        }
        return dotProduct;
    }

    /// <summary>
    /// Normalizes the tensor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Normalize()
    {
        T norm = Norm();
        if (norm == T.Zero)
        {
            throw new InvalidOperationException("Cannot normalize a tensor with zero norm.");
        }

        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] /= norm;
        }
    }

    /// <summary>
    /// Computes the norm of the tensor.
    /// </summary>
    /// <returns>The norm of the tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Norm()
    {
        T sumOfSquares = T.Zero;
        for (int i = 0; i < _values.Length; i++)
        {
            sumOfSquares += _values[i] * _values[i];
        }
        return T.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// Slices the tensor along the specified axis.
    /// </summary>
    /// <param name="axis">The axis to slice along.</param>
    /// <param name="index">The index at which to slice.</param>
    /// <returns>A new tensor that is a slice of this tensor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor<T> Slice(int axis, int index)
    {
        if (axis < 0 || axis > 2)
        {
            throw new ArgumentException("Invalid axis for slicing.");
        }

        int[] dims = { Width, Height, Depth };
        if (index < 0 || index >= dims[axis])
        {
            throw new ArgumentOutOfRangeException("Index is out of range.");
        }

        int newWidth = axis == 0 ? 1 : Width;
        int newHeight = axis == 1 ? 1 : Height;
        int newDepth = axis == 2 ? 1 : Depth;

        Tensor<T> result = new Tensor<T>(newWidth, newHeight, newDepth);

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
    /// Checks if two tensors are equal.
    /// </summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>True if the tensors are equal, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Tensor<T> a, Tensor<T> b)
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
    /// Checks if two tensors are not equal.
    /// </summary>
    /// <param name="a">The first tensor.</param>
    /// <param name="b">The second tensor.</param>
    /// <returns>True if the tensors are not equal, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Tensor<T> a, Tensor<T> b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        if (obj is Tensor<T> other)
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
