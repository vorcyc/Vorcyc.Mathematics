using System.Numerics;

namespace Vorcyc.Mathematics.LinearAlgebra;

///<summary>3-dimentional tensor of <see cref="IBinaryFloatingPointIeee754{TSelf}"/> data type.</summary>
public class Tensor<T>
    where T : IBinaryFloatingPointIeee754<T>
{


    private readonly T[] _values;

    ///<summary>Initializes the Tensor with specified size.</summary>
    ///<param name="w">Width.</param>
    ///<param name="h">Height.</param>
    ///<param name="d">Depth.</param>
    public Tensor(int w, int h, int d)
    {
        this._values = new T[w * h * d];
        this.Width = w;
        this.Height = h;
        this.Depth = d;
    }

    ///<summary>Values.</summary>
    public T[] W => _values;

    ///<summary>Width.</summary>
    public int Width { get; }

    ///<summary>Height.</summary>
    public int Height { get; }

    ///<summary>Depth.</summary>
    public int Depth { get; }


    /// <summary>
    /// Gets or sets the value
    /// </summary>
    /// <param name="x">X coordinate (По ширине).</param>
    /// <param name="y">Y coordinate (По высоте).</param>
    /// <param name="z">Z coordinate (По высоте).</param>
    /// <returns></returns>
    public T this[int x, int y, int z]
    {
        get => this._values[((this.Width * y) + x) * this.Depth + z];
        set => this._values[((this.Width * y) + x) * this.Depth + z] = value;
    }
}
