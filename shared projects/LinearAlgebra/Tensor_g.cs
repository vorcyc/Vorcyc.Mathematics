namespace Vorcyc.Mathematics.LinearAlgebra;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// 3维张量，数据类型为 <see cref="IBinaryFloatingPointIeee754{TSelf}"/>。
/// </summary>
public class Tensor<T> : ICloneable<Tensor<T>>
    where T : IBinaryFloatingPointIeee754<T>
{
    private readonly Memory<T> _values;

    /// <summary>
    /// 使用指定的大小初始化张量。
    /// </summary>
    /// <param name="w">宽度。</param>
    /// <param name="h">高度。</param>
    /// <param name="d">深度。</param>
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

    /// <summary>
    /// 使用指定的大小和初始值初始化张量。
    /// </summary>
    /// <param name="w">宽度。</param>
    /// <param name="h">高度。</param>
    /// <param name="d">深度。</param>
    /// <param name="initialValue">所有元素的初始值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor(int w, int h, int d, T initialValue) : this(w, h, d)
    {
        var span = _values.Span;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = initialValue;
        }
    }

    /// <summary>
    /// 从三维数组初始化张量。
    /// </summary>
    /// <param name="array">用于初始化张量的三维数组。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor(T[,,] array)
    {
        this.Width = array.GetLength(0);
        this.Height = array.GetLength(1);
        this.Depth = array.GetLength(2);
        this._values = new T[Width * Height * Depth];

        var span = _values.Span;
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

    /// <summary>
    /// 获取张量的值。
    /// </summary>
    public Span<T> Values => _values.Span;

    /// <summary>
    /// 获取张量的宽度。
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 获取张量的高度。
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// 获取张量的深度。
    /// </summary>
    public int Depth { get; }

    ///// <summary>
    ///// 获取或设置指定坐标的值。
    ///// </summary>
    ///// <param name="x">X 坐标（宽度）。</param>
    ///// <param name="y">Y 坐标（高度）。</param>
    ///// <param name="z">Z 坐标（深度）。</param>
    ///// <returns>指定坐标的值。</returns>
    //public T this[int x, int y, int z]
    //{
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    get
    //    {
    //        ValidateIndices(x, y, z);
    //        return _values.Span[((this.Width * y) + x) * this.Depth + z];
    //    }
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    set
    //    {
    //        ValidateIndices(x, y, z);
    //        _values.Span[((this.Width * y) + x) * this.Depth + z] = value;
    //    }
    //}

    /// <summary>
    /// 获取或设置指定坐标的值。
    /// </summary>
    /// <param name="x">X 坐标（宽度）。</param>
    /// <param name="y">Y 坐标（高度）。</param>
    /// <param name="z">Z 坐标（深度）。</param>
    /// <returns>指定坐标的值。</returns>
    public ref T this[int x, int y, int z]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ValidateIndices(x, y, z);
            return ref _values.Span[((this.Width * y) + x) * this.Depth + z];
        }
    }


    /// <summary>
    /// 验证提供的索引。
    /// </summary>
    /// <param name="x">X 坐标（宽度）。</param>
    /// <param name="y">Y 坐标（高度）。</param>
    /// <param name="z">Z 坐标（深度）。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateIndices(int x, int y, int z)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
        {
            throw new ArgumentOutOfRangeException("Indices are out of range.");
        }
    }

    /// <summary>
    /// 用指定的值填充张量。
    /// </summary>
    /// <param name="value">用于填充张量的值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(T value)
    {
        var span = _values.Span;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdValue = new System.Numerics.Vector<T>(value);
        for (; i <= span.Length - simdLength; i += simdLength)
        {
            simdValue.CopyTo(span.Slice(i));
        }

        // 处理剩余部分
        for (; i < span.Length; i++)
        {
            span[i] = value;
        }
    }



    #region operators inline

    /// <summary>
    /// 将另一个张量加到此张量。
    /// </summary>
    /// <param name="other">要加的张量。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Tensor<T> other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        var span = _values.Span;
        var otherSpan = other._values.Span;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        for (; i <= span.Length - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<T>(span.Slice(i));
            var vec2 = new System.Numerics.Vector<T>(otherSpan.Slice(i));
            (vec1 + vec2).CopyTo(span.Slice(i));
        }

        // 处理剩余部分
        for (; i < span.Length; i++)
        {
            span[i] += otherSpan[i];
        }
    }

    /// <summary>
    /// 从此张量中减去另一个张量。
    /// </summary>
    /// <param name="other">要减去的张量。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Subtract(Tensor<T> other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        var span = _values.Span;
        var otherSpan = other._values.Span;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        for (; i <= span.Length - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<T>(span.Slice(i));
            var vec2 = new System.Numerics.Vector<T>(otherSpan.Slice(i));
            (vec1 - vec2).CopyTo(span.Slice(i));
        }

        // 处理剩余部分
        for (; i < span.Length; i++)
        {
            span[i] -= otherSpan[i];
        }
    }

    /// <summary>
    /// 将此张量乘以一个标量值。
    /// </summary>
    /// <param name="scalar">要乘以的标量值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Multiply(T scalar)
    {
        var span = _values.Span;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdScalar = new System.Numerics.Vector<T>(scalar);
        for (; i <= span.Length - simdLength; i += simdLength)
        {
            var vec = new System.Numerics.Vector<T>(span.Slice(i));
            (vec * simdScalar).CopyTo(span.Slice(i));
        }

        // 处理剩余部分
        for (; i < span.Length; i++)
        {
            span[i] *= scalar;
        }
    }

    #endregion

    #region operators

    /// <summary>
    /// 将两个张量相加。
    /// </summary>
    /// <param name="a">第一个张量。</param>
    /// <param name="b">第二个张量。</param>
    /// <returns>两个张量相加的结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Tensor<T> operator +(Tensor<T> a, Tensor<T> b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        Tensor<T> result = new Tensor<T>(a.Width, a.Height, a.Depth);
        var resultSpan = result._values.Span;
        var aSpan = a._values.Span;
        var bSpan = b._values.Span;
        for (int i = 0; i < aSpan.Length; i++)
        {
            resultSpan[i] = aSpan[i] + bSpan[i];
        }
        return result;
    }

    /// <summary>
    /// 将一个张量从另一个张量中减去。
    /// </summary>
    /// <param name="a">第一个张量。</param>
    /// <param name="b">第二个张量。</param>
    /// <returns>两个张量相减的结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Tensor<T> operator -(Tensor<T> a, Tensor<T> b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        Tensor<T> result = new Tensor<T>(a.Width, a.Height, a.Depth);
        var resultSpan = result._values.Span;
        var aSpan = a._values.Span;
        var bSpan = b._values.Span;
        for (int i = 0; i < aSpan.Length; i++)
        {
            resultSpan[i] = aSpan[i] - bSpan[i];
        }
        return result;
    }

    /// <summary>
    /// 将张量乘以一个标量值。
    /// </summary>
    /// <param name="tensor">张量。</param>
    /// <param name="scalar">标量值。</param>
    /// <returns>张量乘以标量值的结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Tensor<T> operator *(Tensor<T> tensor, T scalar)
    {
        Tensor<T> result = new Tensor<T>(tensor.Width, tensor.Height, tensor.Depth);
        var resultSpan = result._values.Span;
        var tensorSpan = tensor._values.Span;
        for (int i = 0; i < tensorSpan.Length; i++)
        {
            resultSpan[i] = tensorSpan[i] * scalar;
        }
        return result;
    }

    /// <summary>
    /// 将张量乘以一个标量值。
    /// </summary>
    /// <param name="scalar">标量值。</param>
    /// <param name="tensor">张量。</param>
    /// <returns>张量乘以标量值的结果。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Tensor<T> operator *(T scalar, Tensor<T> tensor)
    {
        return tensor * scalar;
    }

    #endregion


    public static implicit operator Span<T>(Tensor<T> tensor)
    {
        return tensor._values.Span;
    }


    /// <summary>
    /// 沿指定轴转置张量。
    /// </summary>
    /// <param name="axis1">第一个轴。</param>
    /// <param name="axis2">第二个轴。</param>
    /// <returns>转置后的新张量。</returns>
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
    /// 计算张量中所有元素的和。
    /// </summary>
    /// <returns>张量中所有元素的和。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Sum()
    {
        return Statistics.Sum(_values.Span);
    }

    /// <summary>
    /// 计算两个张量的点积。
    /// </summary>
    /// <param name="other">另一个张量。</param>
    /// <returns>两个张量的点积。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Tensor<T> other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("Tensor dimensions must match.");
        }

        T dotProduct = T.Zero;
        var span = _values.Span;
        var otherSpan = other._values.Span;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdDotProduct = new System.Numerics.Vector<T>(T.Zero);
        for (; i <= span.Length - simdLength; i += simdLength)
        {
            var vec1 = new System.Numerics.Vector<T>(span.Slice(i));
            var vec2 = new System.Numerics.Vector<T>(otherSpan.Slice(i));
            simdDotProduct += vec1 * vec2;
        }

        // 处理剩余部分
        for (; i < span.Length; i++)
        {
            dotProduct += span[i] * otherSpan[i];
        }

        // 汇总SIMD结果
        for (int j = 0; j < simdLength; j++)
        {
            dotProduct += simdDotProduct[j];
        }

        return dotProduct;
    }

    /// <summary>
    /// 归一化张量。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Normalize()
    {
        T norm = Norm();
        if (norm == T.Zero)
        {
            throw new InvalidOperationException("Cannot normalize a tensor with zero norm.");
        }

        var span = _values.Span;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdNorm = new System.Numerics.Vector<T>(norm);
        for (; i <= span.Length - simdLength; i += simdLength)
        {
            var vec = new System.Numerics.Vector<T>(span.Slice(i));
            (vec / simdNorm).CopyTo(span.Slice(i));
        }

        // 处理剩余部分
        for (; i < span.Length; i++)
        {
            span[i] /= norm;
        }
    }

    /// <summary>
    /// 计算张量的范数。
    /// </summary>
    /// <returns>张量的范数。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Norm()
    {
        T sumOfSquares = T.Zero;
        var span = _values.Span;
        int simdLength = System.Numerics.Vector<T>.Count;
        int i = 0;

        // SIMD部分
        var simdResult = new System.Numerics.Vector<T>(T.Zero);
        for (; i <= span.Length - simdLength; i += simdLength)
        {
            var vec = new System.Numerics.Vector<T>(span.Slice(i));
            simdResult += vec * vec;
        }

        // 处理剩余部分
        for (; i < span.Length; i++)
        {
            sumOfSquares += span[i] * span[i];
        }

        // 汇总SIMD结果
        for (int j = 0; j < simdLength; j++)
        {
            sumOfSquares += simdResult[j];
        }

        return T.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// 沿指定轴切片张量。
    /// </summary>
    /// <param name="axis">要切片的轴。</param>
    /// <param name="index">切片的索引。</param>
    /// <returns>切片后的新张量。</returns>
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
    /// 检查两个张量是否相等。
    /// </summary>
    /// <param name="a">第一个张量。</param>
    /// <param name="b">第二个张量。</param>
    /// <returns>如果两个张量相等，则为 true，否则为 false。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Tensor<T> a, Tensor<T> b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            return false;
        }

        var aSpan = a._values.Span;
        var bSpan = b._values.Span;
        for (int i = 0; i < aSpan.Length; i++)
        {
            if (!aSpan[i].Equals(bSpan[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 检查两个张量是否不相等。
    /// </summary>
    /// <param name="a">第一个张量。</param>
    /// <param name="b">第二个张量。</param>
    /// <returns>如果两个张量不相等，则为 true，否则为 false。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Tensor<T> a, Tensor<T> b)
    {
        return !(a == b);
    }

    /// <summary>
    /// 确定指定对象是否等于当前对象。
    /// </summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定对象等于当前对象，则为 true，否则为 false。</returns>
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
    /// 作为默认的哈希函数。
    /// </summary>
    /// <returns>当前对象的哈希代码。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + Width.GetHashCode();
        hash = hash * 31 + Height.GetHashCode();
        hash = hash * 31 + Depth.GetHashCode();
        foreach (var value in _values.Span)
        {
            hash = hash * 31 + value.GetHashCode();
        }
        return hash;
    }

    /// <summary>
    /// 克隆张量。
    /// </summary>
    /// <returns>一个新的张量，它是当前张量的副本。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Tensor<T> Clone()
    {
        Tensor<T> clone = new Tensor<T>(Width, Height, Depth);
        _values.CopyTo(clone._values);
        return clone;
    }


    /// <summary>
    /// 返回张量的字符串表示形式。
    /// </summary>
    /// <returns>张量的字符串表示形式。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Tensor<{typeof(T).Name}> [Width={Width}, Height={Height}, Depth={Depth}]");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    sb.AppendLine($"[{x}, {y}, {z}] = {this[x, y, z]}");
                }
            }
        }
        return sb.ToString();
    }
}
