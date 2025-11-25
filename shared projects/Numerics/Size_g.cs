namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// 表示一个具有宽度和高度的二维尺寸结构体。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 <see cref="System.Numerics.INumber{T}"/> 接口。</typeparam>
public readonly struct Size<T>
    where T : System.Numerics.INumber<T>
{
    /// <summary>
    /// 获取宽度。
    /// </summary>
    public T Width { get; }

    /// <summary>
    /// 获取高度。
    /// </summary>
    public T Height { get; }

    /// <summary>
    /// 初始化 <see cref="Size{T}"/> 结构体的新实例，该实例具有指定的宽度和高度。
    /// </summary>
    /// <param name="width">宽度。</param>
    /// <param name="height">高度。</param>
    public Size(T width, T height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 将当前实例解构为宽度和高度。
    /// </summary>
    /// <param name="width">宽度。</param>
    /// <param name="height">高度。</param>
    public void Deconstruct(out T width, out T height)
    {
        width = Width;
        height = Height;
    }

    /// <summary>
    /// 实现尺寸与值的加法运算。
    /// </summary>
    /// <param name="size">尺寸。</param>
    /// <param name="value">值。</param>
    /// <returns>新的 <see cref="Size{T}"/> 实例，其宽度和高度分别加上指定的值。</returns>
    public static Size<T> operator +(Size<T> size, T value) => new Size<T>(size.Width + value, size.Height + value);

    /// <summary>
    /// 实现尺寸与值的减法运算。
    /// </summary>
    /// <param name="size">尺寸。</param>
    /// <param name="value">值。</param>
    /// <returns>新的 <see cref="Size{T}"/> 实例，其宽度和高度分别减去指定的值。</returns>
    public static Size<T> operator -(Size<T> size, T value) => new Size<T>(size.Width - value, size.Height - value);

    /// <summary>
    /// 实现尺寸与值的乘法运算。
    /// </summary>
    /// <param name="size">尺寸。</param>
    /// <param name="value">值。</param>
    /// <returns>新的 <see cref="Size{T}"/> 实例，其宽度和高度分别乘以指定的值。</returns>
    public static Size<T> operator *(Size<T> size, T value) => new Size<T>(size.Width * value, size.Height * value);

    /// <summary>
    /// 实现尺寸与值的除法运算。
    /// </summary>
    /// <param name="size">尺寸。</param>
    /// <param name="value">值。</param>
    /// <returns>新的 <see cref="Size{T}"/> 实例，其宽度和高度分别除以指定的值。</returns>
    public static Size<T> operator /(Size<T> size, T value) => new Size<T>(size.Width / value, size.Height / value);

    /// <summary>
    /// 返回表示当前对象的字符串。
    /// </summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString() => $"({Width},{Height})";
}
