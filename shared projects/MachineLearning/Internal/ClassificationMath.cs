using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// 分类器共用的投票与 argmax 工具。
/// </summary>
internal static class ClassificationMath
{
    public static int ArgMax<T>(ReadOnlySpan<T> values)
        where T : struct, IFloatingPointIeee754<T>
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > values[best])
                best = i;
        }
        return best;
    }

    public static int MajorityClass(int[] labels, int[] indices)
    {
        var counts = new Dictionary<int, int>();
        foreach (int i in indices)
        {
            counts.TryGetValue(labels[i], out int count);
            counts[labels[i]] = count + 1;
        }
        return ArgMaxCount(counts);
    }

    public static int MajorityVote(IEnumerable<int> predictions)
    {
        var counts = new Dictionary<int, int>();
        foreach (int prediction in predictions)
        {
            counts.TryGetValue(prediction, out int count);
            counts[prediction] = count + 1;
        }
        return ArgMaxCount(counts);
    }

    public static int ArgMaxClassScores<T>(ReadOnlySpan<int> classLabels, ReadOnlySpan<T> scores)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (classLabels.Length == 0 || scores.Length != classLabels.Length)
            throw new ArgumentException("类别与分数长度必须一致且非空。");

        int bestLabel = classLabels[0];
        T bestScore = scores[0];
        for (int i = 1; i < classLabels.Length; i++)
        {
            if (scores[i] > bestScore)
            {
                bestScore = scores[i];
                bestLabel = classLabels[i];
            }
        }
        return bestLabel;
    }

    public static int MajorityVote(ReadOnlySpan<int> predictions)
    {
        if (predictions.Length == 0)
            throw new ArgumentException("至少需要一个预测标签。");

        var counts = new Dictionary<int, int>(predictions.Length);
        foreach (int prediction in predictions)
        {
            counts.TryGetValue(prediction, out int count);
            counts[prediction] = count + 1;
        }
        return ArgMaxCount(counts);
    }

    private static int ArgMaxCount(Dictionary<int, int> counts)
    {
        int bestLabel = 0;
        int bestCount = -1;
        foreach (var (label, count) in counts)
        {
            if (count > bestCount || (count == bestCount && label < bestLabel))
            {
                bestCount = count;
                bestLabel = label;
            }
        }
        return bestLabel;
    }

    public static int WeightedArgMax<T>(ReadOnlySpan<T> classWeights)
        where T : struct, IFloatingPointIeee754<T>
    {
        int best = 0;
        for (int c = 1; c < classWeights.Length; c++)
        {
            if (classWeights[c] > classWeights[best])
                best = c;
        }
        return best;
    }
}
