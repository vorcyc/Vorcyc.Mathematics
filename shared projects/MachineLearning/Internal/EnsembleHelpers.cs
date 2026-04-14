namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// 集成学习共用的采样工具。
/// </summary>
internal static class EnsembleHelpers
{
    /// <summary>
    /// 有放回自助法索引，长度等于 <paramref name="count"/>。
    /// </summary>
    public static int[] CreateBootstrapIndices(int count, Random random)
    {
        var indices = new int[count];
        for (int i = 0; i < count; i++)
            indices[i] = random.Next(count);
        return indices;
    }
}
