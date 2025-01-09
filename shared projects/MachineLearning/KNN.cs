using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 表示用于分类和回归的K最近邻算法。
/// </summary>
/// <typeparam name="T">坐标的数值类型。</typeparam>
public class KNN<T> :IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly List<(Point<T> Point, string Label)> _data;

    /// <summary>
    /// 初始化 <see cref="KNN{T}"/> 类的新实例。
    /// </summary>
    public KNN()
    {
        _data = new List<(Point<T> Point, string Label)>();
    }

    public MachineLearningTask Task => MachineLearningTask.Classification | MachineLearningTask.Regression;

    /// <summary>
    /// 添加训练数据点。
    /// </summary>
    /// <param name="point">数据点。</param>
    /// <param name="label">数据点的标签。</param>
    public void Add(Point<T> point, string label)
    {
        _data.Add((point, label));
    }

    /// <summary>
    /// 使用K近邻算法对新数据点进行分类。
    /// </summary>
    /// <param name="point">要分类的数据点。</param>
    /// <param name="k">最近邻居的数量。</param>
    /// <returns>预测的标签。</returns>
    public string Classify(Point<T> point, int k)
    {
        var neighbors = _data
            .Select(d => (Distance: Point<T>.Distance(point, d.Point), d.Label))
            .OrderBy(d => d.Distance)
            .Take(k)
            .ToList();

        var grouped = neighbors
            .GroupBy(n => n.Label)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .First();

        return grouped.Key;
    }

    /// <summary>
    /// 使用K近邻算法对新数据点进行回归。
    /// </summary>
    /// <param name="point">要回归的数据点。</param>
    /// <param name="k">最近邻居的数量。</param>
    /// <returns>预测的值。</returns>
    public T Regress(Point<T> point, int k)
    {
        var neighbors = _data
            .Select(d => (Distance: Point<T>.Distance(point, d.Point), d.Point))
            .OrderBy(d => d.Distance)
            .Take(k)
            .ToList();

        T sum = T.Zero;
        foreach (var neighbor in neighbors)
        {
            sum += neighbor.Point.X; // 假设回归目标是X坐标
        }

        return sum / T.CreateChecked(k);
    }
}
