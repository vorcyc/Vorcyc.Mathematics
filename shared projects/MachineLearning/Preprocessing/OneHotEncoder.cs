namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// Applies One-Hot encoding to string categorical features.
/// </summary>
public sealed class OneHotEncoder : IMachineLearning
{
    private List<string>[] _categories = [];

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.None;

    /// <summary>The category list for each column.</summary>
    public IReadOnlyList<IReadOnlyList<string>> Categories =>
        _categories.Select(c => (IReadOnlyList<string>)c).ToArray();

    /// <summary>
    /// Fits the encoder. The input is a [samples, features] string matrix.
    /// </summary>
    public void Fit(string[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0)
            throw new ArgumentException("Input cannot be empty.");

        _categories = new List<string>[cols];
        for (int j = 0; j < cols; j++)
        {
            var set = new HashSet<string>();
            for (int i = 0; i < rows; i++)
                set.Add(x[i, j]);
            _categories[j] = set.Order().ToList();
        }
    }

    /// <summary>
    /// Transforms into a double matrix.
    /// </summary>
    public double[,] Transform(string[,] x)
    {
        if (_categories.Length == 0)
            throw new InvalidOperationException("The encoder has not been fitted yet.");
        if (x.GetLength(1) != _categories.Length)
            throw new ArgumentException("The number of feature columns does not match.");

        int rows = x.GetLength(0);
        int outCols = _categories.Sum(c => c.Count);
        var result = new double[rows, outCols];

        for (int i = 0; i < rows; i++)
        {
            int col = 0;
            for (int j = 0; j < _categories.Length; j++)
            {
                int index = _categories[j].IndexOf(x[i, j]);
                if (index < 0)
                    throw new ArgumentException($"Unknown category value: {x[i, j]}");
                result[i, col + index] = 1.0;
                col += _categories[j].Count;
            }
        }
        return result;
    }

    /// <summary>
    /// Transforms a single-row sample.
    /// </summary>
    public double[] Transform(string[] sample)
    {
        if (sample.Length != _categories.Length)
            throw new ArgumentException("The feature dimensionality does not match.");

        var row = new string[1, sample.Length];
        for (int j = 0; j < sample.Length; j++)
            row[0, j] = sample[j];
        var matrix = Transform(row);
        var result = new double[matrix.GetLength(1)];
        for (int j = 0; j < result.Length; j++)
            result[j] = matrix[0, j];
        return result;
    }
}
