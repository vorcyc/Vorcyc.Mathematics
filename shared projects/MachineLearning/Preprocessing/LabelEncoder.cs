namespace Vorcyc.Mathematics.MachineLearning.Preprocessing;

/// <summary>
/// Encodes string labels as integers in the range 0..K-1.
/// </summary>
public sealed class LabelEncoder : IMachineLearning
{
    private Dictionary<string, int> _mapping = [];
    private List<string> _labels = [];

    /// <inheritdoc />
    public MachineLearningTask Task => MachineLearningTask.Classification;

    /// <summary>The known class labels.</summary>
    public IReadOnlyList<string> Classes => _labels;

    /// <summary>
    /// Fits the label mapping.
    /// </summary>
    public void Fit(IEnumerable<string> labels)
    {
        _mapping.Clear();
        _labels = labels.Distinct().Order().ToList();
        for (int i = 0; i < _labels.Count; i++)
            _mapping[_labels[i]] = i;
    }

    /// <summary>
    /// Encodes a single label.
    /// </summary>
    public int Transform(string label) =>
        _mapping.TryGetValue(label, out int value)
            ? value
            : throw new ArgumentException($"Unknown label: {label}", nameof(label));

    /// <summary>
    /// Batch encoding.
    /// </summary>
    public int[] Transform(IReadOnlyList<string> labels) =>
        labels.Select(Transform).ToArray();

    /// <summary>
    /// Decodes an integer label.
    /// </summary>
    public string InverseTransform(int label)
    {
        if (label < 0 || label >= _labels.Count)
            throw new ArgumentOutOfRangeException(nameof(label));
        return _labels[label];
    }
}
