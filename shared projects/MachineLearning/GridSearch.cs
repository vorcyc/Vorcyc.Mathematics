using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Grid search result.
/// </summary>
public sealed class GridSearchResult<TOptions>
{
    public required TOptions BestOptions { get; init; }
    public required CrossValidationResult BestScore { get; init; }
}

/// <summary>
/// Grid search based on cross-validation.
/// </summary>
public static class GridSearch
{
    /// <summary>
    /// Searches for the best classifier configuration over the candidate parameters (using macro-averaged F1).
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
            throw new ArgumentException("At least one candidate parameter is required.", nameof(candidates));
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
    /// Performs grid search using an <see cref="IClassifier{T}"/> factory.
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
