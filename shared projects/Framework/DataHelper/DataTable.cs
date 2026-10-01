using System.Collections;
using System.Numerics;
using System.Text;

namespace Vorcyc.Mathematics.Framework.DataHelper;

/// <summary>
/// Represents a row in a data table, containing a specified number of column values along with a row index.
/// </summary>
/// <typeparam name="T">The value type, must implement the INumber{T} interface.</typeparam>
public class DataRow<T>
    where T : INumber<T>
{
    private readonly T[,] _data;
    private readonly int _rowIndex;
    private readonly DataColumnCollection<T> _columns; // 用于支持列名访问

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRow{T}"/> class.
    /// </summary>
    /// <param name="data">The shared data array of the data table.</param>
    /// <param name="rowIndex">The row index.</param>
    /// <param name="columns">The column collection, used to support access by column name.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="columns"/> is null.</exception>
    public DataRow(T[,] data, int rowIndex, DataColumnCollection<T> columns)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        _rowIndex = rowIndex;
    }

    /// <summary>
    /// Gets or sets the value at the specified column index.
    /// </summary>
    /// <param name="index">The column index.</param>
    /// <returns>A reference to the value at the specified column index.</returns>
    /// <exception cref="IndexOutOfRangeException">If the column index is out of range.</exception>
    public ref T this[int index]
    {
        get
        {
            ValidateColumnIndex(index);
            return ref _data[_rowIndex, index];
        }
    }

    /// <summary>
    /// Gets or sets the value at the specified column name.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <returns>A reference to the value at the specified column name.</returns>
    /// <exception cref="ArgumentException">If the column name is not found.</exception>
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
            throw new IndexOutOfRangeException($"Column index {index} is out of range [0, {_data.GetLength(1) - 1}].");
    }

    /// <summary>
    /// Gets the number of columns in the row.
    /// </summary>
    public int Length => _data.GetLength(1);

    /// <summary>
    /// Gets the row index.
    /// </summary>
    public int RowIndex => _rowIndex;

    /// <summary>
    /// Returns a string representing the current row.
    /// </summary>
    /// <returns>A string representing the current row, formatted as "Row {RowIndex}: [{value1}, {value2}, ...]".</returns>
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
/// Represents a column in a data table, containing a specified number of row values along with a column index and an optional name.
/// </summary>
/// <typeparam name="T">The value type, must implement the INumber{T} interface.</typeparam>
public class DataColumn<T>
    where T : INumber<T>
{
    private readonly T[,] _data;
    private readonly int _columnIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataColumn{T}"/> class.
    /// </summary>
    /// <param name="data">The shared data array of the data table.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <param name="name">The column name.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null.</exception>
    public DataColumn(T[,] data, int columnIndex, string? name = null)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _columnIndex = columnIndex;
        Name = name;
    }

    /// <summary>
    /// Gets or sets the value at the specified row index.
    /// </summary>
    /// <param name="index">The row index.</param>
    /// <returns>A reference to the value at the specified row index.</returns>
    /// <exception cref="IndexOutOfRangeException">If the row index is out of range.</exception>
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
            throw new IndexOutOfRangeException($"Row index {index} is out of range [0, {_data.GetLength(0) - 1}].");
    }

    /// <summary>
    /// Gets the number of rows in the column.
    /// </summary>
    public int Length => _data.GetLength(0);

    /// <summary>
    /// Gets the column index.
    /// </summary>
    public int ColumnIndex => _columnIndex;

    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Returns a string representing the current column.
    /// </summary>
    /// <returns>A string representing the current column, formatted as "{Name or Column{ColumnIndex}} (Index {ColumnIndex}): [{value1}, {value2}, ...]".</returns>
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
/// Represents the collection of rows in a data table, managing a set of <see cref="DataRow{T}"/> objects.
/// </summary>
/// <typeparam name="T">The value type, must implement the INumber{T} interface.</typeparam>
public class DataRowCollection<T> : IEnumerable<DataRow<T>>
    where T : INumber<T>
{
    private DataRow<T>[] _rows;
    private T[,] _data;
    private readonly DataColumnCollection<T> _columns;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRowCollection{T}"/> class.
    /// </summary>
    /// <param name="data">The shared data array of the data table.</param>
    /// <param name="rowCount">The number of rows.</param>
    /// <param name="columns">The column collection, used to support access by column name.</param>
    /// <exception cref="ArgumentException">If <paramref name="rowCount"/> is less than 0.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="columns"/> is null.</exception>
    internal DataRowCollection(T[,] data, int rowCount, DataColumnCollection<T> columns)
    {
        if (rowCount < 0)
            throw new ArgumentException("Row count must be non-negative.", nameof(rowCount));
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
    /// Gets the row at the specified index.
    /// </summary>
    /// <param name="index">The row index.</param>
    /// <returns>The row at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">If the row index is out of range.</exception>
    public DataRow<T> this[int index]
    {
        get
        {
            if (index < 0 || index >= _rows.Length)
                throw new IndexOutOfRangeException($"Row index {index} is out of range [0, {_rows.Length - 1}].");
            return _rows[index];
        }
    }

    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    public int Count => _rows.Length;

    /// <summary>
    /// Adds a new row.
    /// </summary>
    /// <param name="defaultValue">The default value for the new row (optional).</param>
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
    /// Removes the row at the specified index.
    /// </summary>
    /// <param name="rowIndex">The row index to remove.</param>
    /// <exception cref="IndexOutOfRangeException">If the row index is out of range.</exception>
    /// <exception cref="InvalidOperationException">If attempting to remove the last row.</exception>
    public void RemoveRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= Count)
            throw new IndexOutOfRangeException($"Row index {rowIndex} is out of range [0, {Count - 1}].");
        if (Count <= 1)
            throw new InvalidOperationException("Cannot remove the last row.");

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
    /// Returns a string representing the current row collection.
    /// </summary>
    /// <returns>A string representing the current row collection. If empty, returns "DataRowCollection: Empty"; otherwise returns the string representation of each row, listed by row.</returns>
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
/// Represents a collection of generic data columns, supporting access to columns by index or name.
/// </summary>
/// <typeparam name="T">The column data type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
public class DataColumnCollection<T> : IEnumerable<DataColumn<T>>
    where T : INumber<T>
{
    private DataColumn<T>[] _columns; // 存储列对象的数组
    private T[,] _data; // 底层数据数组
    private readonly Dictionary<string, DataColumn<T>> _nameToColumn; // 列名到列对象的映射
    private readonly List<string?> _columnNames; // 缓存所有列名的列表

    /// <summary>
    /// Gets the names of all columns.
    /// </summary>
    public string[]? Names => _columnNames.ToArray();

    /// <summary>
    /// Gets the number of columns in the collection.
    /// </summary>
    public int Count => _columns.Length;

    /// <summary>
    /// Initializes a new instance of <see cref="DataColumnCollection{T}"/>.
    /// </summary>
    /// <param name="data">The underlying data array.</param>
    /// <param name="columnCount">The number of columns.</param>
    /// <param name="columnNames">The array of column names (optional).</param>
    /// <exception cref="ArgumentException">Thrown if the column count is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the data array is null.</exception>
    internal DataColumnCollection(T[,] data, int columnCount, string[]? columnNames = null)
    {
        if (columnCount < 0)
            throw new ArgumentException("Column count must be non-negative.", nameof(columnCount));
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        _data = data;
        _columns = new DataColumn<T>[columnCount];
        _nameToColumn = new Dictionary<string, DataColumn<T>>(columnCount);
        _columnNames = new List<string?>(columnCount);

        // 初始化列和列名
        for (int i = 0; i < columnCount; i++)
        {
            string? name = columnNames != null && i < columnNames.Length ? columnNames[i] : null;
            _columns[i] = new DataColumn<T>(_data, i, name);
            _columnNames.Add(name);
            if (name != null)
                _nameToColumn[name] = _columns[i];
        }
    }

    /// <summary>
    /// Gets or sets a column by its column index.
    /// </summary>
    /// <param name="columnIndex">The column index.</param>
    /// <returns>The corresponding <see cref="DataColumn{T}"/> object.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public DataColumn<T> this[int columnIndex]
    {
        get
        {
            if (columnIndex < 0 || columnIndex >= Count)
                throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range [0, {Count - 1}].");
            return _columns[columnIndex];
        }
        set
        {
            if (columnIndex < 0 || columnIndex >= Count)
                throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range [0, {Count - 1}].");
            _columns[columnIndex] = value;
            // 更新列名映射
            if (_columns[columnIndex].Name != null)
            {
                _nameToColumn[_columns[columnIndex].Name] = _columns[columnIndex];
                _columnNames[columnIndex] = _columns[columnIndex].Name;
            }
            else
            {
                _columnNames[columnIndex] = null;
            }
        }
    }

    /// <summary>
    /// Gets a column by its name.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>The corresponding <see cref="DataColumn{T}"/> object.</returns>
    /// <exception cref="ArgumentException">Thrown if the column name does not exist.</exception>
    public DataColumn<T> this[string name]
    {
        get
        {
            if (!_nameToColumn.TryGetValue(name, out var column))
                throw new ArgumentException($"No column named '{name}' was found.");
            return column;
        }
    }

    /// <summary>
    /// Adds a column to the collection.
    /// </summary>
    /// <param name="columnName">The name of the new column (optional).</param>
    /// <param name="defaultValue">The default value for the new column (optional).</param>
    public void AddColumn(string? columnName = null, T? defaultValue = default)
    {
        int newColumnCount = Count + 1;
        var newData = new T[_data.GetLength(0), newColumnCount];

        // 复制现有数据
        for (int i = 0; i < _data.GetLength(0); i++)
            for (int j = 0; j < Count; j++)
                newData[i, j] = _data[i, j];

        // 填充新列的默认值
        for (int i = 0; i < _data.GetLength(0); i++)
            newData[i, newColumnCount - 1] = defaultValue ?? default(T);

        _data = newData;
        Array.Resize(ref _columns, newColumnCount);
        _columns[newColumnCount - 1] = new DataColumn<T>(_data, newColumnCount - 1, columnName);
        _columnNames.Add(columnName); // 更新列名缓存

        if (columnName != null)
            _nameToColumn[columnName] = _columns[newColumnCount - 1];
    }

    /// <summary>
    /// Removes the column at the specified index from the collection.
    /// </summary>
    /// <param name="columnIndex">The column index to remove.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if attempting to remove the last column.</exception>
    public void RemoveColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= Count)
            throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range [0, {Count - 1}].");
        if (Count <= 1)
            throw new InvalidOperationException("Cannot remove the last column.");

        int newColumnCount = Count - 1;
        var newData = new T[_data.GetLength(0), newColumnCount];

        // 复制数据，跳过要移除的列
        for (int i = 0; i < _data.GetLength(0); i++)
        {
            int newCol = 0;
            for (int j = 0; j < Count; j++)
                if (j != columnIndex)
                    newData[i, newCol++] = _data[i, j];
        }

        string? removedName = _columns[columnIndex].Name;
        var newColumns = new DataColumn<T>[newColumnCount];
        _columnNames.RemoveAt(columnIndex); // 移除对应列名
        int newIndex = 0;
        for (int j = 0; j < Count; j++)
            if (j != columnIndex)
            {
                newColumns[newIndex] = new DataColumn<T>(_data, newIndex, _columns[j].Name);
                newIndex++;
            }
        _columns = newColumns;

        // 重建列名映射
        _nameToColumn.Clear();
        foreach (var col in _columns)
            if (col.Name != null)
                _nameToColumn[col.Name] = col;

        _data = newData;
    }

    /// <summary>
    /// Updates the underlying data array.
    /// </summary>
    /// <param name="newData">The new data array.</param>
    internal void UpdateData(T[,] newData)
    {
        _data = newData;
        for (int i = 0; i < Count; i++)
            _columns[i] = new DataColumn<T>(_data, i, _columns[i].Name);
        // _columnNames 不变，因为列名未修改
    }

    /// <summary>
    /// Returns an enumerator for the column collection.
    /// </summary>
    /// <returns>An enumerator for the columns.</returns>
    public IEnumerator<DataColumn<T>> GetEnumerator()
    {
        return ((IEnumerable<DataColumn<T>>)_columns).GetEnumerator();
    }

    /// <summary>
    /// Returns a non-generic enumerator for the column collection.
    /// </summary>
    /// <returns>A non-generic enumerator for the columns.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns the string representation of the collection.
    /// </summary>
    /// <returns>A string containing the column information.</returns>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"DataColumnCollection ({Count} columns):");
        foreach (var col in _columns)
            sb.AppendLine(col.ToString());
        return sb.ToString();
    }
}

/// <summary>
/// Defines the filter condition types.
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
/// Represents a 2D data table, containing collections of rows and columns.
/// </summary>
/// <typeparam name="T">The value type, must implement the INumber{T} interface.</typeparam>
public class DataTable<T>
    where T : INumber<T>
{
    private readonly T[,] _data;
    private readonly DataRowCollection<T> _rows;
    private readonly DataColumnCollection<T> _columns;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTable{T}"/> class.
    /// </summary>
    /// <param name="rowCount">The number of rows.</param>
    /// <param name="columnCount">The number of columns.</param>
    /// <param name="columnNames">The array of column names.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="rowCount"/> or <paramref name="columnCount"/> is less than 0, or the length of <paramref name="columnNames"/> does not match <paramref name="columnCount"/>.</exception>
    public DataTable(int rowCount, int columnCount, string[]? columnNames = null)
    {
        if (rowCount < 0 || columnCount < 0)
            throw new ArgumentException("Row count and column count must be non-negative.");
        if (columnNames != null && columnNames.Length != columnCount)
            throw new ArgumentException("The number of column names must match the column count.", nameof(columnNames));

        _data = new T[rowCount, columnCount];
        _columns = new DataColumnCollection<T>(_data, columnCount, columnNames);
        _rows = new DataRowCollection<T>(_data, rowCount, _columns); // 传递列集合
    }

    /// <summary>
    /// Gets the row collection.
    /// </summary>
    public DataRowCollection<T> Rows => _rows;

    /// <summary>
    /// Gets the column collection.
    /// </summary>
    public DataColumnCollection<T> Columns => _columns;

    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    public int RowCount => _rows.Count;

    /// <summary>
    /// Gets the number of columns.
    /// </summary>
    public int ColumnCount => _columns.Count;

    /// <summary>
    /// Gets or sets the value at the specified row and column index.
    /// </summary>
    /// <param name="rowIndex">The row index.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <returns>A reference to the value at the specified row and column index.</returns>
    /// <exception cref="IndexOutOfRangeException">If the row index or column index is out of range.</exception>
    public ref T this[int rowIndex, int columnIndex]
    {
        get
        {
            if (rowIndex < 0 || rowIndex >= RowCount)
                throw new IndexOutOfRangeException($"Row index {rowIndex} is out of range [0, {RowCount - 1}].");
            if (columnIndex < 0 || columnIndex >= ColumnCount)
                throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range [0, {ColumnCount - 1}].");
            return ref _data[rowIndex, columnIndex];
        }
    }

    /// <summary>
    /// Gets the underlying data array (internal use only).
    /// </summary>
    /// <returns>The underlying data as a 2D array.</returns>
    internal T[,] GetInternalData() => _data;

    /// <summary>
    /// Returns a string representing the current data table.
    /// </summary>
    /// <returns>A string representing the current data table, including the row count, column count, and details of the rows and columns.</returns>
    public override string ToString()
    {
        return $"DataTable ({RowCount} rows, {ColumnCount} columns):\nRows:\n{_rows}\nColumns:\n{_columns}";
    }
}

