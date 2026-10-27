using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Vector autoregression. <c>series</c> is time by rows and variables by columns.
/// Coefficient matrices map lagged variables (columns) into equations (rows).
/// </summary>
public sealed class VarModel<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly double[] _constant;
    private readonly double[][,] _coefficients;
    private readonly double[,] _tail;

    private VarModel(int order, double[] constant, double[][,] coefficients, double[,] residualCovariance, double[,] tail)
    {
        Order = order;
        _constant = constant;
        _coefficients = coefficients;
        _tail = tail;
        Constant = SeriesConvert.FromDouble<T>(constant);
        Coefficients = new T[order][,];
        for (int lag = 0; lag < order; lag++)
            Coefficients[lag] = ToGeneric(coefficients[lag]);
        ResidualCovariance = ToGeneric(residualCovariance);
    }

    public int Order { get; }
    public T[] Constant { get; }
    public T[][,] Coefficients { get; }
    public T[,] ResidualCovariance { get; }

    public static VarModel<T> Estimate(T[,] series, int order, bool includeConstant = true)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order));

        int time = series.GetLength(0);
        int dimension = series.GetLength(1);
        if (time <= order + dimension || dimension == 0)
            throw new ArgumentException("The panel is too short for this VAR order.");

        var data = new double[time, dimension];
        for (int t = 0; t < time; t++)
            for (int v = 0; v < dimension; v++)
                data[t, v] = double.CreateChecked(series[t, v]);

        int rows = time - order;
        int regressors = order * dimension + (includeConstant ? 1 : 0);
        var design = new double[rows, regressors];
        for (int row = 0; row < rows; row++)
        {
            int t = row + order;
            int column = 0;
            for (int lag = 1; lag <= order; lag++)
            {
                for (int v = 0; v < dimension; v++)
                    design[row, column++] = data[t - lag, v];
            }

            if (includeConstant)
                design[row, column] = 1;
        }

        var coefficients = new double[order][,];
        for (int lag = 0; lag < order; lag++)
            coefficients[lag] = new double[dimension, dimension];
        var constant = new double[dimension];
        var residuals = new double[rows, dimension];

        for (int equation = 0; equation < dimension; equation++)
        {
            var target = new double[rows];
            for (int row = 0; row < rows; row++)
                target[row] = data[row + order, equation];
            double[] solved = DenseLinearAlgebra.LeastSquares(design, target);
            int column = 0;
            for (int lag = 0; lag < order; lag++)
            {
                for (int v = 0; v < dimension; v++)
                    coefficients[lag][equation, v] = solved[column++];
            }

            if (includeConstant)
                constant[equation] = solved[column];

            for (int row = 0; row < rows; row++)
            {
                double prediction = includeConstant ? constant[equation] : 0;
                column = 0;
                for (int lag = 1; lag <= order; lag++)
                {
                    for (int v = 0; v < dimension; v++)
                        prediction += coefficients[lag - 1][equation, v] * data[row + order - lag, v];
                }

                residuals[row, equation] = target[row] - prediction;
            }
        }

        var covariance = new double[dimension, dimension];
        for (int i = 0; i < dimension; i++)
        {
            for (int j = i; j < dimension; j++)
            {
                double sum = 0;
                for (int row = 0; row < rows; row++)
                    sum += residuals[row, i] * residuals[row, j];
                double value = sum / rows;
                covariance[i, j] = value;
                covariance[j, i] = value;
            }
        }

        var tail = new double[order, dimension];
        for (int lag = 0; lag < order; lag++)
        {
            for (int v = 0; v < dimension; v++)
                tail[lag, v] = data[time - order + lag, v];
        }

        return new VarModel<T>(order, constant, coefficients, covariance, tail);
    }

    public T[,] Forecast(int horizon)
    {
        if (horizon < 0)
            throw new ArgumentOutOfRangeException(nameof(horizon));

        int dimension = _constant.Length;
        var history = new double[_tail.GetLength(0) + horizon, dimension];
        for (int t = 0; t < _tail.GetLength(0); t++)
            for (int v = 0; v < dimension; v++)
                history[t, v] = _tail[t, v];

        int origin = _tail.GetLength(0);
        for (int h = 0; h < horizon; h++)
        {
            int t = origin + h;
            for (int equation = 0; equation < dimension; equation++)
            {
                double value = _constant[equation];
                for (int lag = 1; lag <= Order; lag++)
                {
                    for (int v = 0; v < dimension; v++)
                        value += _coefficients[lag - 1][equation, v] * history[t - lag, v];
                }

                history[t, equation] = value;
            }
        }

        var forecast = new T[horizon, dimension];
        for (int h = 0; h < horizon; h++)
            for (int v = 0; v < dimension; v++)
                forecast[h, v] = T.CreateChecked(history[origin + h, v]);
        return forecast;
    }

    static T[,] ToGeneric(double[,] values)
    {
        int rows = values.GetLength(0);
        int cols = values.GetLength(1);
        var result = new T[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = T.CreateChecked(values[i, j]);
        return result;
    }
}
