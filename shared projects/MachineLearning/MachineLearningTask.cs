namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 表示机器学习任务的枚举。
/// </summary>
[Flags]
public enum MachineLearningTask
{
    /// <summary>
    /// 无任务。
    /// </summary>
    None = 0,

    /// <summary>
    /// 分类任务。
    /// </summary>
    Classification = 1 << 0, // 1

    /// <summary>
    /// 聚类任务。
    /// </summary>
    Clustering = 1 << 1,     // 2

    /// <summary>
    /// 回归任务。
    /// </summary>
    Regression = 1 << 2,     // 4

    /// <summary>
    /// 异常检测任务。
    /// </summary>
    AnomalyDetection = 1 << 3, // 8

    /// <summary>
    /// 降维任务。
    /// </summary>
    DimensionalityReduction = 1 << 4 // 16
}
