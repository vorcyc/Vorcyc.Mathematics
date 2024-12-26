using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

/// <summary>
/// t-SNE 算法类，用于降维和数据可视化。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 IFloatingPointIeee754 接口。</typeparam>
public class TSNE<T>
    where T : IFloatingPointIeee754<T>
{
    private int _perplexity;
    private int _maxIter;
    private T _learningRate;

    /// <summary>
    /// 构造 t-SNE 算法实例。
    /// </summary>
    /// <param name="perplexity">困惑度参数。</param>
    /// <param name="maxIter">最大迭代次数。</param>
    /// <param name="learningRate">学习率。</param>
    public TSNE(int perplexity = 30, int maxIter = 1000, T learningRate = default)
    {
        _perplexity = perplexity;
        _maxIter = maxIter;
        _learningRate = learningRate.Equals(default) ? T.CreateChecked(200.0) : learningRate;
    }

    /// <summary>
    /// 执行 t-SNE 算法并返回降维后的矩阵。
    /// </summary>
    /// <param name="data">输入的高维数据矩阵。</param>
    /// <returns>降维后的矩阵。</returns>
    public Matrix<T> FitTransform(Matrix<T> data)
    {
        int n = data.Rows;
        int d = data.Columns;
        int outputDims = 2;

        // 第一步：计算高维空间中的成对相似性
        Matrix<T> P = ComputePairwiseAffinities(data, _perplexity);

        // 第二步：随机初始化低维空间
        Random rand = new Random();
        Matrix<T> Y = new Matrix<T>(n, outputDims);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < outputDims; j++)
            {
                Y[i, j] = T.CreateChecked(rand.NextDouble() * 1e-4);
            }
        }

        // 第三步：梯度下降
        for (int iter = 0; iter < _maxIter; iter++)
        {
            Matrix<T> Q = ComputeLowDimensionalAffinities(Y);
            Matrix<T> grads = ComputeGradients(P, Q, Y);

            // 更新 Y
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < outputDims; j++)
                {
                    Y[i, j] -= _learningRate * grads[i, j];
                }
            }

            if (iter % 100 == 0)
            {
                Console.WriteLine($"Iteration {iter}: Cost = {ComputeCost(P, Q)}");
            }
        }

        return Y;
    }

    /// <summary>
    /// 计算高维空间中的成对相似性。
    /// </summary>
    /// <param name="data">输入的高维数据矩阵。</param>
    /// <param name="perplexity">困惑度参数。</param>
    /// <returns>成对相似性矩阵。</returns>
    private Matrix<T> ComputePairwiseAffinities(Matrix<T> data, int perplexity)
    {
        int n = data.Rows;
        Matrix<T> P = new Matrix<T>(n, n);

        for (int i = 0; i < n; i++)
        {
            T[] distances = new T[n];
            for (int j = 0; j < n; j++)
            {
                distances[j] = EuclideanDistance(data, i, j);
            }

            T[] affinities = ComputeAffinities(distances, perplexity);
            for (int j = 0; j < n; j++)
            {
                P[i, j] = affinities[j];
            }
        }

        return P;
    }

    /// <summary>
    /// 计算成对距离的相似性。
    /// </summary>
    /// <param name="distances">成对距离数组。</param>
    /// <param name="perplexity">困惑度参数。</param>
    /// <returns>相似性数组。</returns>
    private T[] ComputeAffinities(T[] distances, int perplexity)
    {
        int n = distances.Length;
        T[] affinities = new T[n];
        T beta = T.One;
        T logPerplexity = T.CreateChecked(Math.Log(perplexity));

        for (int i = 0; i < 50; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j < n; j++)
            {
                affinities[j] = T.Exp(-distances[j] * beta);
                sum += affinities[j];
            }

            T entropy = T.Zero;
            for (int j = 0; j < n; j++)
            {
                affinities[j] /= sum;
                entropy -= affinities[j] * T.Log(affinities[j]);
            }

            if (T.Abs(entropy - logPerplexity) < T.CreateChecked(1e-5))
            {
                break;
            }

            if (entropy > logPerplexity)
            {
                beta *= T.CreateChecked(1.1);
            }
            else
            {
                beta /= T.CreateChecked(1.1);
            }
        }

        return affinities;
    }

    /// <summary>
    /// 计算两个数据点之间的欧几里得距离。
    /// </summary>
    /// <param name="data">数据矩阵。</param>
    /// <param name="i">第一个数据点的索引。</param>
    /// <param name="j">第二个数据点的索引。</param>
    /// <returns>欧几里得距离。</returns>
    private T EuclideanDistance(Matrix<T> data, int i, int j)
    {
        T sum = T.Zero;
        for (int k = 0; k < data.Columns; k++)
        {
            T diff = data[i, k] - data[j, k];
            sum += diff * diff;
        }
        return T.Sqrt(sum);
    }

    /// <summary>
    /// 计算低维空间中的成对相似性。
    /// </summary>
    /// <param name="Y">低维数据矩阵。</param>
    /// <returns>成对相似性矩阵。</returns>
    private Matrix<T> ComputeLowDimensionalAffinities(Matrix<T> Y)
    {
        int n = Y.Rows;
        Matrix<T> Q = new Matrix<T>(n, n);
        T sum = T.Zero;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                {
                    T dist = EuclideanDistance(Y, i, j);
                    Q[i, j] = T.One / (T.One + dist * dist);
                    sum += Q[i, j];
                }
            }
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Q[i, j] /= sum;
            }
        }

        return Q;
    }

    /// <summary>
    /// 计算梯度。
    /// </summary>
    /// <param name="P">高维空间中的成对相似性矩阵。</param>
    /// <param name="Q">低维空间中的成对相似性矩阵。</param>
    /// <param name="Y">低维数据矩阵。</param>
    /// <returns>梯度矩阵。</returns>
    private Matrix<T> ComputeGradients(Matrix<T> P, Matrix<T> Q, Matrix<T> Y)
    {
        int n = Y.Rows;
        int d = Y.Columns;
        Matrix<T> grads = new Matrix<T>(n, d);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                {
                    T dist = EuclideanDistance(Y, i, j);
                    T coeff = T.CreateChecked(4) * (P[i, j] - Q[i, j]) * Q[i, j] / (T.One + dist * dist);
                    for (int k = 0; k < d; k++)
                    {
                        grads[i, k] += coeff * (Y[i, k] - Y[j, k]);
                    }
                }
            }
        }

        return grads;
    }

    /// <summary>
    /// 计算成本函数值。
    /// </summary>
    /// <param name="P">高维空间中的成对相似性矩阵。</param>
    /// <param name="Q">低维空间中的成对相似性矩阵。</param>
    /// <returns>成本函数值。</returns>
    private T ComputeCost(Matrix<T> P, Matrix<T> Q)
    {
        int n = P.Rows;
        T cost = T.Zero;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (P[i, j] > T.Zero)
                {
                    cost += P[i, j] * T.Log(P[i, j] / Q[i, j]);
                }
            }
        }

        return cost;
    }
}
