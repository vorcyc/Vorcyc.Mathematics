using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Framework.DataHelper;


/// <summary>
/// 为 <see cref="DataTable{T}"/> 提供扩展方法。
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// 将 <see cref="DataTable{T}"/> 转换为 <see cref="Matrix{T}"/>。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现 <see cref="INumber{T}"/> 和 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
    /// <param name="table">要转换的 <see cref="DataTable{T}"/> 实例。</param>
    /// <returns>转换后的 <see cref="Matrix{T}"/> 实例。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="table"/> 为 null 时抛出。</exception>
    /// <exception cref="InvalidOperationException">当 <paramref name="table"/> 为空（无行或无列）时抛出。</exception>
    public static Matrix<T> ToMatrix<T>(this DataTable<T> table)
        where T : struct, INumber<T>, IFloatingPointIeee754<T>
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));
        if (table.RowCount == 0 || table.ColumnCount == 0)
            throw new InvalidOperationException("数据表为空，无法转换为矩阵。");

        int rows = table.RowCount;
        int cols = table.ColumnCount;
        var matrix = new Matrix<T>(rows, cols);
        var tableData = table.GetInternalData(); // T[,]
        var matrixData = matrix.GetInternalData(); // T[]

        // 逐行复制，使用临时一维数组
        var tempRow = new T[cols];
        for (int i = 0; i < rows; i++)
        {
            // 从二维数组的第 i 行复制到临时数组
            for (int j = 0; j < cols; j++)
            {
                tempRow[j] = tableData[i, j];
            }
            // 将临时数组复制到矩阵的对应位置
            Array.Copy(tempRow, 0, matrixData, i * cols, cols);
        }

        return matrix;
    }
}