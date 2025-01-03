namespace Vorcyc.Mathematics.Framework;

/// <summary>
/// Creates a copy of specified <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICloneable<T>
{
    /// <summary>
    /// Creates a copy .
    /// </summary>
    /// <returns></returns>
    T Clone();
}
