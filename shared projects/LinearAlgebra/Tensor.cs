namespace Vorcyc.Mathematics.LinearAlgebra;


///<summary>3维张量，数据类型为 <see cref="float"/>。</summary>
public class Tensor
{
    private readonly float[] _values;

    ///<summary>使用指定的大小初始化张量。</summary>
    ///<param name="w">宽度。</param>
    ///<param name="h">高度。</param>
    ///<param name="d">深度。</param>
    public Tensor(int w, int h, int d)
    {
        if (w <= 0 || h <= 0 || d <= 0)
        {
            throw new ArgumentException("维度必须是正数且非零。");
        }

        this._values = new float[w * h * d];
        this.Width = w;
        this.Height = h;
        this.Depth = d;
    }

    ///<summary>使用指定的大小和初始值初始化张量。</summary>
    ///<param name="w">宽度。</param>
    ///<param name="h">高度。</param>
    ///<param name="d">深度。</param>
    ///<param name="initialValue">所有元素的初始值。</param>
    public Tensor(int w, int h, int d, float initialValue) : this(w, h, d)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = initialValue;
        }
    }

    ///<summary>从三维数组初始化张量。</summary>
    ///<param name="array">用于初始化张量的三维数组。</param>
    public Tensor(float[,,] array)
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

    ///<summary>张量的值。</summary>
    public float[] Values => _values;

    ///<summary>宽度。</summary>
    public int Width { get; }

    ///<summary>高度。</summary>
    public int Height { get; }

    ///<summary>深度。</summary>
    public int Depth { get; }

    /// <summary>
    /// 获取或设置指定坐标的值。
    /// </summary>
    /// <param name="x">X 坐标（宽度）。</param>
    /// <param name="y">Y 坐标（高度）。</param>
    /// <param name="z">Z 坐标（深度）。</param>
    /// <returns>指定坐标的值。</returns>
    public float this[int x, int y, int z]
    {
        get
        {
            ValidateIndices(x, y, z);
            return this._values[((this.Width * y) + x) * this.Depth + z];
        }
        set
        {
            ValidateIndices(x, y, z);
            this._values[((this.Width * y) + x) * this.Depth + z] = value;
        }
    }

    /// <summary>
    /// 验证提供的索引。
    /// </summary>
    /// <param name="x">X 坐标（宽度）。</param>
    /// <param name="y">Y 坐标（高度）。</param>
    /// <param name="z">Z 坐标（深度）。</param>
    private void ValidateIndices(int x, int y, int z)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
        {
            throw new ArgumentOutOfRangeException("索引超出范围。");
        }
    }

    /// <summary>
    /// 用指定的值填充张量。
    /// </summary>
    /// <param name="value">用于填充张量的值。</param>
    public void Fill(float value)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = value;
        }
    }

    #region operators inline

    /// <summary>
    /// 将另一个张量加到当前张量。
    /// </summary>
    /// <param name="other">要相加的张量。</param>
    public void Add(Tensor other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("张量维度必须匹配。");
        }

        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] += other._values[i];
        }
    }

    /// <summary>
    /// 从当前张量中减去另一个张量。
    /// </summary>
    /// <param name="other">要减去的张量。</param>
    public void Subtract(Tensor other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("张量维度必须匹配。");
        }

        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] -= other._values[i];
        }
    }

    /// <summary>
    /// 将当前张量乘以一个标量值。
    /// </summary>
    /// <param name="scalar">要乘以的标量值。</param>
    public void Multiply(float scalar)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] *= scalar;
        }
    }

    #endregion

    /// <summary>
    /// 克隆张量。
    /// </summary>
    /// <returns>一个新的张量，它是当前张量的副本。</returns>
    public Tensor Clone()
    {
        Tensor clone = new Tensor(Width, Height, Depth);
        Array.Copy(_values, clone._values, _values.Length);
        return clone;
    }

    /// <summary>
    /// 返回张量的字符串表示形式。
    /// </summary>
    /// <returns>张量的字符串表示形式。</returns>
    public override string ToString()
    {
        return $"Tensor<float> [Width={Width}, Height={Height}, Depth={Depth}]";
    }

    #region operators

    /// <summary>
    /// 将两个张量相加。
    /// </summary>
    /// <param name="a">第一个张量。</param>
    /// <param name="b">第二个张量。</param>
    /// <returns>两个张量相加的结果。</returns>
    public static Tensor operator +(Tensor a, Tensor b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("张量维度必须匹配。");
        }

        Tensor result = new Tensor(a.Width, a.Height, a.Depth);
        for (int i = 0; i < a._values.Length; i++)
        {
            result._values[i] = a._values[i] + b._values[i];
        }
        return result;
    }

    /// <summary>
    /// 将一个张量从另一个张量中减去。
    /// </summary>
    /// <param name="a">第一个张量。</param>
    /// <param name="b">第二个张量。</param>
    /// <returns>从第一个张量中减去第二个张量的结果。</returns>
    public static Tensor operator -(Tensor a, Tensor b)
    {
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            throw new ArgumentException("张量维度必须匹配。");
        }

        Tensor result = new Tensor(a.Width, a.Height, a.Depth);
        for (int i = 0; i < a._values.Length; i++)
        {
            result._values[i] = a._values[i] - b._values[i];
        }
        return result;
    }

    /// <summary>
    /// 将张量乘以一个标量值。
    /// </summary>
    /// <param name="tensor">张量。</param>
    /// <param name="scalar">标量值。</param>
    /// <returns>张量乘以标量值的结果。</returns>
    public static Tensor operator *(Tensor tensor, float scalar)
    {
        Tensor result = new Tensor(tensor.Width, tensor.Height, tensor.Depth);
        for (int i = 0; i < tensor._values.Length; i++)
        {
            result._values[i] = tensor._values[i] * scalar;
        }
        return result;
    }

    /// <summary>
    /// 将张量乘以一个标量值。
    /// </summary>
    /// <param name="scalar">标量值。</param>
    /// <param name="tensor">张量。</param>
    /// <returns>张量乘以标量值的结果。</returns>
    public static Tensor operator *(float scalar, Tensor tensor)
    {
        return tensor * scalar;
    }

    #endregion

    /// <summary>
    /// 沿指定轴转置张量。
    /// </summary>
    /// <param name="axis1">第一个轴。</param>
    /// <param name="axis2">第二个轴。</param>
    /// <returns>转置后的新张量。</returns>
    public Tensor Transpose(int axis1, int axis2)
    {
        if (axis1 < 0 || axis1 > 2 || axis2 < 0 || axis2 > 2 || axis1 == axis2)
        {
            throw new ArgumentException("无效的转置轴。");
        }

        int[] dims = { Width, Height, Depth };
        int newWidth = dims[axis1];
        int newHeight = dims[axis2];
        int newDepth = dims[3 - axis1 - axis2];

        Tensor result = new Tensor(newWidth, newHeight, newDepth);

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
    /// 计算两个张量的点积。
    /// </summary>
    /// <param name="other">另一个张量。</param>
    /// <returns>两个张量的点积。</returns>
    public float Dot(Tensor other)
    {
        if (this.Width != other.Width || this.Height != other.Height || this.Depth != other.Depth)
        {
            throw new ArgumentException("张量维度必须匹配。");
        }

        float dotProduct = 0;
        for (int i = 0; i < _values.Length; i++)
        {
            dotProduct += _values[i] * other._values[i];
        }
        return dotProduct;
    }

    /// <summary>
    /// 归一化张量。
    /// </summary>
    public void Normalize()
    {
        float norm = Norm();
        if (norm == 0)
        {
            throw new InvalidOperationException("无法归一化范数为零的张量。");
        }

        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] /= norm;
        }
    }

    /// <summary>
    /// 计算张量的范数。
    /// </summary>
    /// <returns>张量的范数。</returns>
    public float Norm()
    {
        float sumOfSquares = 0;
        for (int i = 0; i < _values.Length; i++)
        {
            sumOfSquares += _values[i] * _values[i];
        }
        return MathF.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// 沿指定轴切片张量。
    /// </summary>
    /// <param name="axis">要切片的轴。</param>
    /// <param name="index">切片的索引。</param>
    /// <returns>切片后的新张量。</returns>
    public Tensor Slice(int axis, int index)
    {
        if (axis < 0 || axis > 2)
        {
            throw new ArgumentException("无效的切片轴。");
        }

        int[] dims = { Width, Height, Depth };
        if (index < 0 || index >= dims[axis])
        {
            throw new ArgumentOutOfRangeException("索引超出范围。");
        }

        int newWidth = axis == 0 ? 1 : Width;
        int newHeight = axis == 1 ? 1 : Height;
        int newDepth = axis == 2 ? 1 : Depth;

        Tensor result = new Tensor(newWidth, newHeight, newDepth);

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
    /// <returns>如果张量相等，则为 true，否则为 false。</returns>
    public static bool operator ==(Tensor a, Tensor b)
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
    /// 检查两个张量是否不相等。
    /// </summary>
    /// <param name="a">第一个张量。</param>
    /// <param name="b">第二个张量。</param>
    /// <returns>如果张量不相等，则为 true，否则为 false。</returns>
    public static bool operator !=(Tensor a, Tensor b)
    {
        return !(a == b);
    }

    /// <summary>
    /// 确定指定对象是否等于当前对象。
    /// </summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定对象等于当前对象，则为 true，否则为 false。</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Tensor other)
        {
            return this == other;
        }
        return false;
    }

    /// <summary>
    /// 作为默认的哈希函数。
    /// </summary>
    /// <returns>当前对象的哈希代码。</returns>
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
