namespace Vorcyc.Mathematics.Numerics;

public readonly struct Size<T>
    where T : System.Numerics.INumber<T>
{

    private readonly T _width;
    private readonly T _height;

    public Size(T width, T height)
    {
        _width = width;
        _height = height;
    }

    public T Width => _width;

    public T Height => _height;

    public void Deconstruct(out T width, out T height)
    {
        width = _width;
        height = _height;
    }


    public static Size<T> operator +(Size<T> size, T value)
    {
        return new Size<T>(size.Width + value, size.Height + value);
    }

    public static Size<T> operator -(Size<T> size, T value)
    {
        return new Size<T>(size.Width - value, size.Height - value);
    }

    public static Size<T> operator *(Size<T> size, T value)
    {
        return new Size<T>(size.Width * value, size.Height * value);
    }

    public static Size<T> operator /(Size<T> size, T value)
    {
        return new Size<T>(size.Width / value, size.Height / value);
    }

    public override string ToString() => $"({_width},{_height})";

}
