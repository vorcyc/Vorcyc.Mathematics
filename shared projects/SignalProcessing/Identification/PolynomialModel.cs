using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Identification;

/// <summary>
/// Polynomial plant <c>A y = B/F u + C/D e</c>. Coefficients use the monic left-side convention:
/// <c>A = [1, a1, …]</c> means <c>y + a1 y(t-1) + …</c>.
/// </summary>
public sealed class PolynomialModel : IIdentifiedModel
{
    private PolynomialModel(string kind, float[] a, float[] b, float[] c, float[] d, float[] f, int delay)
    {
        Kind = kind;
        A = a;
        B = b;
        C = c;
        D = d;
        F = f;
        Delay = delay;
    }

    public string Kind { get; }
    public float[] A { get; }
    public float[] B { get; }
    public float[] C { get; }
    public float[] D { get; }
    public float[] F { get; }
    public int Delay { get; }

    public static PolynomialModel Arx(IdData data, int na, int nb, int nk = 1)
    {
        RequireOrders(na, nb, nk);
        var (a, b, _) = ExtendedLeastSquares(RequireIo(data), na, nb, nc: 0, nk, iterations: 1);
        return new PolynomialModel("arx", a, b, [1f], [1f], [1f], nk);
    }

    public static PolynomialModel Armax(IdData data, int na, int nb, int nc, int nk = 1)
    {
        RequireOrders(na, nb, nk);
        if (nc < 0)
            throw new ArgumentOutOfRangeException(nameof(nc));
        var (a, b, c) = ExtendedLeastSquares(RequireIo(data), na, nb, nc, nk, iterations: 8);
        return new PolynomialModel("armax", a, b, c, [1f], [1f], nk);
    }

    public static PolynomialModel Oe(IdData data, int nb, int nf, int nk = 1)
    {
        if (nb < 1 || nf < 1 || nk < 0)
            throw new ArgumentOutOfRangeException(nameof(nb));
        var io = RequireIo(data);
        var seed = Arx(data, nf, nb, nk);
        float[] b = (float[])seed.B.Clone();
        float[] f = (float[])seed.A.Clone();
        float[] bestB = (float[])b.Clone();
        float[] bestF = (float[])f.Clone();
        double best = double.PositiveInfinity;

        for (int iteration = 0; iteration < 12; iteration++)
        {
            float[] simulated = SimulatePlant(b, f, nk, io.Input, io.Output);
            double sse = SumSquares(io.Output, simulated);
            if (sse < best)
            {
                best = sse;
                bestB = (float[])b.Clone();
                bestF = (float[])f.Clone();
            }

            int rows = io.Output.Length - Math.Max(nf, nk + nb - 1);
            if (rows < nf + nb)
                break;
            int start = io.Output.Length - rows;
            var design = new double[rows, nf + nb];
            var target = new double[rows];
            for (int row = 0; row < rows; row++)
            {
                int t = start + row;
                target[row] = io.Output[t];
                for (int i = 0; i < nf; i++)
                    design[row, i] = -(t - 1 - i >= 0 ? simulated[t - 1 - i] : 0);
                for (int j = 0; j < nb; j++)
                {
                    int index = t - nk - j;
                    design[row, nf + j] = index >= 0 ? io.Input[index] : 0;
                }
            }

            double[] solved = DenseLinearAlgebra.LeastSquares(design, target);
            f = new float[nf + 1];
            f[0] = 1;
            for (int i = 0; i < nf; i++)
                f[i + 1] = (float)solved[i];
            b = new float[nb];
            for (int j = 0; j < nb; j++)
                b[j] = (float)solved[nf + j];
        }

        return new PolynomialModel("oe", [1f], bestB, [1f], [1f], bestF, nk);
    }

    public static PolynomialModel Bj(IdData data, int nb, int nc, int nd, int nf, int nk = 1)
    {
        if (nc < 0 || nd < 0)
            throw new ArgumentOutOfRangeException(nameof(nc));
        var plant = Oe(data, nb, nf, nk);
        float[] simulated = plant.Simulate(data);
        var noise = new float[simulated.Length];
        for (int i = 0; i < noise.Length; i++)
            noise[i] = data.Output[i] - simulated[i];
        var (d, _, c) = ExtendedLeastSquares((noise, Array.Empty<float>()), nd, 0, nc, 1, 6);
        if (nd == 0)
            d = [1f];
        return new PolynomialModel("bj", [1f], plant.B, c, d, plant.F, nk);
    }

    public float[] Simulate(IdData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        float[] input = data.Input ?? new float[data.Samples];
        if (input.Length != data.Output.Length)
            throw new ArgumentException("Input and output must have the same length.");
        float[] denominator = F.Length > 1 ? F : A;
        return SimulatePlant(B, denominator, Delay, input, data.Output);
    }

    static (float[] Output, float[] Input) RequireIo(IdData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Input == null)
            throw new ArgumentException("This polynomial model needs an input.");
        return (data.Output, data.Input);
    }

    static void RequireOrders(int na, int nb, int nk)
    {
        if (na < 0 || nb < 1 || nk < 0)
            throw new ArgumentOutOfRangeException(nameof(nb));
    }

    static (float[] A, float[] B, float[] C) ExtendedLeastSquares((float[] Output, float[] Input) io, int na, int nb, int nc, int nk, int iterations)
    {
        int n = io.Output.Length;
        var errors = new float[n];
        float[] a = Ones(na);
        float[] b = new float[nb];
        float[] c = Ones(nc);
        int columns = na + nb + nc;
        if (columns == 0)
            return (a, b, c);

        int maxLag = Math.Max(na, Math.Max(nb == 0 ? 0 : nk + nb - 1, nc));
        int rows = n - maxLag;
        if (rows <= columns)
            throw new ArgumentException("The record is too short for these polynomial orders.");

        for (int iteration = 0; iteration < Math.Max(iterations, 1); iteration++)
        {
            var design = new double[rows, columns];
            var target = new double[rows];
            for (int row = 0; row < rows; row++)
            {
                int t = row + maxLag;
                target[row] = io.Output[t];
                int column = 0;
                for (int i = 0; i < na; i++)
                    design[row, column++] = -io.Output[t - 1 - i];
                for (int j = 0; j < nb; j++)
                    design[row, column++] = io.Input[t - nk - j];
                for (int k = 0; k < nc; k++)
                    design[row, column++] = t - 1 - k >= 0 ? errors[t - 1 - k] : 0;
            }

            double[] solved = DenseLinearAlgebra.LeastSquares(design, target);
            int cursor = 0;
            a = Ones(na);
            for (int i = 0; i < na; i++)
                a[i + 1] = (float)solved[cursor++];
            b = new float[nb];
            for (int j = 0; j < nb; j++)
                b[j] = (float)solved[cursor++];
            c = Ones(nc);
            for (int k = 0; k < nc; k++)
                c[k + 1] = (float)solved[cursor++];

            for (int t = 0; t < n; t++)
            {
                if (t < maxLag)
                {
                    errors[t] = 0;
                    continue;
                }

                double prediction = 0;
                for (int i = 1; i < a.Length && t - i >= 0; i++)
                    prediction -= a[i] * io.Output[t - i];
                for (int j = 0; j < b.Length; j++)
                {
                    int index = t - nk - j;
                    if (index >= 0 && io.Input.Length > 0)
                        prediction += b[j] * io.Input[index];
                }

                for (int k = 1; k < c.Length && t - k >= 0; k++)
                    prediction += c[k] * errors[t - k];
                errors[t] = io.Output[t] - (float)prediction;
            }
        }

        return (a, b, c);
    }

    public static float[] SimulatePlant(float[] b, float[] denominator, int delay, float[] input, float[]? measured)
    {
        int n = input.Length;
        var output = new float[n];
        int order = Math.Max(0, denominator.Length - 1);
        int warmup = measured == null ? 0 : Math.Min(n, Math.Max(order, delay));
        for (int t = 0; t < warmup; t++)
            output[t] = measured![t];

        for (int t = warmup; t < n; t++)
        {
            double value = 0;
            for (int i = 1; i <= order && t - i >= 0; i++)
                value -= denominator[i] * output[t - i];
            for (int j = 0; j < b.Length; j++)
            {
                int index = t - delay - j;
                if (index >= 0)
                    value += b[j] * input[index];
            }

            output[t] = (float)value;
        }

        return output;
    }

    static float[] Ones(int order)
    {
        var values = new float[order + 1];
        values[0] = 1;
        return values;
    }

    static double SumSquares(float[] left, float[] right)
    {
        double sum = 0;
        for (int i = 0; i < left.Length; i++)
        {
            double error = left[i] - right[i];
            sum += error * error;
        }

        return sum;
    }
}

/// <summary>Model that can simulate an output record from <see cref="IdData"/>.</summary>
public interface IIdentifiedModel
{
    float[] Simulate(IdData data);
}
