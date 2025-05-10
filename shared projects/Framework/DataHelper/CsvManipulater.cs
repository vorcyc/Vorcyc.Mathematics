using System.Globalization;
using System.Numerics;
using System.Text;

namespace Vorcyc.Mathematics.Framework.DataHelper;

/// <summary>
/// 提供从 CSV 文件读取数据并返回 <see cref="DataTable{T}"/> 对象的功能，以及将数据保存到 CSV 文件的工具类。
/// </summary>
public static class CsvManipulater
{
    #region Private Helpers

    /// <summary>
    /// 异步逐行读取 CSV 文件的所有行。
    /// </summary>
    /// <param name="filePath">CSV 文件的路径。</param>
    /// <returns>异步枚举的行数据。</returns>
    /// <exception cref="ArgumentException">如果文件路径无效，则抛出此异常。</exception>
    private static async IAsyncEnumerable<string> ReadLinesAsync(string filePath)
    {
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (!string.IsNullOrEmpty(line)) // 跳过空行
                yield return line;
        }
    }

    /// <summary>
    /// 获取 CSV 文件的元数据，包括总列数、起始行和列名。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/>。</typeparam>
    /// <param name="header">CSV 文件的标题行。</param>
    /// <param name="hasHeader">指示文件是否包含标题行。</param>
    /// <param name="delimiter">CSV 文件的分隔符。</param>
    /// <param name="columnIndices">要读取的列索引数组（可选）。</param>
    /// <param name="columnNamesInput">要读取的列名称数组（可选）。</param>
    /// <returns>一个元组，包含总列数、起始行索引和列名数组。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int totalColumns, int startLine, string[]? columnNames) GetCsvMetadata<T>(
        string header, bool hasHeader, char delimiter, int[]? columnIndices = null, string[]? columnNamesInput = null)
        where T : INumber<T>
    {
        if (string.IsNullOrEmpty(header))
            throw new ArgumentException("标题行不能为空", nameof(header));

        var totalColumns = header.AsSpan().Count(delimiter) + 1;
        var startLine = hasHeader ? 1 : 0;
        string[]? columnNames = null;

        if (hasHeader)
        {
            var headerArray = header.Split(delimiter);
            if (headerArray.Length != totalColumns)
                throw new FormatException("标题行中的列数与分隔符计算不一致");
            columnNames = columnIndices is not null
                ? columnIndices.Select(i => i >= 0 && i < headerArray.Length ? headerArray[i] : throw new ArgumentOutOfRangeException(nameof(columnIndices), $"索引 {i} 超出范围")).ToArray()
                : columnNamesInput ?? headerArray;
        }
        else
        {
            columnNames = columnIndices is not null
                ? columnIndices.Select(i => $"Column{i}").ToArray()
                : columnNamesInput ?? Enumerable.Range(0, totalColumns).Select(i => $"Column{i}").ToArray();
        }

        return (totalColumns, startLine, columnNames);
    }

    /// <summary>
    /// 验证列索引或列名称的有效性。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/>。</typeparam>
    /// <param name="totalColumns">CSV 文件的总列数。</param>
    /// <param name="columnIndices">要验证的列索引数组（可选）。</param>
    /// <param name="columnNames">要验证的列名称数组（可选）。</param>
    /// <param name="header">CSV 文件的标题行（可选）。</param>
    /// <exception cref="ArgumentException">如果列索引或列名称无效，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateColumns<T>(int totalColumns, int[]? columnIndices, string[]? columnNames, string[]? header)
        where T : INumber<T>
    {
        if (columnIndices is not null)
        {
            if (columnIndices.Length == 0)
                throw new ArgumentException("必须指定要读取的列索引数组", nameof(columnIndices));
            if (columnIndices.Any(i => i < 0 || i >= totalColumns))
                throw new ArgumentException("列索引数组包含无效索引", nameof(columnIndices));
        }
        else if (columnNames is not null)
        {
            if (columnNames.Length == 0)
                throw new ArgumentException("必须指定要读取的列名称数组", nameof(columnNames));
            if (header is not null && columnNames.Any(name => Array.IndexOf(header, name) == -1))
                throw new ArgumentException("列名称数组中包含无效的列名称", nameof(columnNames));
        }
    }

    /// <summary>
    /// 根据过滤条件过滤 CSV 文件的行，仅解析必要的列。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 和 <see cref="IComparable{T}"/>。</typeparam>
    /// <param name="lines">CSV 文件的行数据异步枚举。</param>
    /// <param name="startLine">数据起始行索引。</param>
    /// <param name="totalColumns">CSV 文件的总列数。</param>
    /// <param name="delimiter">CSV 文件的分隔符。</param>
    /// <param name="filterConditions">基于函数的过滤条件（可选）。</param>
    /// <param name="filterColumns">基于比较的过滤条件（可选）。</param>
    /// <param name="estimatedRowCount">预估的行数，用于预分配内存。</param>
    /// <returns>过滤后的行列表。</returns>
    /// <exception cref="FormatException">如果行数据格式错误或无法解析，则抛出此异常。</exception>
    /// <exception cref="ArgumentException">如果过滤条件无效，则抛出此异常。</exception>
    private static async Task<List<string>> FilterRowsAsync<T>(
        IAsyncEnumerable<string> lines, int startLine, int totalColumns, char delimiter,
        Dictionary<int, Func<T, bool>>? filterConditions = null,
        Dictionary<int, (T value, FilterCondition condition)>? filterColumns = null,
        int estimatedRowCount = 1000)
        where T : INumber<T>, IComparable<T>
    {
        var filteredRows = new List<string>(estimatedRowCount);
        var filterCols = filterConditions?.Keys.ToHashSet() ?? filterColumns?.Keys.ToHashSet();
        int rowIndex = 0;

        await foreach (var line in lines)
        {
            if (rowIndex++ < startLine) continue;

            var lineSpan = line.AsSpan();
            if (lineSpan.Count(delimiter) + 1 != totalColumns)
                throw new FormatException($"第 {rowIndex} 行的列数与预期 ({totalColumns}) 不匹配");

            bool matches = true;
            if (filterConditions is not null || filterColumns is not null)
            {
                int colIndex = 0, start = 0;
                for (int i = 0; i < lineSpan.Length && matches; i++)
                {
                    if (lineSpan[i] == delimiter || i == lineSpan.Length - 1)
                    {
                        int length = (i == lineSpan.Length - 1 && lineSpan[i] != delimiter) ? i - start + 1 : i - start;
                        if (filterCols?.Contains(colIndex) == true)
                        {
                            var valueSpan = lineSpan.Slice(start, length);
                            try
                            {
                                var value = T.Parse(valueSpan, CultureInfo.InvariantCulture);
                                if (filterConditions?.TryGetValue(colIndex, out var condition) == true)
                                {
                                    if (!condition(value)) matches = false;
                                }
                                else if (filterColumns?.TryGetValue(colIndex, out var filter) == true)
                                {
                                    bool conditionMet = filter.condition switch
                                    {
                                        FilterCondition.GreaterThan => value.CompareTo(filter.value) > 0,
                                        FilterCondition.GreaterThanOrEqual => value.CompareTo(filter.value) >= 0,
                                        FilterCondition.Equal => value.CompareTo(filter.value) == 0,
                                        FilterCondition.LessThan => value.CompareTo(filter.value) < 0,
                                        FilterCondition.LessThanOrEqual => value.CompareTo(filter.value) <= 0,
                                        FilterCondition.NotEqual => value.CompareTo(filter.value) != 0,
                                        _ => throw new ArgumentException($"不支持的过滤条件: {filter.condition}", nameof(filterColumns))
                                    };
                                    if (!conditionMet) matches = false;
                                }
                            }
                            catch (FormatException ex)
                            {
                                throw new FormatException($"无法解析第 {rowIndex} 行，第 {colIndex + 1} 列的值 '{valueSpan.ToString()}' 为类型 {typeof(T).Name}", ex);
                            }
                        }
                        start = i + 1;
                        colIndex++;
                    }
                }
            }
            if (matches) filteredRows.Add(line);
        }
        return filteredRows;
    }

    /// <summary>
    /// 将过滤后的行数据填充到 <see cref="DataTable{T}"/> 中，使用 Span 解析。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/>。</typeparam>
    /// <param name="filteredRows">过滤后的行数据。</param>
    /// <param name="columnIndices">要读取的列索引数组。</param>
    /// <param name="delimiter">CSV 文件的分隔符。</param>
    /// <param name="columnNames">列名数组（可选）。</param>
    /// <returns>填充好的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="FormatException">如果数据无法解析为类型 <typeparamref name="T"/>，则抛出此异常。</exception>
    private static DataTable<T> PopulateDataTableSpan<T>(
        List<string> filteredRows, int[] columnIndices, char delimiter, string[]? columnNames)
        where T : INumber<T>
    {
        var dataTable = new DataTable<T>(filteredRows.Count, columnIndices.Length, columnNames);
        var columnSet = columnIndices.ToHashSet();

        for (int rowIndex = 0; rowIndex < filteredRows.Count; rowIndex++)
        {
            var lineSpan = filteredRows[rowIndex].AsSpan();
            int colIndex = 0, start = 0, targetColIndex = 0;
            for (int i = 0; i < lineSpan.Length && targetColIndex < columnIndices.Length; i++)
            {
                if (lineSpan[i] == delimiter || i == lineSpan.Length - 1)
                {
                    int length = (i == lineSpan.Length - 1 && lineSpan[i] != delimiter) ? i - start + 1 : i - start;
                    if (columnSet.Contains(colIndex))
                    {
                        var valueSpan = lineSpan.Slice(start, length);
                        try
                        {
                            dataTable[rowIndex, targetColIndex++] = T.Parse(valueSpan, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            throw new FormatException($"无法解析第 {rowIndex + 1} 行，第 {colIndex + 1} 列的值 '{valueSpan.ToString()}' 为类型 {typeof(T).Name}", ex);
                        }
                    }
                    start = i + 1;
                    colIndex++;
                }
            }
        }
        return dataTable;
    }

    #endregion

    #region Public Methods

    #region Read All Columns

    /// <summary>
    /// 从指定的 CSV 文件中读取所有列数据，并返回一个 <see cref="DataTable{T}"/> 对象。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 接口。</typeparam>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="hasHeader">指示 CSV 文件是否包含标题行，默认为 <c>true</c>。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>包含读取数据的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="ArgumentException">当文件为空或路径无效时抛出。</exception>
    /// <exception cref="FormatException">当无法解析数据为类型 <typeparamref name="T"/> 时抛出。</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(string filePath, bool hasHeader = true, char delimiter = ',')
        where T : INumber<T>
    {
        string? header = null;
        int totalColumns = 0, rowCount = 0;
        var lines = ReadLinesAsync(filePath);

        await foreach (var line in lines)
        {
            if (rowCount++ == 0)
            {
                header = line;
                totalColumns = header.AsSpan().Count(delimiter) + 1;
                break;
            }
        }

        var (totalCols, startLine, columnNames) = GetCsvMetadata<T>(header!, hasHeader, delimiter);
        var columnIndices = Enumerable.Range(0, totalColumns).ToArray();
        var filteredRows = await FilterRowsAsync<T>(lines, startLine, totalCols, delimiter, estimatedRowCount: rowCount - startLine);
        return PopulateDataTableSpan<T>(filteredRows, columnIndices, delimiter, columnNames);
    }

    #endregion

    #region Read by Column Indices

    /// <summary>
    /// 从指定的 CSV 文件中读取指定列索引的数据，并返回一个 <see cref="DataTable{T}"/> 对象。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 接口。</typeparam>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="columnsToRead">要读取的列索引数组。</param>
    /// <param name="hasHeader">指示 CSV 文件是否包含标题行，默认为 <c>true</c>。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>包含读取数据的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="ArgumentException">当文件为空、路径无效或列索引数组无效时抛出。</exception>
    /// <exception cref="FormatException">当无法解析数据为类型 <typeparamref name="T"/> 时抛出。</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(string filePath, int[] columnsToRead, bool hasHeader = true, char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("列索引数组不能为 null", nameof(columnsToRead));

        string? header = null;
        int totalColumns = 0, rowCount = 0;
        var lines = ReadLinesAsync(filePath);

        await foreach (var line in lines)
        {
            if (rowCount++ == 0)
            {
                header = line;
                totalColumns = header.AsSpan().Count(delimiter) + 1;
                break;
            }
        }

        var (totalCols, startLine, columnNames) = GetCsvMetadata<T>(header!, hasHeader, delimiter, columnsToRead);
        ValidateColumns<T>(totalCols, columnsToRead, null, null);
        var filteredRows = await FilterRowsAsync<T>(lines, startLine, totalCols, delimiter, estimatedRowCount: rowCount - startLine);
        return PopulateDataTableSpan<T>(filteredRows, columnsToRead, delimiter, columnNames);
    }

    /// <summary>
    /// 从指定的 CSV 文件中读取指定列索引的数据，并根据条件过滤返回一个 <see cref="DataTable{T}"/> 对象。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 接口。</typeparam>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="columnsToRead">要读取的列索引数组。</param>
    /// <param name="filterConditions">要应用的过滤条件字典，键为列索引，值为过滤函数。</param>
    /// <param name="hasHeader">指示 CSV 文件是否包含标题行，默认为 <c>true</c>。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>包含读取和过滤数据的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="ArgumentException">当文件为空、路径无效、列索引数组无效或过滤条件为空时抛出。</exception>
    /// <exception cref="FormatException">当无法解析数据为类型 <typeparamref name="T"/> 时抛出。</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, int[] columnsToRead, Dictionary<int, Func<T, bool>> filterConditions,
        bool hasHeader = true, char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("列索引数组不能为 null", nameof(columnsToRead));
        if (filterConditions is null || filterConditions.Count == 0)
            throw new ArgumentException("必须指定要过滤的列索引和条件", nameof(filterConditions));

        string? header = null;
        int totalColumns = 0, rowCount = 0;
        var lines = ReadLinesAsync(filePath);

        await foreach (var line in lines)
        {
            if (rowCount++ == 0)
            {
                header = line;
                totalColumns = header.AsSpan().Count(delimiter) + 1;
                break;
            }
        }

        var (totalCols, startLine, columnNames) = GetCsvMetadata<T>(header!, hasHeader, delimiter, columnsToRead);
        ValidateColumns<T>(totalCols, columnsToRead, null, null);
        var filteredRows = await FilterRowsAsync<T>(lines, startLine, totalCols, delimiter, filterConditions, estimatedRowCount: rowCount - startLine);
        return PopulateDataTableSpan<T>(filteredRows, columnsToRead, delimiter, columnNames);
    }

    /// <summary>
    /// 从指定的 CSV 文件中读取指定列索引的数据，并根据比较条件过滤返回一个 <see cref="DataTable{T}"/> 对象。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 和 <see cref="IComparable{T}"/> 接口。</typeparam>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="columnsToRead">要读取的列索引数组。</param>
    /// <param name="filterColumns">要应用的比较过滤条件字典，键为列索引，值为 (比较值, 条件) 元组。</param>
    /// <param name="hasHeader">指示 CSV 文件是否包含标题行，默认为 <c>true</c>。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>包含读取和过滤数据的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="ArgumentException">当文件为空、路径无效、列索引数组无效或过滤条件无效时抛出。</exception>
    /// <exception cref="FormatException">当无法解析数据为类型 <typeparamref name="T"/> 时抛出。</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, int[] columnsToRead, Dictionary<int, (T value, FilterCondition condition)> filterColumns,
        bool hasHeader = true, char delimiter = ',')
        where T : INumber<T>, IComparable<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("列索引数组不能为 null", nameof(columnsToRead));
        if (filterColumns is null || filterColumns.Count == 0)
            throw new ArgumentException("必须指定要过滤的列索引和条件", nameof(filterColumns));

        string? header = null;
        int totalColumns = 0, rowCount = 0;
        var lines = ReadLinesAsync(filePath);

        await foreach (var line in lines)
        {
            if (rowCount++ == 0)
            {
                header = line;
                totalColumns = header.AsSpan().Count(delimiter) + 1;
                break;
            }
        }

        var (totalCols, startLine, columnNames) = GetCsvMetadata<T>(header!, hasHeader, delimiter, columnsToRead);
        ValidateColumns<T>(totalCols, columnsToRead, null, null);
        var filteredRows = await FilterRowsAsync<T>(lines, startLine, totalCols, delimiter, null, filterColumns, rowCount - startLine);
        return PopulateDataTableSpan<T>(filteredRows, columnsToRead, delimiter, columnNames);
    }

    #endregion

    #region Read by Column Names

    /// <summary>
    /// 从指定的 CSV 文件中读取指定列名称的数据，并返回一个 <see cref="DataTable{T}"/> 对象。
    /// 第一行将被视为列名称。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 接口。</typeparam>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="columnsToRead">要读取的列名称数组。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>包含读取数据的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="ArgumentException">当文件为空、路径无效或列名称数组无效时抛出。</exception>
    /// <exception cref="FormatException">当无法解析数据为类型 <typeparamref name="T"/> 时抛出。</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(string filePath, string[] columnsToRead, char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("列名称数组不能为 null", nameof(columnsToRead));

        string? header = null;
        int totalColumns = 0, rowCount = 0;
        var lines = ReadLinesAsync(filePath);

        await foreach (var line in lines)
        {
            if (rowCount++ == 0)
            {
                header = line;
                totalColumns = header.AsSpan().Count(delimiter) + 1;
                break;
            }
        }

        var headerArray = header!.Split(delimiter);
        var columnMap = headerArray.Select((name, index) => (name, index))
                                   .ToDictionary(x => x.name, x => x.index);
        var columnIndices = columnsToRead.Select(name => columnMap[name]).ToArray();
        var (totalCols, startLine, _) = GetCsvMetadata<T>(header, true, delimiter);
        ValidateColumns<T>(totalCols, null, columnsToRead, headerArray);
        var filteredRows = await FilterRowsAsync<T>(lines, startLine, totalCols, delimiter, estimatedRowCount: rowCount - startLine);
        return PopulateDataTableSpan<T>(filteredRows, columnIndices, delimiter, columnsToRead);
    }

    /// <summary>
    /// 从指定的 CSV 文件中读取指定列名称的数据，并根据条件过滤返回一个 <see cref="DataTable{T}"/> 对象。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 接口。</typeparam>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="columnsToRead">要读取的列名称数组。</param>
    /// <param name="filterConditions">要应用的过滤条件字典，键为列名称，值为过滤函数。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>包含读取和过滤数据的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="ArgumentException">当文件为空、路径无效、列名称数组无效或过滤条件为空时抛出。</exception>
    /// <exception cref="FormatException">当无法解析数据为类型 <typeparamref name="T"/> 时抛出。</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, string[] columnsToRead, Dictionary<string, Func<T, bool>> filterConditions,
        char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("列名称数组不能为 null", nameof(columnsToRead));
        if (filterConditions is null || filterConditions.Count == 0)
            throw new ArgumentException("必须指定要过滤的列名称和条件", nameof(filterConditions));

        string? header = null;
        int totalColumns = 0, rowCount = 0;
        var lines = ReadLinesAsync(filePath);

        await foreach (var line in lines)
        {
            if (rowCount++ == 0)
            {
                header = line;
                totalColumns = header.AsSpan().Count(delimiter) + 1;
                break;
            }
        }

        var headerArray = header!.Split(delimiter);
        var columnMap = headerArray.Select((name, index) => (name, index))
                                   .ToDictionary(x => x.name, x => x.index);
        var columnIndices = columnsToRead.Select(name => columnMap[name]).ToArray();
        var filterConditionsByIndex = filterConditions.ToDictionary(kv => columnMap[kv.Key], kv => kv.Value);
        var (totalCols, startLine, _) = GetCsvMetadata<T>(header, true, delimiter);
        ValidateColumns<T>(totalCols, null, columnsToRead, headerArray);
        var filteredRows = await FilterRowsAsync<T>(lines, startLine, totalCols, delimiter, filterConditionsByIndex, estimatedRowCount: rowCount - startLine);
        return PopulateDataTableSpan<T>(filteredRows, columnIndices, delimiter, columnsToRead);
    }

    /// <summary>
    /// 从指定的 CSV 文件中读取指定列名称的数据，并根据比较条件过滤返回一个 <see cref="DataTable{T}"/> 对象。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 和 <see cref="IComparable{T}"/> 接口。</typeparam>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="columnsToRead">要读取的列名称数组。</param>
    /// <param name="filterColumns">要应用的比较过滤条件字典，键为列名称，值为 (比较值, 条件) 元组。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>包含读取和过滤数据的 <see cref="DataTable{T}"/> 对象。</returns>
    /// <exception cref="ArgumentException">当文件为空、路径无效、列名称数组无效或过滤条件无效时抛出。</exception>
    /// <exception cref="FormatException">当无法解析数据为类型 <typeparamref name="T"/> 时抛出。</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, string[] columnsToRead, Dictionary<string, (T value, FilterCondition condition)> filterColumns,
        char delimiter = ',')
        where T : INumber<T>, IComparable<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("列名称数组不能为 null", nameof(columnsToRead));
        if (filterColumns is null || filterColumns.Count == 0)
            throw new ArgumentException("必须指定要过滤的列名称和条件", nameof(filterColumns));

        string? header = null;
        int totalColumns = 0, rowCount = 0;
        var lines = ReadLinesAsync(filePath);

        await foreach (var line in lines)
        {
            if (rowCount++ == 0)
            {
                header = line;
                totalColumns = header.AsSpan().Count(delimiter) + 1;
                break;
            }
        }

        var headerArray = header!.Split(delimiter);
        var columnMap = headerArray.Select((name, index) => (name, index))
                                   .ToDictionary(x => x.name, x => x.index);
        var columnIndices = columnsToRead.Select(name => columnMap[name]).ToArray();
        var filterColumnsByIndex = filterColumns.ToDictionary(kv => columnMap[kv.Key], kv => kv.Value);
        var (totalCols, startLine, _) = GetCsvMetadata<T>(header, true, delimiter);
        ValidateColumns<T>(totalCols, null, columnsToRead, headerArray);
        var filteredRows = await FilterRowsAsync<T>(lines, startLine, totalCols, delimiter, null, filterColumnsByIndex, rowCount - startLine);
        return PopulateDataTableSpan<T>(filteredRows, columnIndices, delimiter, columnsToRead);
    }

    #endregion

    #region Save Methods

    /// <summary>
    /// 同步将 <see cref="DataTable{T}"/> 对象保存到指定的 CSV 文件中。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 接口。</typeparam>
    /// <param name="dataTable">要保存的数据表。</param>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <exception cref="ArgumentNullException">如果 <paramref name="dataTable"/> 或 <paramref name="filePath"/> 为 null，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Save<T>(DataTable<T> dataTable, string filePath, char delimiter = ',')
        where T : INumber<T>
    {
        if (dataTable is null)
            throw new ArgumentNullException(nameof(dataTable));
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));

        using var writer = new StreamWriter(filePath);

        if (dataTable.Columns.Count > 0 && dataTable.Columns[0].Name is not null)
        {
            var columnNames = dataTable.Columns.Select(c => c.Name ?? $"Column{c.ColumnIndex}");
            writer.WriteLine(string.Join(delimiter, columnNames));
        }

        for (int rowIndex = 0; rowIndex < dataTable.RowCount; rowIndex++)
        {
            var values = new string[dataTable.ColumnCount];
            for (int colIndex = 0; colIndex < dataTable.ColumnCount; colIndex++)
            {
                values[colIndex] = dataTable[rowIndex, colIndex].ToString() ?? "";
            }
            writer.WriteLine(string.Join(delimiter, values));
        }
    }

    /// <summary>
    /// 异步将 <see cref="DataTable{T}"/> 对象保存到指定的 CSV 文件中。
    /// </summary>
    /// <typeparam name="T">值的类型，必须实现 <see cref="INumber{T}"/> 接口。</typeparam>
    /// <param name="dataTable">要保存的数据表。</param>
    /// <param name="filePath">CSV 文件路径。</param>
    /// <param name="delimiter">CSV 文件的分隔符，默认为 <c>,</c>。</param>
    /// <returns>表示异步保存操作的任务。</returns>
    /// <exception cref="ArgumentNullException">如果 <paramref name="dataTable"/> 或 <paramref name="filePath"/> 为 null，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task SaveAsync<T>(DataTable<T> dataTable, string filePath, char delimiter = ',')
        where T : INumber<T>
    {
        if (dataTable is null)
            throw new ArgumentNullException(nameof(dataTable));
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));

        await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

        if (dataTable.Columns.Count > 0 && dataTable.Columns[0].Name is not null)
        {
            var columnNames = dataTable.Columns.Select(c => c.Name ?? $"Column{c.ColumnIndex}");
            await writer.WriteLineAsync(string.Join(delimiter, columnNames));
        }

        for (int rowIndex = 0; rowIndex < dataTable.RowCount; rowIndex++)
        {
            var values = new string[dataTable.ColumnCount];
            for (int colIndex = 0; colIndex < dataTable.ColumnCount; colIndex++)
            {
                values[colIndex] = dataTable[rowIndex, colIndex].ToString() ?? "";
            }
            await writer.WriteLineAsync(string.Join(delimiter, values));
        }
    }

    #endregion

    #endregion
}