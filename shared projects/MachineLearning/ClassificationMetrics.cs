namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 混淆矩阵。
/// </summary>
public sealed class ConfusionMatrix
{
    /// <summary>
    /// 初始化混淆矩阵。
    /// </summary>
    /// <param name="matrix">行=真实类别，列=预测类别。</param>
    /// <param name="classLabels">类别标签。</param>
    public ConfusionMatrix(int[,] matrix, IReadOnlyList<int> classLabels)
    {
        Matrix = matrix;
        ClassLabels = classLabels;
    }

    /// <summary>矩阵元素 [actual, predicted]。</summary>
    public int[,] Matrix { get; }

    /// <summary>类别标签顺序。</summary>
    public IReadOnlyList<int> ClassLabels { get; }

    /// <summary>类别数。</summary>
    public int NumClasses => ClassLabels.Count;
}

/// <summary>
/// 分类评估指标。
/// </summary>
public static class ClassificationMetrics
{
    /// <summary>
    /// 构建混淆矩阵。
    /// </summary>
    public static ConfusionMatrix ConfusionMatrix(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("标签长度必须相同。");
        if (actual.Length == 0)
            throw new ArgumentException("输入不能为空。");

        var labels = actual.ToArray().Concat(predicted.ToArray()).Distinct().Order().ToArray();
        int k = labels.Length;
        var matrix = new int[k, k];
        var labelToIndex = labels.Select((label, index) => (label, index)).ToDictionary(t => t.label, t => t.index);

        for (int i = 0; i < actual.Length; i++)
        {
            int a = labelToIndex[actual[i]];
            int p = labelToIndex[predicted[i]];
            matrix[a, p]++;
        }

        return new ConfusionMatrix(matrix, labels);
    }

    /// <summary>
    /// 计算单类精确率。
    /// </summary>
    public static double Precision(ConfusionMatrix cm, int classLabel)
    {
        int index = cm.ClassLabels.ToList().IndexOf(classLabel);
        if (index < 0)
            throw new ArgumentException("类别不存在。", nameof(classLabel));

        int tp = cm.Matrix[index, index];
        int predictedPositive = 0;
        for (int i = 0; i < cm.NumClasses; i++)
            predictedPositive += cm.Matrix[i, index];
        return predictedPositive == 0 ? 0.0 : (double)tp / predictedPositive;
    }

    /// <summary>
    /// 计算单类召回率。
    /// </summary>
    public static double Recall(ConfusionMatrix cm, int classLabel)
    {
        int index = cm.ClassLabels.ToList().IndexOf(classLabel);
        if (index < 0)
            throw new ArgumentException("类别不存在。", nameof(classLabel));

        int tp = cm.Matrix[index, index];
        int actualPositive = 0;
        for (int j = 0; j < cm.NumClasses; j++)
            actualPositive += cm.Matrix[index, j];
        return actualPositive == 0 ? 0.0 : (double)tp / actualPositive;
    }

    /// <summary>
    /// 计算单类 F1。
    /// </summary>
    public static double F1Score(ConfusionMatrix cm, int classLabel)
    {
        double precision = Precision(cm, classLabel);
        double recall = Recall(cm, classLabel);
        return precision + recall == 0 ? 0.0 : 2.0 * precision * recall / (precision + recall);
    }

    /// <summary>
    /// 宏平均 F1。
    /// </summary>
    public static double MacroF1(ConfusionMatrix cm)
    {
        if (cm.NumClasses == 0)
            return 0.0;
        double sum = 0.0;
        foreach (int label in cm.ClassLabels)
            sum += F1Score(cm, label);
        return sum / cm.NumClasses;
    }

    /// <summary>
    /// 微平均 F1（多分类单标签下等价于准确率）。
    /// </summary>
    public static double MicroF1(ConfusionMatrix cm)
    {
        int tp = 0;
        int total = 0;
        for (int i = 0; i < cm.NumClasses; i++)
        {
            tp += cm.Matrix[i, i];
            for (int j = 0; j < cm.NumClasses; j++)
                total += cm.Matrix[i, j];
        }
        return total == 0 ? 0.0 : (double)tp / total;
    }

    /// <summary>
    /// 从标签直接计算宏平均 F1。
    /// </summary>
    public static double MacroF1(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted) =>
        MacroF1(ConfusionMatrix(actual, predicted));

    /// <summary>
    /// 从标签直接计算微平均 F1。
    /// </summary>
    public static double MicroF1(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted) =>
        MicroF1(ConfusionMatrix(actual, predicted));
}
