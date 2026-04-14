namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// 将字符串标签编码为 0..K-1 整数。
/// </summary>
public sealed class LabelEncoder : IMachineLearning
{
    private Dictionary<string, int> _mapping = [];
    private List<string> _labels = [];

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>已知类别标签。</summary>
    public IReadOnlyList<string> Classes => _labels;

    /// <summary>
    /// 拟合标签映射。
    /// </summary>
    public void Fit(IEnumerable<string> labels)
    {
        _mapping.Clear();
        _labels = labels.Distinct().Order().ToList();
        for (int i = 0; i < _labels.Count; i++)
            _mapping[_labels[i]] = i;
    }

    /// <summary>
    /// 编码单个标签。
    /// </summary>
    public int Transform(string label) =>
        _mapping.TryGetValue(label, out int value)
            ? value
            : throw new ArgumentException($"未知标签: {label}", nameof(label));

    /// <summary>
    /// 批量编码。
    /// </summary>
    public int[] Transform(IReadOnlyList<string> labels) =>
        labels.Select(Transform).ToArray();

    /// <summary>
    /// 解码整数标签。
    /// </summary>
    public string InverseTransform(int label)
    {
        if (label < 0 || label >= _labels.Count)
            throw new ArgumentOutOfRangeException(nameof(label));
        return _labels[label];
    }
}
