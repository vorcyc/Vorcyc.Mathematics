﻿using System.Collections;
using System.Numerics;
using System.Text;

namespace Vorcyc.Mathematics.Framework.DataHelper;

/// <summary>
/// 表示数据表中的一行，包含指定数量的列值，并带有行索引。
/// </summary>
/// <typeparam name="T">值的类型，必须实现 INumber{T} 接口。</typeparam>
public class DataRow<T>
    where T : INumber<T>
{
    private readonly T[,] _data;
    private readonly int _rowIndex;
    private readonly DataColumnCollection<T> _columns; // 用于支持列名访问

    /// <summary>
    /// 初始化 <see cref="DataRow{T}"/> 类的新实例。
    /// </summary>
    /// <param name="data">数据表的共享数据数组。</param>
    /// <param name="rowIndex">行索引。</param>
    /// <param name="columns">列集合，用于支持按列名访问。</param>
    /// <exception cref="ArgumentNullException">如果 <paramref name="data"/> 或 <paramref name="columns"/> 为 null。</exception>
    public DataRow(T[,] data, int rowIndex, DataColumnCollection<T> columns)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        _rowIndex = rowIndex;
    }

    /// <summary>
    /// 获取或设置指定列索引处的值。
    /// </summary>
    /// <param name="index">列索引。</param>
    /// <returns>指定列索引处的值的引用。</returns>
    /// <exception cref="IndexOutOfRangeException">如果列索引超出范围。</exception>
    public ref T this[int index]
    {
        get
        {
            ValidateColumnIndex(index);
            return ref _data[_rowIndex, index];
        }
    }

    /// <summary>
    /// 获取或设置指定列名处的值。
    /// </summary>
    /// <param name="columnName">列名。</param>
    /// <returns>指定列名处的值的引用。</returns>
    /// <exception cref="ArgumentException">如果列名未找到。</exception>
    public ref T this[string columnName]
    {
        get
        {
            var column = _columns[columnName]; // 获取对应的 DataColumn<T>
            return ref _data[_rowIndex, column.ColumnIndex];
        }
    }

    private void ValidateColumnIndex(int index)
    {
        if (index < 0 || index >= _data.GetLength(1))
            throw new IndexOutOfRangeException($"列索引 {index} 超出范围 [0, {_data.GetLength(1) - 1}]");
    }

    /// <summary>
    /// 获取行的列数。
    /// </summary>
    public int Length => _data.GetLength(1);

    /// <summary>
    /// 获取行索引。
    /// </summary>
    public int RowIndex => _rowIndex;

    /// <summary>
    /// 返回表示当前行的字符串。
    /// </summary>
    /// <returns>表示当前行的字符串，格式为 "Row {RowIndex}: [{值1}, {值2}, ...]"。</returns>
    public override string ToString()
    {
        var sb = new StringBuilder($"Row {RowIndex}: [");
        for (int i = 0; i < Length; i++)
        {
            sb.Append(_data[_rowIndex, i]);
            if (i < Length - 1) sb.Append(", ");
        }
        sb.Append("]");
        return sb.ToString();
    }
}

/// <summary>
/// 表示数据表中的一列，包含指定数量的行值，并带有列索引和可选的名称。
/// </summary>
/// <typeparam name="T">值的类型，必须实现 INumber{T} 接口。</typeparam>
public class DataColumn<T>
    where T : INumber<T>
{
    private readonly T[,] _data;
    private readonly int _columnIndex;

    /// <summary>
    /// 初始化 <see cref="DataColumn{T}"/> 类的新实例。
    /// </summary>
    /// <param name="data">数据表的共享数据数组。</param>
    /// <param name="columnIndex">列索引。</param>
    /// <param name="name">列名称。</param>
    /// <exception cref="ArgumentNullException">如果 <paramref name="data"/> 为 null。</exception>
    public DataColumn(T[,] data, int columnIndex, string? name = null)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _columnIndex = columnIndex;
        Name = name;
    }

    /// <summary>
    /// 获取或设置指定行索引处的值。
    /// </summary>
    /// <param name="index">行索引。</param>
    /// <returns>指定行索引处的值的引用。</returns>
    /// <exception cref="IndexOutOfRangeException">如果行索引超出范围。</exception>
    public ref T this[int index]
    {
        get
        {
            ValidateRowIndex(index);
            return ref _data[index, _columnIndex];
        }
    }

    private void ValidateRowIndex(int index)
    {
        if (index < 0 || index >= _data.GetLength(0))
            throw new IndexOutOfRangeException($"行索引 {index} 超出范围 [0, {_data.GetLength(0) - 1}]");
    }

    /// <summary>
    /// 获取列的行数。
    /// </summary>
    public int Length => _data.GetLength(0);

    /// <summary>
    /// 获取列索引。
    /// </summary>
    public int ColumnIndex => _columnIndex;

    /// <summary>
    /// 获取列名称。
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// 返回表示当前列的字符串。
    /// </summary>
    /// <returns>表示当前列的字符串，格式为 "{Name 或 Column{ColumnIndex}} (Index {ColumnIndex}): [{值1}, {值2}, ...]"。</returns>
    public override string ToString()
    {
        var sb = new StringBuilder($"{Name ?? $"Column{ColumnIndex}"} (Index {ColumnIndex}): [");
        for (int i = 0; i < Length; i++)
        {
            sb.Append(_data[i, _columnIndex]);
            if (i < Length - 1) sb.Append(", ");
        }
        sb.Append("]");
        return sb.ToString();
    }
}


/// <summary>
/// 表示数据表中的行集合，管理一组 <see cref="DataRow{T}"/> 对象。
/// </summary>
/// <typeparam name="T">值的类型，必须实现 INumber{T} 接口。</typeparam>
public class DataRowCollection<T> : IEnumerable<DataRow<T>>
    where T : INumber<T>
{
    private DataRow<T>[] _rows;
    private T[,] _data;
    private readonly DataColumnCollection<T> _columns;

    /// <summary>
    /// 初始化 <see cref="DataRowCollection{T}"/> 类的新实例。
    /// </summary>
    /// <param name="data">数据表的共享数据数组。</param>
    /// <param name="rowCount">行数。</param>
    /// <param name="columns">列集合，用于支持按列名访问。</param>
    /// <exception cref="ArgumentException">如果 <paramref name="rowCount"/> 小于 0。</exception>
    /// <exception cref="ArgumentNullException">如果 <paramref name="data"/> 或 <paramref name="columns"/> 为 null。</exception>
    public DataRowCollection(T[,] data, int rowCount, DataColumnCollection<T> columns)
    {
        if (rowCount < 0)
            throw new ArgumentException("行数必须是非负数", nameof(rowCount));
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        if (columns == null)
            throw new ArgumentNullException(nameof(columns));

        _data = data;
        _columns = columns;
        _rows = new DataRow<T>[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            _rows[i] = new DataRow<T>(_data, i, _columns);
        }
    }

    /// <summary>
    /// 获取指定索引处的行。
    /// </summary>
    /// <param name="index">行索引。</param>
    /// <returns>指定索引处的行。</returns>
    /// <exception cref="IndexOutOfRangeException">如果行索引超出范围。</exception>
    public DataRow<T> this[int index]
    {
        get
        {
            if (index < 0 || index >= _rows.Length)
                throw new IndexOutOfRangeException($"行索引 {index} 超出范围 [0, {_rows.Length - 1}]");
            return _rows[index];
        }
    }

    /// <summary>
    /// 获取行数。
    /// </summary>
    public int Count => _rows.Length;

    /// <summary>
    /// 添加新行。
    /// </summary>
    /// <param name="defaultValue">新行的默认值（可选）。</param>
    public void AddRow(T? defaultValue = default)
    {
        int newRowCount = Count + 1;
        var newData = new T[newRowCount, _data.GetLength(1)];

        // 复制现有数据
        for (int i = 0; i < Count; i++)
            for (int j = 0; j < _data.GetLength(1); j++)
                newData[i, j] = _data[i, j];

        // 初始化新行
        for (int j = 0; j < _data.GetLength(1); j++)
            newData[newRowCount - 1, j] = defaultValue ?? default(T);

        // 更新数据和行集合
        _data = newData;
        Array.Resize(ref _rows, newRowCount);
        _rows[newRowCount - 1] = new DataRow<T>(_data, newRowCount - 1, _columns);

        // 更新列集合的底层数据引用
        _columns.UpdateData(_data);
    }

    /// <summary>
    /// 删除指定索引处的行。
    /// </summary>
    /// <param name="rowIndex">要删除的行索引。</param>
    /// <exception cref="IndexOutOfRangeException">如果行索引超出范围。</exception>
    /// <exception cref="InvalidOperationException">如果尝试删除最后一行。</exception>
    public void RemoveRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= Count)
            throw new IndexOutOfRangeException($"行索引 {rowIndex} 超出范围 [0, {Count - 1}]");
        if (Count <= 1)
            throw new InvalidOperationException("无法删除最后一行");

        int newRowCount = Count - 1;
        var newData = new T[newRowCount, _data.GetLength(1)];

        // 复制数据并跳过被删除的行
        int newRow = 0;
        for (int i = 0; i < Count; i++)
            if (i != rowIndex)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                    newData[newRow, j] = _data[i, j];
                newRow++;
            }

        // 更新数据和行集合
        _data = newData;
        var newRows = new DataRow<T>[newRowCount];
        for (int i = 0; i < newRowCount; i++)
            newRows[i] = new DataRow<T>(_data, i, _columns);
        _rows = newRows;

        // 更新列集合的底层数据引用
        _columns.UpdateData(_data);
    }

    /// <summary>
    /// 返回表示当前行集合的字符串。
    /// </summary>
    /// <returns>表示当前行集合的字符串。如果为空，返回 "DataRowCollection: Empty"；否则返回每行的字符串表示，按行排列。</returns>
    public override string ToString()
    {
        if (Count == 0)
            return "DataRowCollection: Empty";
        return $"DataRowCollection ({Count} rows):\n{string.Join("\n", _rows.Select(r => r.ToString()))}";
    }

    public IEnumerator<DataRow<T>> GetEnumerator() => ((IEnumerable<DataRow<T>>)_rows).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _rows.GetEnumerator();
}


/// <summary>
/// 表示数据表中的列集合，管理一组 <see cref="DataColumn{T}"/> 对象，并支持按名称访问。
/// </summary>
/// <typeparam name="T">值的类型，必须实现 INumber{T} 接口。</typeparam>
public class DataColumnCollection<T> : IEnumerable<DataColumn<T>>
    where T : INumber<T>
{
    private DataColumn<T>[] _columns;
    private T[,] _data;
    private readonly Dictionary<string, DataColumn<T>> _nameToColumn;

    /// <summary>
    /// 初始化 <see cref="DataColumnCollection{T}"/> 类的新实例。
    /// </summary>
    /// <param name="data">数据表的共享数据数组。</param>
    /// <param name="columnCount">列数。</param>
    /// <param name="columnNames">列名称数组。</param>
    /// <exception cref="ArgumentException">如果 <paramref name="columnCount"/> 小于 0。</exception>
    /// <exception cref="ArgumentNullException">如果 <paramref name="data"/> 为 null。</exception>
    public DataColumnCollection(T[,] data, int columnCount, string[]? columnNames = null)
    {
        if (columnCount < 0)
            throw new ArgumentException("列数必须是非负数", nameof(columnCount));
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        _data = data;
        _columns = new DataColumn<T>[columnCount];
        _nameToColumn = new Dictionary<string, DataColumn<T>>(columnCount);

        for (int i = 0; i < columnCount; i++)
        {
            string? name = columnNames != null && i < columnNames.Length ? columnNames[i] : null;
            _columns[i] = new DataColumn<T>(_data, i, name);
            if (name != null)
                _nameToColumn[name] = _columns[i];
        }
    }

    /// <summary>
    /// 获取指定索引处的列。
    /// </summary>
    /// <param name="index">列索引。</param>
    /// <returns>指定索引处的列。</returns>
    /// <exception cref="IndexOutOfRangeException">如果列索引超出范围。</exception>
    public DataColumn<T> this[int index]
    {
        get
        {
            if (index < 0 || index >= _columns.Length)
                throw new IndexOutOfRangeException($"列索引 {index} 超出范围 [0, {_columns.Length - 1}]");
            return _columns[index];
        }
    }

    /// <summary>
    /// 获取指定名称的列。
    /// </summary>
    /// <param name="name">列名称。</param>
    /// <returns>指定名称的列。</returns>
    /// <exception cref="ArgumentException">如果未找到指定名称的列。</exception>
    public DataColumn<T> this[string name]
    {
        get
        {
            if (!_nameToColumn.TryGetValue(name, out var column))
                throw new ArgumentException($"未找到名为 '{name}' 的列");
            return column;
        }
    }

    /// <summary>
    /// 获取列数。
    /// </summary>
    public int Count => _columns.Length;

    /// <summary>
    /// 添加新列。
    /// </summary>
    /// <param name="columnName">新列的名称（可选）。</param>
    /// <param name="defaultValue">新列的默认值（可选）。</param>
    public void AddColumn(string? columnName = null, T? defaultValue = default)
    {
        int newColumnCount = Count + 1;
        var newData = new T[_data.GetLength(0), newColumnCount];

        // 复制现有数据
        for (int i = 0; i < _data.GetLength(0); i++)
            for (int j = 0; j < Count; j++)
                newData[i, j] = _data[i, j];

        // 初始化新列
        for (int i = 0; i < _data.GetLength(0); i++)
            newData[i, newColumnCount - 1] = defaultValue ?? default(T);

        // 更新数据和列集合
        _data = newData;
        Array.Resize(ref _columns, newColumnCount);
        _columns[newColumnCount - 1] = new DataColumn<T>(_data, newColumnCount - 1, columnName);
        if (columnName != null)
            _nameToColumn[columnName] = _columns[newColumnCount - 1];
    }

    /// <summary>
    /// 删除指定索引处的列。
    /// </summary>
    /// <param name="columnIndex">要删除的列索引。</param>
    /// <exception cref="IndexOutOfRangeException">如果列索引超出范围。</exception>
    /// <exception cref="InvalidOperationException">如果尝试删除最后一列。</exception>
    public void RemoveColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= Count)
            throw new IndexOutOfRangeException($"列索引 {columnIndex} 超出范围 [0, {Count - 1}]");
        if (Count <= 1)
            throw new InvalidOperationException("无法删除最后一列");

        int newColumnCount = Count - 1;
        var newData = new T[_data.GetLength(0), newColumnCount];

        // 复制数据并跳过被删除的列
        for (int i = 0; i < _data.GetLength(0); i++)
        {
            int newCol = 0;
            for (int j = 0; j < Count; j++)
                if (j != columnIndex)
                    newData[i, newCol++] = _data[i, j];
        }

        // 更新列集合
        string? removedName = _columns[columnIndex].Name;
        var newColumns = new DataColumn<T>[newColumnCount];
        int newIndex = 0;
        for (int j = 0; j < Count; j++)
            if (j != columnIndex)
                newColumns[newIndex++] = new DataColumn<T>(_data, newIndex - 1, _columns[j].Name);
        _columns = newColumns;

        // 更新名称映射
        _nameToColumn.Clear();
        foreach (var col in _columns)
            if (col.Name != null)
                _nameToColumn[col.Name] = col;

        _data = newData;
    }

    /// <summary>
    /// 删除指定名称的列。
    /// </summary>
    /// <param name="columnName">要删除的列名称。</param>
    /// <exception cref="ArgumentException">如果未找到指定名称的列。</exception>
    /// <exception cref="InvalidOperationException">如果尝试删除最后一列。</exception>
    public void RemoveColumn(string columnName)
    {
        if (!_nameToColumn.TryGetValue(columnName, out var column))
            throw new ArgumentException($"未找到名为 '{columnName}' 的列");

        // 调用基于索引的 RemoveColumn 方法
        RemoveColumn(column.ColumnIndex);
    }

    /// <summary>
    /// 更新底层数据引用（供 DataRowCollection 调用）。
    /// </summary>
    /// <param name="newData">新的数据数组。</param>
    internal void UpdateData(T[,] newData)
    {
        _data = newData;
        for (int i = 0; i < Count; i++)
            _columns[i] = new DataColumn<T>(_data, i, _columns[i].Name);
    }

    /// <summary>
    /// 返回表示当前列集合的字符串。
    /// </summary>
    /// <returns>表示当前列集合的字符串。如果为空，返回 "DataColumnCollection: Empty"；否则返回每列的字符串表示，按列排列。</returns>
    public override string ToString()
    {
        if (Count == 0)
            return "DataColumnCollection: Empty";
        return $"DataColumnCollection ({Count} columns):\n{string.Join("\n", _columns.Select(c => c.ToString()))}";
    }

    public IEnumerator<DataColumn<T>> GetEnumerator() => ((IEnumerable<DataColumn<T>>)_columns).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _columns.GetEnumerator();
}

/// <summary>
/// 定义过滤条件类型。
/// </summary>
public enum FilterCondition
{
    GreaterThan,
    GreaterThanOrEqual,
    Equal,
    LessThan,
    LessThanOrEqual,
    NotEqual
}

/// <summary>
/// 表示一个二维数据表，包含行和列的集合。
/// </summary>
/// <typeparam name="T">值的类型，必须实现 INumber{T} 接口。</typeparam>
public class DataTable<T>
    where T : INumber<T>
{
    private readonly T[,] _data;
    private readonly DataRowCollection<T> _rows;
    private readonly DataColumnCollection<T> _columns;

    /// <summary>
    /// 初始化 <see cref="DataTable{T}"/> 类的新实例。
    /// </summary>
    /// <param name="rowCount">行数。</param>
    /// <param name="columnCount">列数。</param>
    /// <param name="columnNames">列名称数组。</param>
    /// <exception cref="ArgumentException">如果 <paramref name="rowCount"/> 或 <paramref name="columnCount"/> 小于 0，或 <paramref name="columnNames"/> 的长度与 <paramref name="columnCount"/> 不匹配。</exception>
    internal DataTable(int rowCount, int columnCount, string[]? columnNames = null)
    {
        if (rowCount < 0 || columnCount < 0)
            throw new ArgumentException("行数和列数必须是非负数");
        if (columnNames != null && columnNames.Length != columnCount)
            throw new ArgumentException("列名数量必须与列数匹配", nameof(columnNames));

        _data = new T[rowCount, columnCount];
        _columns = new DataColumnCollection<T>(_data, columnCount, columnNames);
        _rows = new DataRowCollection<T>(_data, rowCount, _columns); // 传递列集合
    }

    /// <summary>
    /// 获取行集合。
    /// </summary>
    public DataRowCollection<T> Rows => _rows;

    /// <summary>
    /// 获取列集合。
    /// </summary>
    public DataColumnCollection<T> Columns => _columns;

    /// <summary>
    /// 获取行数。
    /// </summary>
    public int RowCount => _rows.Count;

    /// <summary>
    /// 获取列数。
    /// </summary>
    public int ColumnCount => _columns.Count;

    /// <summary>
    /// 获取或设置指定行索引和列索引处的值。
    /// </summary>
    /// <param name="rowIndex">行索引。</param>
    /// <param name="columnIndex">列索引。</param>
    /// <returns>指定行索引和列索引处的值的引用。</returns>
    /// <exception cref="IndexOutOfRangeException">如果行索引或列索引超出范围。</exception>
    public ref T this[int rowIndex, int columnIndex]
    {
        get
        {
            if (rowIndex < 0 || rowIndex >= RowCount)
                throw new IndexOutOfRangeException($"行索引 {rowIndex} 超出范围 [0, {RowCount - 1}]");
            if (columnIndex < 0 || columnIndex >= ColumnCount)
                throw new IndexOutOfRangeException($"列索引 {columnIndex} 超出范围 [0, {ColumnCount - 1}]");
            return ref _data[rowIndex, columnIndex];
        }
    }

    /// <summary>
    /// 返回表示当前数据表的字符串。
    /// </summary>
    /// <returns>表示当前数据表的字符串，包含行数、列数以及行和列的详细信息。</returns>
    public override string ToString()
    {
        return $"DataTable ({RowCount} rows, {ColumnCount} columns):\nRows:\n{_rows}\nColumns:\n{_columns}";
    }
}

