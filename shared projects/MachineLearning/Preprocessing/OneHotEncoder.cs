namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// 对字符串类别特征做 One-Hot 编码。
/// </summary>
public sealed class OneHotEncoder : IMachineLearning
{
    private List<string>[] _categories = [];

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.None;

    /// <summary>每列的类别列表。</summary>
    public IReadOnlyList<IReadOnlyList<string>> Categories =>
        _categories.Select(c => (IReadOnlyList<string>)c).ToArray();

    /// <summary>
    /// 拟合编码器。输入为 [样本数, 特征数] 的字符串矩阵。
    /// </summary>
    public void Fit(string[,] x)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0)
            throw new ArgumentException("输入不能为空。");

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
    /// 变换为 double 矩阵。
    /// </summary>
    public double[,] Transform(string[,] x)
    {
        if (_categories.Length == 0)
            throw new InvalidOperationException("编码器尚未拟合。");
        if (x.GetLength(1) != _categories.Length)
            throw new ArgumentException("特征列数不匹配。");

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
                    throw new ArgumentException($"未知类别值: {x[i, j]}");
                result[i, col + index] = 1.0;
                col += _categories[j].Count;
            }
        }
        return result;
    }

    /// <summary>
    /// 变换单行样本。
    /// </summary>
    public double[] Transform(string[] sample)
    {
        if (sample.Length != _categories.Length)
            throw new ArgumentException("特征维度不匹配。");

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
