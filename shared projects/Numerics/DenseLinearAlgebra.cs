namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// Small dense linear-algebra kernels shared by estimation code in this assembly.
/// </summary>
internal static class DenseLinearAlgebra
{
    public static double[] Solve(double[,] matrix, double[] rightHandSide)
    {
        int n = rightHandSide.Length;
        if (matrix.GetLength(0) != n || matrix.GetLength(1) != n)
            throw new ArgumentException("Matrix dimension does not match the right-hand side.");

        var a = (double[,])matrix.Clone();
        var b = (double[])rightHandSide.Clone();

        for (int k = 0; k < n; k++)
        {
            int pivot = k;
            double max = Math.Abs(a[k, k]);
            for (int i = k + 1; i < n; i++)
            {
                double value = Math.Abs(a[i, k]);
                if (value > max)
                {
                    max = value;
                    pivot = i;
                }
            }

            if (max < 1e-14)
                throw new InvalidOperationException("The linear system is singular.");

            if (pivot != k)
            {
                for (int j = k; j < n; j++)
                    (a[k, j], a[pivot, j]) = (a[pivot, j], a[k, j]);
                (b[k], b[pivot]) = (b[pivot], b[k]);
            }

            double diagonal = a[k, k];
            for (int i = k + 1; i < n; i++)
            {
                double factor = a[i, k] / diagonal;
                a[i, k] = 0;
                for (int j = k + 1; j < n; j++)
                    a[i, j] -= factor * a[k, j];
                b[i] -= factor * b[k];
            }
        }

        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = b[i];
            for (int j = i + 1; j < n; j++)
                sum -= a[i, j] * x[j];
            x[i] = sum / a[i, i];
        }

        return x;
    }

    public static double[] LeastSquares(double[,] design, double[] observations)
    {
        int rows = design.GetLength(0);
        int cols = design.GetLength(1);
        if (observations.Length != rows)
            throw new ArgumentException("Observation length does not match the design matrix.");
        if (rows == 0 || cols == 0)
            throw new ArgumentException("The least-squares problem is empty.");

        var normal = new double[cols, cols];
        var rhs = new double[cols];
        for (int j = 0; j < cols; j++)
        {
            for (int i = 0; i < rows; i++)
                rhs[j] += design[i, j] * observations[i];

            for (int k = j; k < cols; k++)
            {
                double sum = 0;
                for (int i = 0; i < rows; i++)
                    sum += design[i, j] * design[i, k];
                normal[j, k] = sum;
                normal[k, j] = sum;
            }
        }

        for (int j = 0; j < cols; j++)
            normal[j, j] += 1e-10 * (1.0 + Math.Abs(normal[j, j]));

        return Solve(normal, rhs);
    }

    public static double[,] Multiply(double[,] left, double[,] right)
    {
        int rows = left.GetLength(0);
        int inner = left.GetLength(1);
        int cols = right.GetLength(1);
        if (right.GetLength(0) != inner)
            throw new ArgumentException("Matrix dimensions do not agree.");

        var result = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int k = 0; k < inner; k++)
            {
                double lik = left[i, k];
                if (lik == 0)
                    continue;
                for (int j = 0; j < cols; j++)
                    result[i, j] += lik * right[k, j];
            }
        }

        return result;
    }

    public static double[] Multiply(double[,] matrix, double[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        if (vector.Length != cols)
            throw new ArgumentException("Vector length does not match the matrix.");

        var result = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double sum = 0;
            for (int j = 0; j < cols; j++)
                sum += matrix[i, j] * vector[j];
            result[i] = sum;
        }

        return result;
    }

    public static double Dot(double[] left, double[] right)
    {
        double sum = 0;
        for (int i = 0; i < left.Length; i++)
            sum += left[i] * right[i];
        return sum;
    }

    public static double[,] Transpose(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new double[cols, rows];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[j, i] = matrix[i, j];
        return result;
    }

    /// <summary>
    /// Symmetric eigendecomposition. Eigenvalues are returned in descending order.
    /// </summary>
    public static void JacobiEigen(double[,] symmetric, double[] eigenvalues, double[,] eigenvectors)
    {
        int n = eigenvalues.Length;
        if (symmetric.GetLength(0) != n || symmetric.GetLength(1) != n || eigenvectors.GetLength(0) != n || eigenvectors.GetLength(1) != n)
            throw new ArgumentException("Eigen-decomposition dimensions do not agree.");

        var a = (double[,])symmetric.Clone();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                eigenvectors[i, j] = i == j ? 1.0 : 0.0;
        }

        for (int sweep = 0; sweep < 60; sweep++)
        {
            double off = 0;
            for (int p = 0; p < n; p++)
                for (int q = p + 1; q < n; q++)
                    off += a[p, q] * a[p, q];
            if (off < 1e-24)
                break;

            for (int p = 0; p < n; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    double apq = a[p, q];
                    if (Math.Abs(apq) <= 1e-15 * (Math.Abs(a[p, p]) + Math.Abs(a[q, q]) + 1.0))
                        continue;

                    double tau = (a[q, q] - a[p, p]) / (2.0 * apq);
                    double t = tau == 0
                        ? 1.0
                        : Math.CopySign(1.0, tau) / (Math.Abs(tau) + Math.Sqrt(1.0 + tau * tau));
                    double c = 1.0 / Math.Sqrt(1.0 + t * t);
                    double s = t * c;

                    double app = a[p, p];
                    double aqq = a[q, q];
                    a[p, p] = app - t * apq;
                    a[q, q] = aqq + t * apq;
                    a[p, q] = 0;
                    a[q, p] = 0;

                    for (int k = 0; k < n; k++)
                    {
                        if (k == p || k == q)
                            continue;
                        double aik = a[k, p];
                        double akq = a[k, q];
                        double newKp = c * aik - s * akq;
                        double newKq = s * aik + c * akq;
                        a[k, p] = newKp;
                        a[p, k] = newKp;
                        a[k, q] = newKq;
                        a[q, k] = newKq;
                    }

                    for (int k = 0; k < n; k++)
                    {
                        double vip = eigenvectors[k, p];
                        double viq = eigenvectors[k, q];
                        eigenvectors[k, p] = c * vip - s * viq;
                        eigenvectors[k, q] = s * vip + c * viq;
                    }
                }
            }
        }

        var order = new int[n];
        for (int i = 0; i < n; i++)
        {
            eigenvalues[i] = a[i, i];
            order[i] = i;
        }

        Array.Sort(eigenvalues, order, Comparer<double>.Create((x, y) => y.CompareTo(x)));
        var sorted = new double[n, n];
        for (int col = 0; col < n; col++)
        {
            int source = order[col];
            for (int row = 0; row < n; row++)
                sorted[row, col] = eigenvectors[row, source];
        }

        for (int col = 0; col < n; col++)
            for (int row = 0; row < n; row++)
                eigenvectors[row, col] = sorted[row, col];
    }
}

/// <summary>Derivative-free Nelder–Mead minimization for small parameter vectors.</summary>
internal static class NelderMead
{
    public static double[] Minimize(Func<double[], double> objective, double[] start, double[] step, int maxIterations = 250)
    {
        int n = start.Length;
        var simplex = new double[n + 1][];
        var values = new double[n + 1];
        simplex[0] = (double[])start.Clone();
        values[0] = objective(simplex[0]);
        for (int i = 0; i < n; i++)
        {
            simplex[i + 1] = (double[])start.Clone();
            simplex[i + 1][i] += step[i] == 0 ? 0.05 : step[i];
            values[i + 1] = objective(simplex[i + 1]);
        }

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            Sort(simplex, values);
            double range = values[n] - values[0];
            if (range < 1e-12)
                break;

            var centroid = new double[n];
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++)
                    sum += simplex[j][i];
                centroid[i] = sum / n;
            }

            var reflected = Move(centroid, simplex[n], 1.0);
            double reflectedValue = objective(reflected);
            if (reflectedValue < values[0])
            {
                var expanded = Move(centroid, simplex[n], 2.0);
                double expandedValue = objective(expanded);
                if (expandedValue < reflectedValue)
                    Replace(simplex, values, n, expanded, expandedValue);
                else
                    Replace(simplex, values, n, reflected, reflectedValue);
            }
            else if (reflectedValue < values[n - 1])
            {
                Replace(simplex, values, n, reflected, reflectedValue);
            }
            else
            {
                var contracted = Move(centroid, simplex[n], reflectedValue < values[n] ? 0.5 : -0.5);
                double contractedValue = objective(contracted);
                if (contractedValue < Math.Min(reflectedValue, values[n]))
                {
                    Replace(simplex, values, n, contracted, contractedValue);
                }
                else
                {
                    for (int j = 1; j <= n; j++)
                    {
                        for (int i = 0; i < n; i++)
                            simplex[j][i] = 0.5 * (simplex[j][i] + simplex[0][i]);
                        values[j] = objective(simplex[j]);
                    }
                }
            }
        }

        Sort(simplex, values);
        return simplex[0];
    }

    static double[] Move(double[] centroid, double[] worst, double factor)
    {
        var point = new double[centroid.Length];
        for (int i = 0; i < centroid.Length; i++)
            point[i] = centroid[i] + factor * (centroid[i] - worst[i]);
        return point;
    }

    static void Replace(double[][] simplex, double[] values, int index, double[] point, double value)
    {
        simplex[index] = point;
        values[index] = value;
    }

    static void Sort(double[][] simplex, double[] values)
    {
        int n = values.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int best = i;
            for (int j = i + 1; j < n; j++)
            {
                if (values[j] < values[best])
                    best = j;
            }

            if (best != i)
            {
                (values[i], values[best]) = (values[best], values[i]);
                (simplex[i], simplex[best]) = (simplex[best], simplex[i]);
            }
        }
    }
}
