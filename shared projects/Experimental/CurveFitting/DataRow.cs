using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;



/// <summary>
/// 表示一行多列的数据。
/// </summary>
/// <typeparam name="T">数据类型，必须支持IEEE 754浮点运算。</typeparam>
public readonly struct DataRow<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly T[] _columns;

    /// <summary>
    /// 初始化一行数据。
    /// </summary>
    /// <param name="columns">列数据。</param>
    public DataRow(params T[] columns)
    {
        _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        if (columns.Length == 0)
            throw new ArgumentException("Row must contain at least one column.");
    }

    /// <summary>
    /// 获取指定列的值。
    /// </summary>
    public T this[int index] => _columns[index];

    /// <summary>
    /// 列数。
    /// </summary>
    public int ColumnCount => _columns.Length;

    /// <summary>
    /// 将行数据转换为数组。
    /// </summary>
    public T[] ToArray() => _columns.ToArray();
}