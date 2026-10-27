using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Identification;

/// <summary>
/// Discrete state-space model <c>x(t+1) = A x(t) + B u(t)</c>, <c>y(t) = C x(t) + D u(t)</c>.
/// </summary>
public sealed class StateSpaceModel : IIdentifiedModel
{
    public StateSpaceModel(float[,] a, float[,] b, float[,] c, float[,] d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }

    public float[,] A { get; }
    public float[,] B { get; }
    public float[,] C { get; }
    public float[,] D { get; }

    public int Order => A.GetLength(0);

    public static StateSpaceModel N4Sid(IdData data, int order)
    {
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order));
        var io = RequireIo(data);
        int i = Math.Max(order + 1, 2 * order);
        int n = io.Output.Length;
        if (n < 4 * i)
            throw new ArgumentException("The record is too short for this subspace order.");

        int columns = n - 2 * i + 1;
        var pastOutput = Hankel(io.Output, 0, i, columns);
        var futureOutput = Hankel(io.Output, i, i, columns);
        var pastInput = Hankel(io.Input, 0, i, columns);
        var futureInput = Hankel(io.Input, i, i, columns);
        var past = Vertical(pastOutput, pastInput);
        var regressors = Vertical(past, futureInput);
        var gram = DenseLinearAlgebra.Multiply(regressors, DenseLinearAlgebra.Transpose(regressors));
        int width = gram.GetLength(0);
        double scale = 0;
        for (int k = 0; k < width; k++)
            scale += gram[k, k];
        double ridge = 1e-8 * (1 + scale / width);
        for (int k = 0; k < width; k++)
            gram[k, k] += ridge;

        var correlation = DenseLinearAlgebra.Multiply(futureOutput, DenseLinearAlgebra.Transpose(regressors));
        var weights = new double[i, width];
        for (int row = 0; row < i; row++)
        {
            var rhs = new double[width];
            for (int col = 0; col < width; col++)
                rhs[col] = correlation[row, col];
            double[] solved = DenseLinearAlgebra.Solve(gram, rhs);
            for (int col = 0; col < width; col++)
                weights[row, col] = solved[col];
        }

        var pastWeights = new double[i, past.GetLength(0)];
        for (int row = 0; row < i; row++)
            for (int col = 0; col < pastWeights.GetLength(1); col++)
                pastWeights[row, col] = weights[row, col];

        var projection = DenseLinearAlgebra.Multiply(pastWeights, past);
        var scatter = DenseLinearAlgebra.Multiply(projection, DenseLinearAlgebra.Transpose(projection));
        var eigenvalues = new double[i];
        var eigenvectors = new double[i, i];
        DenseLinearAlgebra.JacobiEigen(scatter, eigenvalues, eigenvectors);

        var gamma = new double[i, order];
        for (int row = 0; row < i; row++)
            for (int col = 0; col < order; col++)
                gamma[row, col] = eigenvectors[row, col];

        var gammaUp = new double[i - 1, order];
        var gammaDown = new double[i - 1, order];
        for (int row = 0; row < i - 1; row++)
        {
            for (int col = 0; col < order; col++)
            {
                gammaUp[row, col] = gamma[row, col];
                gammaDown[row, col] = gamma[row + 1, col];
            }
        }

        var transition = new double[order, order];
        for (int col = 0; col < order; col++)
        {
            var rhs = new double[i - 1];
            for (int row = 0; row < i - 1; row++)
                rhs[row] = gammaDown[row, col];
            double[] solved = DenseLinearAlgebra.LeastSquares(gammaUp, rhs);
            for (int row = 0; row < order; row++)
                transition[row, col] = solved[row];
        }

        var observation = new double[1, order];
        for (int col = 0; col < order; col++)
            observation[0, col] = gamma[0, col];

        Stabilize(transition);
        var (inputMatrix, direct, _) = EstimateInputMatrices(transition, observation, io.Input, io.Output);
        return new StateSpaceModel(ToSingle(transition), ToSingle(inputMatrix), ToSingle(observation), ToSingle(direct));
    }

    public float[] Simulate(IdData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        float[] input = data.Input ?? new float[data.Samples];
        var transition = ToDouble(A);
        var inputMatrix = ToDouble(B);
        var observation = ToDouble(C);
        var direct = ToDouble(D);
        double[] initial = EstimateInitialState(transition, observation, inputMatrix, direct, input, data.Output);
        return SimulateCore(transition, inputMatrix, observation, direct, input, initial);
    }

    internal static StateSpaceModel FromPlant(float[] numerator, float[] denominator, int delay)
    {
        var padded = new float[delay + numerator.Length];
        numerator.CopyTo(padded, delay);
        int order = denominator.Length - 1;
        if (order < 1)
        {
            return new StateSpaceModel(new float[1, 1], new[,] { { 0f } }, new[,] { { 0f } }, new[,] { { padded[0] } });
        }

        while (padded.Length < order + 1)
        {
            var grown = new float[padded.Length + 1];
            padded.CopyTo(grown, 0);
            padded = grown;
        }

        if (padded.Length > order + 1)
            padded = padded[..(order + 1)];

        float directTerm = padded[0];
        var transition = new float[order, order];
        for (int i = 0; i < order; i++)
            transition[0, i] = -denominator[i + 1];
        for (int i = 1; i < order; i++)
            transition[i, i - 1] = 1;

        var inputMatrix = new float[order, 1];
        inputMatrix[0, 0] = 1;
        var observation = new float[1, order];
        for (int i = 0; i < order; i++)
            observation[0, i] = padded[i + 1] - directTerm * denominator[i + 1];
        return new StateSpaceModel(transition, inputMatrix, observation, new[,] { { directTerm } });
    }

    static (double[,] B, double[,] D, double[] Initial) EstimateInputMatrices(double[,] transition, double[,] observation, float[] input, float[] output)
    {
        int order = transition.GetLength(0);
        int n = output.Length;
        int columns = order + order + 1;
        var design = new double[n, columns];
        var target = new double[n];
        for (int t = 0; t < n; t++)
            target[t] = output[t];

        for (int state = 0; state < order; state++)
        {
            var x = new double[order];
            x[state] = 1;
            for (int t = 0; t < n; t++)
            {
                design[t, state] = DotRow(observation, x);
                x = DenseLinearAlgebra.Multiply(transition, x);
            }
        }

        for (int state = 0; state < order; state++)
        {
            var x = new double[order];
            for (int t = 0; t < n; t++)
            {
                design[t, order + state] = DotRow(observation, x);
                var next = DenseLinearAlgebra.Multiply(transition, x);
                next[state] += input[t];
                x = next;
            }
        }

        for (int t = 0; t < n; t++)
            design[t, columns - 1] = input[t];

        double[] solved = DenseLinearAlgebra.LeastSquares(design, target);
        var inputMatrix = new double[order, 1];
        for (int state = 0; state < order; state++)
            inputMatrix[state, 0] = solved[order + state];
        var direct = new double[1, 1];
        direct[0, 0] = solved[^1];
        var initial = new double[order];
        for (int state = 0; state < order; state++)
            initial[state] = solved[state];
        return (inputMatrix, direct, initial);
    }

    static double[] EstimateInitialState(double[,] transition, double[,] observation, double[,] inputMatrix, double[,] direct, float[] input, float[] output)
    {
        int order = transition.GetLength(0);
        int n = output.Length;
        var forced = new double[n];
        var x = new double[order];
        for (int t = 0; t < n; t++)
        {
            forced[t] = DotRow(observation, x) + direct[0, 0] * input[t];
            var next = DenseLinearAlgebra.Multiply(transition, x);
            for (int state = 0; state < order; state++)
                next[state] += inputMatrix[state, 0] * input[t];
            x = next;
        }

        var design = new double[n, order];
        var target = new double[n];
        for (int state = 0; state < order; state++)
        {
            var free = new double[order];
            free[state] = 1;
            for (int t = 0; t < n; t++)
            {
                design[t, state] = DotRow(observation, free);
                free = DenseLinearAlgebra.Multiply(transition, free);
            }
        }

        for (int t = 0; t < n; t++)
            target[t] = output[t] - forced[t];
        return order == 0 ? [] : DenseLinearAlgebra.LeastSquares(design, target);
    }

    static float[] SimulateCore(double[,] transition, double[,] inputMatrix, double[,] observation, double[,] direct, float[] input, double[] initial)
    {
        int n = input.Length;
        var output = new float[n];
        var x = (double[])initial.Clone();
        for (int t = 0; t < n; t++)
        {
            output[t] = (float)(DotRow(observation, x) + direct[0, 0] * input[t]);
            var next = DenseLinearAlgebra.Multiply(transition, x);
            for (int state = 0; state < x.Length; state++)
                next[state] += inputMatrix[state, 0] * input[t];
            x = next;
        }

        return output;
    }

    static void Stabilize(double[,] transition)
    {
        double radius = SpectralRadius(transition);
        if (radius > 0.98)
        {
            double scale = 0.98 / radius;
            int n = transition.GetLength(0);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    transition[i, j] *= scale;
        }
    }

    static double SpectralRadius(double[,] transition)
    {
        int n = transition.GetLength(0);
        var vector = new double[n];
        vector[0] = 1;
        double radius = 0;
        for (int iteration = 0; iteration < 40; iteration++)
        {
            var next = DenseLinearAlgebra.Multiply(transition, vector);
            radius = 0;
            for (int i = 0; i < n; i++)
                radius += next[i] * next[i];
            radius = Math.Sqrt(radius);
            if (radius < 1e-15)
                return 0;
            for (int i = 0; i < n; i++)
                vector[i] = next[i] / radius;
        }

        return radius;
    }

    static (float[] Output, float[] Input) RequireIo(IdData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Input == null)
            throw new ArgumentException("Subspace identification needs an input.");
        return (data.Output, data.Input);
    }

    static double[,] Hankel(float[] values, int start, int rows, int columns)
    {
        var matrix = new double[rows, columns];
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                matrix[row, column] = values[start + row + column];
        return matrix;
    }

    static double[,] Vertical(double[,] top, double[,] bottom)
    {
        int rows = top.GetLength(0) + bottom.GetLength(0);
        int columns = top.GetLength(1);
        var matrix = new double[rows, columns];
        for (int row = 0; row < top.GetLength(0); row++)
            for (int column = 0; column < columns; column++)
                matrix[row, column] = top[row, column];
        for (int row = 0; row < bottom.GetLength(0); row++)
            for (int column = 0; column < columns; column++)
                matrix[top.GetLength(0) + row, column] = bottom[row, column];
        return matrix;
    }

    static double DotRow(double[,] matrix, double[] vector)
    {
        double sum = 0;
        for (int i = 0; i < vector.Length; i++)
            sum += matrix[0, i] * vector[i];
        return sum;
    }

    static float[,] ToSingle(double[,] values)
    {
        int rows = values.GetLength(0);
        int columns = values.GetLength(1);
        var result = new float[rows, columns];
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                result[row, column] = (float)values[row, column];
        return result;
    }

    static double[,] ToDouble(float[,] values)
    {
        int rows = values.GetLength(0);
        int columns = values.GetLength(1);
        var result = new double[rows, columns];
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                result[row, column] = values[row, column];
        return result;
    }
}
