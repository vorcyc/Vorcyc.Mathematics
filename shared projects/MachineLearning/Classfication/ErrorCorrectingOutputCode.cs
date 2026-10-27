using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Classfication;

/// <summary>Coding design used to turn binary learners into a multi-class classifier.</summary>
public enum EcocCoding
{
    OneVersusOne,
    OneVersusAll
}

/// <summary>
/// Error-correcting output codes. Each column of the code matrix is one binary <see cref="IClassifier{T}"/>.
/// </summary>
public sealed class ErrorCorrectingOutputCode<T> : IBatchClassifier<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly Func<IClassifier<T>> _factory;
    private readonly EcocCoding _coding;
    private int[] _labels = [];
    private int[,] _codes = new int[0, 0];
    private IClassifier<T>[] _learners = [];
    private bool _isFitted;

    public ErrorCorrectingOutputCode(Func<IClassifier<T>>? binaryLearnerFactory = null, EcocCoding coding = EcocCoding.OneVersusOne, ComputingContext? context = null)
    {
        _factory = binaryLearnerFactory ?? (() => new LogisticRegression<T>(learningRate: T.CreateChecked(0.1), epochs: 800, seed: 1));
        _coding = coding;
        Context = context;
    }

    public ComputingContext? Context { get; set; }

    public MachineLearningTask Task => MachineLearningTask.Classification;

    public int[,] CodeMatrix => (int[,])_codes.Clone();

    public void Fit(T[,] x, int[] y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        int rows = x.GetLength(0);
        if (rows == 0 || rows != y.Length)
            throw new ArgumentException("Training data is empty or the labels do not match.");

        _labels = y.Distinct().OrderBy(label => label).ToArray();
        if (_labels.Length < 2)
            throw new ArgumentException("ECOC needs at least two classes.");

        _codes = BuildCodes(_labels.Length, _coding);
        int learners = _codes.GetLength(1);
        _learners = new IClassifier<T>[learners];
        var labelRow = new Dictionary<int, int>();
        for (int i = 0; i < _labels.Length; i++)
            labelRow[_labels[i]] = i;

        for (int learner = 0; learner < learners; learner++)
        {
            var selected = new List<int>();
            for (int row = 0; row < rows; row++)
            {
                int code = _codes[labelRow[y[row]], learner];
                if (code != 0)
                    selected.Add(row);
            }

            var subsetX = Array2DHelpers.CopyRows(x, selected.ToArray());
            var subsetY = new int[selected.Count];
            for (int i = 0; i < selected.Count; i++)
                subsetY[i] = _codes[labelRow[y[selected[i]]], learner] > 0 ? 1 : 0;

            var model = _factory();
            model.Fit(subsetX, subsetY);
            _learners[learner] = model;
        }

        _isFitted = true;
    }

    public int Predict(T[] sample)
    {
        EnsureFitted();
        int learners = _learners.Length;
        var bits = new int[learners];
        for (int i = 0; i < learners; i++)
            bits[i] = _learners[i].Predict(sample) > 0 ? 1 : -1;

        int bestClass = 0;
        int bestDistance = int.MaxValue;
        for (int classIndex = 0; classIndex < _labels.Length; classIndex++)
        {
            int distance = 0;
            for (int learner = 0; learner < learners; learner++)
            {
                int code = _codes[classIndex, learner];
                if (code != 0 && code != bits[learner])
                    distance++;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestClass = classIndex;
            }
        }

        return _labels[bestClass];
    }

    public void PredictBatch(T[,] x, Span<int> predictions)
    {
        EnsureFitted();
        ArgumentNullException.ThrowIfNull(x);
        int rows = x.GetLength(0);
        if (predictions.Length < rows)
            throw new ArgumentException("The prediction buffer is shorter than the sample count.");
        for (int i = 0; i < rows; i++)
            predictions[i] = Predict(Array2DHelpers.GetRow(x, i));
    }

    static int[,] BuildCodes(int classes, EcocCoding coding)
    {
        if (coding == EcocCoding.OneVersusAll)
        {
            var codes = new int[classes, classes];
            for (int column = 0; column < classes; column++)
            {
                for (int row = 0; row < classes; row++)
                    codes[row, column] = row == column ? 1 : -1;
            }

            return codes;
        }

        int pairs = classes * (classes - 1) / 2;
        var pairwise = new int[classes, pairs];
        int columnIndex = 0;
        for (int i = 0; i < classes; i++)
        {
            for (int j = i + 1; j < classes; j++)
            {
                pairwise[i, columnIndex] = -1;
                pairwise[j, columnIndex] = 1;
                columnIndex++;
            }
        }

        return pairwise;
    }

    void EnsureFitted()
    {
        if (!_isFitted)
            throw new InvalidOperationException("The model has not been fitted yet.");
    }
}
