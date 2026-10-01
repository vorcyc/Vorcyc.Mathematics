using System.Globalization;
using System.Numerics;
using System.Text;

namespace Vorcyc.Mathematics.Framework.DataHelper;

/// <summary>
/// Provides utilities for reading data from CSV files into <see cref="DataTable{T}"/> objects, and for saving data back to CSV files.
/// </summary>
public static class CsvManipulater
{
    #region Private Helpers

    /// <summary>
    /// Asynchronously reads all lines of a CSV file, one at a time.
    /// </summary>
    /// <param name="filePath">The path of the CSV file.</param>
    /// <returns>An asynchronous enumerable of the line data.</returns>
    /// <exception cref="ArgumentException">Thrown if the file path is invalid.</exception>
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
    /// Gets the metadata of a CSV file, including the total column count, start row, and column names.
    /// </summary>
    /// <typeparam name="T">The value type, must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="header">The header row of the CSV file.</param>
    /// <param name="hasHeader">Indicates whether the file contains a header row.</param>
    /// <param name="delimiter">The delimiter of the CSV file.</param>
    /// <param name="columnIndices">The array of column indices to read (optional).</param>
    /// <param name="columnNamesInput">The array of column names to read (optional).</param>
    /// <returns>A tuple containing the total column count, the start row index, and the array of column names.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int totalColumns, int startLine, string[]? columnNames) GetCsvMetadata<T>(
        string header, bool hasHeader, char delimiter, int[]? columnIndices = null, string[]? columnNamesInput = null)
        where T : INumber<T>
    {
        if (string.IsNullOrEmpty(header))
            throw new ArgumentException("Header row cannot be empty.", nameof(header));

        var totalColumns = header.AsSpan().Count(delimiter) + 1;
        var startLine = hasHeader ? 1 : 0;
        string[]? columnNames = null;

        if (hasHeader)
        {
            var headerArray = header.Split(delimiter);
            if (headerArray.Length != totalColumns)
                throw new FormatException("The column count in the header row does not match the delimiter-based count.");
            columnNames = columnIndices is not null
                ? columnIndices.Select(i => i >= 0 && i < headerArray.Length ? headerArray[i] : throw new ArgumentOutOfRangeException(nameof(columnIndices), $"Index {i} is out of range.")).ToArray()
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
    /// Validates the column indices or column names.
    /// </summary>
    /// <typeparam name="T">The value type, must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="totalColumns">The total column count of the CSV file.</param>
    /// <param name="columnIndices">The array of column indices to validate (optional).</param>
    /// <param name="columnNames">The array of column names to validate (optional).</param>
    /// <param name="header">The header row of the CSV file (optional).</param>
    /// <exception cref="ArgumentException">Thrown if the column indices or column names are invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateColumns<T>(int totalColumns, int[]? columnIndices, string[]? columnNames, string[]? header)
        where T : INumber<T>
    {
        if (columnIndices is not null)
        {
            if (columnIndices.Length == 0)
                throw new ArgumentException("An array of column indices to read must be specified.", nameof(columnIndices));
            if (columnIndices.Any(i => i < 0 || i >= totalColumns))
                throw new ArgumentException("The column index array contains an invalid index.", nameof(columnIndices));
        }
        else if (columnNames is not null)
        {
            if (columnNames.Length == 0)
                throw new ArgumentException("An array of column names to read must be specified.", nameof(columnNames));
            if (header is not null && columnNames.Any(name => Array.IndexOf(header, name) == -1))
                throw new ArgumentException("The column name array contains an invalid column name.", nameof(columnNames));
        }
    }

    /// <summary>
    /// Filters the rows of a CSV file based on filter conditions, parsing only the necessary columns.
    /// </summary>
    /// <typeparam name="T">The value type, must implement <see cref="INumber{T}"/> and <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="lines">The asynchronous enumerable of CSV file row data.</param>
    /// <param name="startLine">The start row index of the data.</param>
    /// <param name="totalColumns">The total column count of the CSV file.</param>
    /// <param name="delimiter">The delimiter of the CSV file.</param>
    /// <param name="filterConditions">The function-based filter conditions (optional).</param>
    /// <param name="filterColumns">The comparison-based filter conditions (optional).</param>
    /// <param name="estimatedRowCount">The estimated row count, used for memory pre-allocation.</param>
    /// <returns>The list of filtered rows.</returns>
    /// <exception cref="FormatException">Thrown if the row data is malformed or cannot be parsed.</exception>
    /// <exception cref="ArgumentException">Thrown if the filter conditions are invalid.</exception>
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
                throw new FormatException($"The column count of row {rowIndex} does not match the expected count ({totalColumns}).");

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
                                        _ => throw new ArgumentException($"Unsupported filter condition: {filter.condition}", nameof(filterColumns))
                                    };
                                    if (!conditionMet) matches = false;
                                }
                            }
                            catch (FormatException ex)
                            {
                                throw new FormatException($"Failed to parse the value '{valueSpan.ToString()}' in row {rowIndex}, column {colIndex + 1} as type {typeof(T).Name}.", ex);
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
    /// Populates the filtered row data into a <see cref="DataTable{T}"/>, parsing via spans.
    /// </summary>
    /// <typeparam name="T">The value type, must implement <see cref="INumber{T}"/>.</typeparam>
    /// <param name="filteredRows">The filtered row data.</param>
    /// <param name="columnIndices">The array of column indices to read.</param>
    /// <param name="delimiter">The delimiter of the CSV file.</param>
    /// <param name="columnNames">The array of column names (optional).</param>
    /// <returns>The populated <see cref="DataTable{T}"/> object.</returns>
    /// <exception cref="FormatException">Thrown if the data cannot be parsed as type <typeparamref name="T"/>.</exception>
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
                            throw new FormatException($"Failed to parse the value '{valueSpan.ToString()}' in row {rowIndex + 1}, column {colIndex + 1} as type {typeof(T).Name}.", ex);
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
    /// Reads all column data from the specified CSV file and returns a <see cref="DataTable{T}"/> object.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="hasHeader">Indicates whether the CSV file contains a header row, defaults to <c>true</c>.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A <see cref="DataTable{T}"/> object containing the read data.</returns>
    /// <exception cref="ArgumentException">Thrown when the file is empty or the path is invalid.</exception>
    /// <exception cref="FormatException">Thrown when the data cannot be parsed as type <typeparamref name="T"/>.</exception>
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
    /// Reads the data of specified column indices from the specified CSV file and returns a <see cref="DataTable{T}"/> object.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="columnsToRead">The array of column indices to read.</param>
    /// <param name="hasHeader">Indicates whether the CSV file contains a header row, defaults to <c>true</c>.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A <see cref="DataTable{T}"/> object containing the read data.</returns>
    /// <exception cref="ArgumentException">Thrown when the file is empty, the path is invalid, or the column index array is invalid.</exception>
    /// <exception cref="FormatException">Thrown when the data cannot be parsed as type <typeparamref name="T"/>.</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(string filePath, int[] columnsToRead, bool hasHeader = true, char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("The column index array cannot be null.", nameof(columnsToRead));

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
    /// Reads the data of specified column indices from the specified CSV file, filters it by conditions, and returns a <see cref="DataTable{T}"/> object.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="columnsToRead">The array of column indices to read.</param>
    /// <param name="filterConditions">The dictionary of filter conditions to apply, keyed by column index with the filter function as the value.</param>
    /// <param name="hasHeader">Indicates whether the CSV file contains a header row, defaults to <c>true</c>.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A <see cref="DataTable{T}"/> object containing the read and filtered data.</returns>
    /// <exception cref="ArgumentException">Thrown when the file is empty, the path is invalid, the column index array is invalid, or the filter conditions are empty.</exception>
    /// <exception cref="FormatException">Thrown when the data cannot be parsed as type <typeparamref name="T"/>.</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, int[] columnsToRead, Dictionary<int, Func<T, bool>> filterConditions,
        bool hasHeader = true, char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("The column index array cannot be null.", nameof(columnsToRead));
        if (filterConditions is null || filterConditions.Count == 0)
            throw new ArgumentException("Column indices and conditions to filter on must be specified.", nameof(filterConditions));

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
    /// Reads the data of specified column indices from the specified CSV file, filters it by comparison conditions, and returns a <see cref="DataTable{T}"/> object.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> and <see cref="IComparable{T}"/> interfaces.</typeparam>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="columnsToRead">The array of column indices to read.</param>
    /// <param name="filterColumns">The dictionary of comparison filter conditions to apply, keyed by column index with a (comparison value, condition) tuple as the value.</param>
    /// <param name="hasHeader">Indicates whether the CSV file contains a header row, defaults to <c>true</c>.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A <see cref="DataTable{T}"/> object containing the read and filtered data.</returns>
    /// <exception cref="ArgumentException">Thrown when the file is empty, the path is invalid, the column index array is invalid, or the filter conditions are invalid.</exception>
    /// <exception cref="FormatException">Thrown when the data cannot be parsed as type <typeparamref name="T"/>.</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, int[] columnsToRead, Dictionary<int, (T value, FilterCondition condition)> filterColumns,
        bool hasHeader = true, char delimiter = ',')
        where T : INumber<T>, IComparable<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("The column index array cannot be null.", nameof(columnsToRead));
        if (filterColumns is null || filterColumns.Count == 0)
            throw new ArgumentException("Column indices and conditions to filter on must be specified.", nameof(filterColumns));

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
    /// Reads the data of specified column names from the specified CSV file and returns a <see cref="DataTable{T}"/> object.
    /// The first row is treated as the column names.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="columnsToRead">The array of column names to read.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A <see cref="DataTable{T}"/> object containing the read data.</returns>
    /// <exception cref="ArgumentException">Thrown when the file is empty, the path is invalid, or the column name array is invalid.</exception>
    /// <exception cref="FormatException">Thrown when the data cannot be parsed as type <typeparamref name="T"/>.</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(string filePath, string[] columnsToRead, char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("The column name array cannot be null.", nameof(columnsToRead));

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
    /// Reads the data of specified column names from the specified CSV file, filters it by conditions, and returns a <see cref="DataTable{T}"/> object.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="columnsToRead">The array of column names to read.</param>
    /// <param name="filterConditions">The dictionary of filter conditions to apply, keyed by column name with the filter function as the value.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A <see cref="DataTable{T}"/> object containing the read and filtered data.</returns>
    /// <exception cref="ArgumentException">Thrown when the file is empty, the path is invalid, the column name array is invalid, or the filter conditions are empty.</exception>
    /// <exception cref="FormatException">Thrown when the data cannot be parsed as type <typeparamref name="T"/>.</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, string[] columnsToRead, Dictionary<string, Func<T, bool>> filterConditions,
        char delimiter = ',')
        where T : INumber<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("The column name array cannot be null.", nameof(columnsToRead));
        if (filterConditions is null || filterConditions.Count == 0)
            throw new ArgumentException("Column names and conditions to filter on must be specified.", nameof(filterConditions));

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
    /// Reads the data of specified column names from the specified CSV file, filters it by comparison conditions, and returns a <see cref="DataTable{T}"/> object.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> and <see cref="IComparable{T}"/> interfaces.</typeparam>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="columnsToRead">The array of column names to read.</param>
    /// <param name="filterColumns">The dictionary of comparison filter conditions to apply, keyed by column name with a (comparison value, condition) tuple as the value.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A <see cref="DataTable{T}"/> object containing the read and filtered data.</returns>
    /// <exception cref="ArgumentException">Thrown when the file is empty, the path is invalid, the column name array is invalid, or the filter conditions are invalid.</exception>
    /// <exception cref="FormatException">Thrown when the data cannot be parsed as type <typeparamref name="T"/>.</exception>
    public static async Task<DataTable<T>> ReadAsync<T>(
        string filePath, string[] columnsToRead, Dictionary<string, (T value, FilterCondition condition)> filterColumns,
        char delimiter = ',')
        where T : INumber<T>, IComparable<T>
    {
        if (columnsToRead is null)
            throw new ArgumentException("The column name array cannot be null.", nameof(columnsToRead));
        if (filterColumns is null || filterColumns.Count == 0)
            throw new ArgumentException("Column names and conditions to filter on must be specified.", nameof(filterColumns));

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
    /// Synchronously saves a <see cref="DataTable{T}"/> object to the specified CSV file.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="dataTable">The data table to save.</param>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataTable"/> or <paramref name="filePath"/> is null.</exception>
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
    /// Asynchronously saves a <see cref="DataTable{T}"/> object to the specified CSV file.
    /// </summary>
    /// <typeparam name="T">The value type, must implement the <see cref="INumber{T}"/> interface.</typeparam>
    /// <param name="dataTable">The data table to save.</param>
    /// <param name="filePath">The CSV file path.</param>
    /// <param name="delimiter">The delimiter of the CSV file, defaults to <c>,</c>.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataTable"/> or <paramref name="filePath"/> is null.</exception>
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