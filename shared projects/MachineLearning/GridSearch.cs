using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 网格搜索结果。
/// </summary>
public sealed class GridSearchResult<TOptions>
{
    public required TOptions BestOptions { get; init; }
    public required CrossValidationResult BestScore { get; init; }
}

/// <summary>
/// 基于交叉验证的网格搜索。
/// </summary>
public static class GridSearch
{
    /// <summary>
    /// 在候选参数上搜索最佳分类器配置（以宏平均 F1 为准）。
    /// </summary>
    public static GridSearchResult<TOptions> SearchClassifier<T, TOptions>(
        T[,] x,
        int[] y,
        IReadOnlyList<TOptions> candidates,
        Func<TOptions, Func<T[,], int[], Func<T[], int>>> factory,
        int folds = 5,
        int? seed = null)
        where T : struct
    {
        if (candidates == null || candidates.Count == 0)
            throw new ArgumentException("至少需要一个候选参数。", nameof(candidates));
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        TOptions bestOptions = candidates[0];
        CrossValidationResult? bestScore = null;

        foreach (var options in candidates)
        {
            var score = CrossValidation.Validate(x, y, folds, factory(options), seed);
            if (bestScore == null || score.MeanMacroF1 > bestScore.MeanMacroF1)
            {
                bestScore = score;
                bestOptions = options;
            }
        }

        return new GridSearchResult<TOptions>
        {
            BestOptions = bestOptions,
            BestScore = bestScore!
        };
    }

    /// <summary>
    /// 使用 <see cref="IClassifier{T}"/> 工厂进行网格搜索。
    /// </summary>
    public static GridSearchResult<TOptions> SearchClassifier<T, TOptions>(
        T[,] x,
        int[] y,
        IReadOnlyList<TOptions> candidates,
        Func<TOptions, IClassifier<T>> classifierFactory,
        int folds = 5,
        int? seed = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        return SearchClassifier<T, TOptions>(
            x,
            y,
            candidates,
            options =>
            {
                return (trainX, trainY) =>
                {
                    var classifier = classifierFactory(options);
                    classifier.Fit(trainX, trainY);
                    return classifier.Predict;
                };
            },
            folds,
            seed);
    }
}
