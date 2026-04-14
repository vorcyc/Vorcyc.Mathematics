using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>微积分模块内联向量运算（double/float 使用 SIMD）。</summary>
internal static class CalculusVectorOps
{
    public static T Dot<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b) where T : struct, IFloatingPointIeee754<T>
    {
        if (a.Length != b.Length)
            throw new ArgumentException("向量长度不匹配");

        if (typeof(T) == typeof(double))
            return (T)(object)DotDouble(MemoryMarshal.Cast<T, double>(a), MemoryMarshal.Cast<T, double>(b));

        if (typeof(T) == typeof(float))
            return (T)(object)DotFloat(MemoryMarshal.Cast<T, float>(a), MemoryMarshal.Cast<T, float>(b));

        T sum = T.Zero;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    public static void MatVec<T>(ReadOnlySpan<T> m, int n, ReadOnlySpan<T> v, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (v.Length < n || result.Length < n || m.Length < n * n)
            throw new ArgumentException("矩阵/向量维数不匹配");

        if (typeof(T) == typeof(double))
        {
            MatVecDouble(MemoryMarshal.Cast<T, double>(m), n, MemoryMarshal.Cast<T, double>(v), MemoryMarshal.Cast<T, double>(result));
            return;
        }

        if (typeof(T) == typeof(float))
        {
            MatVecFloat(MemoryMarshal.Cast<T, float>(m), n, MemoryMarshal.Cast<T, float>(v), MemoryMarshal.Cast<T, float>(result));
            return;
        }

        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            int row = i * n;
            for (int j = 0; j < n; j++)
                sum += m[row + j] * v[j];
            result[i] = sum;
        }
    }

    public static T Norm<T>(ReadOnlySpan<T> v) where T : struct, IFloatingPointIeee754<T> =>
        T.Sqrt(Dot(v, v));

    /// <summary>dest[i] = base[i] + scale * delta[i]。</summary>
    public static void AssignPlusScaled<T>(Span<T> dest, ReadOnlySpan<T> base_, ReadOnlySpan<T> delta, T scale)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (dest.Length != base_.Length || dest.Length != delta.Length)
            throw new ArgumentException("向量长度不匹配");

        if (typeof(T) == typeof(double))
        {
            AssignPlusScaledDouble(
                MemoryMarshal.Cast<T, double>(dest),
                MemoryMarshal.Cast<T, double>(base_),
                MemoryMarshal.Cast<T, double>(delta),
                (double)(object)scale!);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            AssignPlusScaledFloat(
                MemoryMarshal.Cast<T, float>(dest),
                MemoryMarshal.Cast<T, float>(base_),
                MemoryMarshal.Cast<T, float>(delta),
                (float)(object)scale!);
            return;
        }

        for (int i = 0; i < dest.Length; i++)
            dest[i] = base_[i] + scale * delta[i];
    }

    /// <summary>y += alpha * x。</summary>
    public static void AddScaled<T>(Span<T> y, ReadOnlySpan<T> x, T alpha) where T : struct, IFloatingPointIeee754<T>
    {
        if (y.Length != x.Length)
            throw new ArgumentException("向量长度不匹配");

        if (typeof(T) == typeof(double))
        {
            SaxpyDouble(MemoryMarshal.Cast<T, double>(y), MemoryMarshal.Cast<T, double>(x), (double)(object)alpha!);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            SaxpyFloat(MemoryMarshal.Cast<T, float>(y), MemoryMarshal.Cast<T, float>(x), (float)(object)alpha!);
            return;
        }

        for (int i = 0; i < y.Length; i++)
            y[i] += alpha * x[i];
    }

    /// <summary>y -= alpha * x。</summary>
    public static void SubScaled<T>(Span<T> y, ReadOnlySpan<T> x, T alpha) where T : struct, IFloatingPointIeee754<T>
    {
        if (y.Length != x.Length)
            throw new ArgumentException("向量长度不匹配");

        if (typeof(T) == typeof(double))
        {
            SaxpyDouble(MemoryMarshal.Cast<T, double>(y), MemoryMarshal.Cast<T, double>(x), -(double)(object)alpha!);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            SaxpyFloat(MemoryMarshal.Cast<T, float>(y), MemoryMarshal.Cast<T, float>(x), -(float)(object)alpha!);
            return;
        }

        for (int i = 0; i < y.Length; i++)
            y[i] -= alpha * x[i];
    }

    /// <summary>y *= scale。</summary>
    public static void Scale<T>(Span<T> y, T scale) where T : struct, IFloatingPointIeee754<T>
    {
        if (typeof(T) == typeof(double))
        {
            ScaleDouble(MemoryMarshal.Cast<T, double>(y), (double)(object)scale!);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            ScaleFloat(MemoryMarshal.Cast<T, float>(y), (float)(object)scale!);
            return;
        }

        for (int i = 0; i < y.Length; i++)
            y[i] *= scale;
    }

    /// <summary>对称矩阵 BFGS 秩二修正（行主序扁平存储）。</summary>
    public static void SymmetricBfgsUpdate<T>(
        T[] hFlat, int n, ReadOnlySpan<T> s, ReadOnlySpan<T> hy, T invSy, T factor)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (s.Length < n || hy.Length < n || hFlat.Length < n * n)
            throw new ArgumentException("矩阵/向量维数不匹配");

        if (typeof(T) == typeof(double))
        {
            SymmetricBfgsUpdateDouble(
                (double[])(object)hFlat, n,
                MemoryMarshal.Cast<T, double>(s),
                MemoryMarshal.Cast<T, double>(hy),
                (double)(object)invSy!,
                (double)(object)factor!);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            SymmetricBfgsUpdateFloat(
                (float[])(object)hFlat, n,
                MemoryMarshal.Cast<T, float>(s),
                MemoryMarshal.Cast<T, float>(hy),
                (float)(object)invSy!,
                (float)(object)factor!);
            return;
        }

        for (int i = 0; i < n; i++)
        {
            T si = s[i];
            T hyi = hy[i];
            int row = i * n;
            for (int j = 0; j < n; j++)
            {
                int idx = row + j;
                hFlat[idx] += factor * si * s[j] - invSy * (si * hy[j] + hyi * s[j]);
            }
        }
    }

    /// <summary>RK4 终步：y += (step/6)·(k1 + 2k2 + 2k3 + k4)。</summary>
    public static void Rk4Accumulate<T>(
        Span<T> y,
        ReadOnlySpan<T> k1, ReadOnlySpan<T> k2, ReadOnlySpan<T> k3, ReadOnlySpan<T> k4,
        T stepOverSix, T twoStepOverSix)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (y.Length != k1.Length || y.Length != k2.Length || y.Length != k3.Length || y.Length != k4.Length)
            throw new ArgumentException("向量长度不匹配");

        if (typeof(T) == typeof(double))
        {
            Rk4AccumulateDouble(
                MemoryMarshal.Cast<T, double>(y),
                MemoryMarshal.Cast<T, double>(k1),
                MemoryMarshal.Cast<T, double>(k2),
                MemoryMarshal.Cast<T, double>(k3),
                MemoryMarshal.Cast<T, double>(k4),
                (double)(object)stepOverSix!,
                (double)(object)twoStepOverSix!);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            Rk4AccumulateFloat(
                MemoryMarshal.Cast<T, float>(y),
                MemoryMarshal.Cast<T, float>(k1),
                MemoryMarshal.Cast<T, float>(k2),
                MemoryMarshal.Cast<T, float>(k3),
                MemoryMarshal.Cast<T, float>(k4),
                (float)(object)stepOverSix!,
                (float)(object)twoStepOverSix!);
            return;
        }

        for (int i = 0; i < y.Length; i++)
            y[i] += stepOverSix * (k1[i] + k4[i]) + twoStepOverSix * (k2[i] + k3[i]);
    }

    /// <summary>自适应步长：max_i |yFull[i] - yHalf[i]| / scale_i。</summary>
    public static T ScaledMaxError<T>(ReadOnlySpan<T> yFull, ReadOnlySpan<T> yHalf, T rtol, T atol, T scaleFloor)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (yFull.Length != yHalf.Length)
            throw new ArgumentException("向量长度不匹配");

        if (typeof(T) == typeof(double))
            return (T)(object)ScaledMaxErrorDouble(
                MemoryMarshal.Cast<T, double>(yFull),
                MemoryMarshal.Cast<T, double>(yHalf),
                (double)(object)rtol!,
                (double)(object)atol!,
                (double)(object)scaleFloor!);

        if (typeof(T) == typeof(float))
            return (T)(object)ScaledMaxErrorFloat(
                MemoryMarshal.Cast<T, float>(yFull),
                MemoryMarshal.Cast<T, float>(yHalf),
                (float)(object)rtol!,
                (float)(object)atol!,
                (float)(object)scaleFloor!);

        T maxErr = T.Zero;
        for (int i = 0; i < yFull.Length; i++)
        {
            T scale = atol + rtol * T.Max(T.Abs(yFull[i]), T.Abs(yHalf[i]));
            T err = T.Abs(yFull[i] - yHalf[i]) / T.Max(scale, scaleFloor);
            if (err > maxErr) maxErr = err;
        }
        return maxErr;
    }

    /// <summary>result = JᵀJ（对称，行主序雅可比）。</summary>
    public static void JacobianTransposeJacobian<T>(Matrix<T> jacobian, Matrix<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        int m = jacobian.Rows;
        int n = jacobian.Columns;
        if (result.Rows != n || result.Columns != n)
            throw new ArgumentException("结果矩阵维数须为 n×n", nameof(result));

        T[] jData = jacobian.GetInternalData();
        T[] rData = result.GetInternalData();
        Array.Clear(rData);

        if (typeof(T) == typeof(double))
        {
            JacobianTransposeJacobianDouble((double[])(object)jData, m, n, (double[])(object)rData);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            JacobianTransposeJacobianFloat((float[])(object)jData, m, n, (float[])(object)rData);
            return;
        }

        for (int k = 0; k < m; k++)
        {
            int rowBase = k * n;
            for (int i = 0; i < n; i++)
            {
                T ji = jData[rowBase + i];
                for (int j = i; j < n; j++)
                {
                    T v = ji * jData[rowBase + j];
                    rData[i * n + j] += v;
                    if (j != i)
                        rData[j * n + i] = rData[i * n + j];
                }
            }
        }
    }

    /// <summary>result = Jᵀr。</summary>
    public static void JacobianTransposeVector<T>(Matrix<T> jacobian, ReadOnlySpan<T> residuals, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        int m = jacobian.Rows;
        int n = jacobian.Columns;
        if (residuals.Length < m || result.Length < n)
            throw new ArgumentException("向量维数不匹配");

        result[..n].Clear();
        T[] jData = jacobian.GetInternalData();

        if (typeof(T) == typeof(double))
        {
            JacobianTransposeVectorDouble(
                (double[])(object)jData, m, n,
                MemoryMarshal.Cast<T, double>(residuals),
                MemoryMarshal.Cast<T, double>(result));
            return;
        }

        if (typeof(T) == typeof(float))
        {
            JacobianTransposeVectorFloat(
                (float[])(object)jData, m, n,
                MemoryMarshal.Cast<T, float>(residuals),
                MemoryMarshal.Cast<T, float>(result));
            return;
        }

        for (int k = 0; k < m; k++)
        {
            T rk = residuals[k];
            int rowBase = k * n;
            for (int i = 0; i < n; i++)
                result[i] += rk * jData[rowBase + i];
        }
    }

    private static double DotDouble(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
    {
        int vectorSize = Vector<double>.Count;
        Vector<double> vSum = Vector<double>.Zero;
        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
            vSum += new Vector<double>(a.Slice(i, vectorSize)) * new Vector<double>(b.Slice(i, vectorSize));

        double sum = 0;
        for (int j = 0; j < vectorSize; j++)
            sum += vSum[j];
        for (; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    private static float DotFloat(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        int vectorSize = Vector<float>.Count;
        Vector<float> vSum = Vector<float>.Zero;
        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
            vSum += new Vector<float>(a.Slice(i, vectorSize)) * new Vector<float>(b.Slice(i, vectorSize));

        float sum = 0;
        for (int j = 0; j < vectorSize; j++)
            sum += vSum[j];
        for (; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    private static void MatVecDouble(ReadOnlySpan<double> m, int n, ReadOnlySpan<double> v, Span<double> result)
    {
        int vectorSize = Vector<double>.Count;
        for (int i = 0; i < n; i++)
        {
            ReadOnlySpan<double> row = m.Slice(i * n, n);
            Vector<double> vSum = Vector<double>.Zero;
            int j = 0;
            for (; j <= n - vectorSize; j += vectorSize)
                vSum += new Vector<double>(row.Slice(j, vectorSize)) * new Vector<double>(v.Slice(j, vectorSize));

            double sum = 0;
            for (int k = 0; k < vectorSize; k++)
                sum += vSum[k];
            for (; j < n; j++)
                sum += row[j] * v[j];
            result[i] = sum;
        }
    }

    private static void MatVecFloat(ReadOnlySpan<float> m, int n, ReadOnlySpan<float> v, Span<float> result)
    {
        int vectorSize = Vector<float>.Count;
        for (int i = 0; i < n; i++)
        {
            ReadOnlySpan<float> row = m.Slice(i * n, n);
            Vector<float> vSum = Vector<float>.Zero;
            int j = 0;
            for (; j <= n - vectorSize; j += vectorSize)
                vSum += new Vector<float>(row.Slice(j, vectorSize)) * new Vector<float>(v.Slice(j, vectorSize));

            float sum = 0;
            for (int k = 0; k < vectorSize; k++)
                sum += vSum[k];
            for (; j < n; j++)
                sum += row[j] * v[j];
            result[i] = sum;
        }
    }

    private static void JacobianTransposeJacobianDouble(double[] jData, int m, int n, double[] rData)
    {
        ReadOnlySpan<double> jSpan = jData;
        Span<double> rSpan = rData;
        for (int k = 0; k < m; k++)
        {
            ReadOnlySpan<double> row = jSpan.Slice(k * n, n);
            for (int i = 0; i < n; i++)
            {
                double ji = row[i];
                for (int j = i; j < n; j++)
                {
                    double v = ji * row[j];
                    rSpan[i * n + j] += v;
                    if (j != i)
                        rSpan[j * n + i] = rSpan[i * n + j];
                }
            }
        }
    }

    private static void JacobianTransposeJacobianFloat(float[] jData, int m, int n, float[] rData)
    {
        ReadOnlySpan<float> jSpan = jData;
        Span<float> rSpan = rData;
        for (int k = 0; k < m; k++)
        {
            ReadOnlySpan<float> row = jSpan.Slice(k * n, n);
            for (int i = 0; i < n; i++)
            {
                float ji = row[i];
                for (int j = i; j < n; j++)
                {
                    float v = ji * row[j];
                    rSpan[i * n + j] += v;
                    if (j != i)
                        rSpan[j * n + i] = rSpan[i * n + j];
                }
            }
        }
    }

    private static void JacobianTransposeVectorDouble(double[] jData, int m, int n, ReadOnlySpan<double> residuals, Span<double> result)
    {
        ReadOnlySpan<double> jSpan = jData;
        for (int k = 0; k < m; k++)
            SaxpyDouble(result[..n], jSpan.Slice(k * n, n), residuals[k]);
    }

    private static void JacobianTransposeVectorFloat(float[] jData, int m, int n, ReadOnlySpan<float> residuals, Span<float> result)
    {
        ReadOnlySpan<float> jSpan = jData;
        for (int k = 0; k < m; k++)
            SaxpyFloat(result[..n], jSpan.Slice(k * n, n), residuals[k]);
    }

    private static void AssignPlusScaledDouble(Span<double> dest, ReadOnlySpan<double> base_, ReadOnlySpan<double> delta, double scale)
    {
        int vectorSize = Vector<double>.Count;
        Vector<double> scaleVec = Vector<double>.One * scale;
        int i = 0;
        for (; i <= dest.Length - vectorSize; i += vectorSize)
        {
            (new Vector<double>(base_.Slice(i, vectorSize)) + new Vector<double>(delta.Slice(i, vectorSize)) * scaleVec)
                .CopyTo(dest.Slice(i, vectorSize));
        }

        for (; i < dest.Length; i++)
            dest[i] = base_[i] + scale * delta[i];
    }

    private static void AssignPlusScaledFloat(Span<float> dest, ReadOnlySpan<float> base_, ReadOnlySpan<float> delta, float scale)
    {
        int vectorSize = Vector<float>.Count;
        Vector<float> scaleVec = Vector<float>.One * scale;
        int i = 0;
        for (; i <= dest.Length - vectorSize; i += vectorSize)
        {
            (new Vector<float>(base_.Slice(i, vectorSize)) + new Vector<float>(delta.Slice(i, vectorSize)) * scaleVec)
                .CopyTo(dest.Slice(i, vectorSize));
        }

        for (; i < dest.Length; i++)
            dest[i] = base_[i] + scale * delta[i];
    }

    private static void ScaleDouble(Span<double> y, double scale)
    {
        int vectorSize = Vector<double>.Count;
        Vector<double> scaleVec = Vector<double>.One * scale;
        int i = 0;
        for (; i <= y.Length - vectorSize; i += vectorSize)
            (new Vector<double>(y.Slice(i, vectorSize)) * scaleVec).CopyTo(y.Slice(i, vectorSize));

        for (; i < y.Length; i++)
            y[i] *= scale;
    }

    private static void ScaleFloat(Span<float> y, float scale)
    {
        int vectorSize = Vector<float>.Count;
        Vector<float> scaleVec = Vector<float>.One * scale;
        int i = 0;
        for (; i <= y.Length - vectorSize; i += vectorSize)
            (new Vector<float>(y.Slice(i, vectorSize)) * scaleVec).CopyTo(y.Slice(i, vectorSize));

        for (; i < y.Length; i++)
            y[i] *= scale;
    }

    private static void SymmetricBfgsUpdateDouble(
        double[] hFlat, int n, ReadOnlySpan<double> s, ReadOnlySpan<double> hy, double invSy, double factor)
    {
        int vectorSize = Vector<double>.Count;
        for (int i = 0; i < n; i++)
        {
            double si = s[i];
            double hyi = hy[i];
            double negB = invSy * hyi;
            int row = i * n;
            int j = 0;
            Vector<double> vSi = Vector<double>.One * si;
            Vector<double> vFactor = Vector<double>.One * factor;
            Vector<double> vInvSy = Vector<double>.One * invSy;
            Vector<double> vNegB = Vector<double>.One * negB;

            for (; j <= n - vectorSize; j += vectorSize)
            {
                Vector<double> vs = new(s.Slice(j, vectorSize));
                Vector<double> vhy = new(hy.Slice(j, vectorSize));
                Vector<double> delta = vSi * (vFactor * vs - vInvSy * vhy) + vNegB * vs;
                (new Vector<double>(hFlat.AsSpan(row + j, vectorSize)) + delta)
                    .CopyTo(hFlat.AsSpan(row + j, vectorSize));
            }

            for (; j < n; j++)
            {
                int idx = row + j;
                hFlat[idx] += factor * si * s[j] - invSy * (si * hy[j] + hyi * s[j]);
            }
        }
    }

    private static void SymmetricBfgsUpdateFloat(
        float[] hFlat, int n, ReadOnlySpan<float> s, ReadOnlySpan<float> hy, float invSy, float factor)
    {
        int vectorSize = Vector<float>.Count;
        for (int i = 0; i < n; i++)
        {
            float si = s[i];
            float hyi = hy[i];
            float negB = invSy * hyi;
            int row = i * n;
            int j = 0;
            Vector<float> vSi = Vector<float>.One * si;
            Vector<float> vFactor = Vector<float>.One * factor;
            Vector<float> vInvSy = Vector<float>.One * invSy;
            Vector<float> vNegB = Vector<float>.One * negB;

            for (; j <= n - vectorSize; j += vectorSize)
            {
                Vector<float> vs = new(s.Slice(j, vectorSize));
                Vector<float> vhy = new(hy.Slice(j, vectorSize));
                Vector<float> delta = vSi * (vFactor * vs - vInvSy * vhy) + vNegB * vs;
                (new Vector<float>(hFlat.AsSpan(row + j, vectorSize)) + delta)
                    .CopyTo(hFlat.AsSpan(row + j, vectorSize));
            }

            for (; j < n; j++)
            {
                int idx = row + j;
                hFlat[idx] += factor * si * s[j] - invSy * (si * hy[j] + hyi * s[j]);
            }
        }
    }

    private static void Rk4AccumulateDouble(
        Span<double> y, ReadOnlySpan<double> k1, ReadOnlySpan<double> k2, ReadOnlySpan<double> k3, ReadOnlySpan<double> k4,
        double stepOverSix, double twoStepOverSix)
    {
        int vectorSize = Vector<double>.Count;
        Vector<double> vC1 = Vector<double>.One * stepOverSix;
        Vector<double> vC2 = Vector<double>.One * twoStepOverSix;
        int i = 0;
        for (; i <= y.Length - vectorSize; i += vectorSize)
        {
            Vector<double> sum = new Vector<double>(k1.Slice(i, vectorSize)) + new Vector<double>(k4.Slice(i, vectorSize));
            sum = vC1 * sum + vC2 * (new Vector<double>(k2.Slice(i, vectorSize)) + new Vector<double>(k3.Slice(i, vectorSize)));
            (new Vector<double>(y.Slice(i, vectorSize)) + sum).CopyTo(y.Slice(i, vectorSize));
        }

        for (; i < y.Length; i++)
            y[i] += stepOverSix * (k1[i] + k4[i]) + twoStepOverSix * (k2[i] + k3[i]);
    }

    private static void Rk4AccumulateFloat(
        Span<float> y, ReadOnlySpan<float> k1, ReadOnlySpan<float> k2, ReadOnlySpan<float> k3, ReadOnlySpan<float> k4,
        float stepOverSix, float twoStepOverSix)
    {
        int vectorSize = Vector<float>.Count;
        Vector<float> vC1 = Vector<float>.One * stepOverSix;
        Vector<float> vC2 = Vector<float>.One * twoStepOverSix;
        int i = 0;
        for (; i <= y.Length - vectorSize; i += vectorSize)
        {
            Vector<float> sum = new Vector<float>(k1.Slice(i, vectorSize)) + new Vector<float>(k4.Slice(i, vectorSize));
            sum = vC1 * sum + vC2 * (new Vector<float>(k2.Slice(i, vectorSize)) + new Vector<float>(k3.Slice(i, vectorSize)));
            (new Vector<float>(y.Slice(i, vectorSize)) + sum).CopyTo(y.Slice(i, vectorSize));
        }

        for (; i < y.Length; i++)
            y[i] += stepOverSix * (k1[i] + k4[i]) + twoStepOverSix * (k2[i] + k3[i]);
    }

    private static void SaxpyDouble(Span<double> acc, ReadOnlySpan<double> x, double scale)
    {
        int vectorSize = Vector<double>.Count;
        Vector<double> scaleVec = Vector<double>.One * scale;
        int i = 0;
        for (; i <= acc.Length - vectorSize; i += vectorSize)
        {
            (new Vector<double>(acc.Slice(i, vectorSize)) + new Vector<double>(x.Slice(i, vectorSize)) * scaleVec)
                .CopyTo(acc.Slice(i, vectorSize));
        }

        for (; i < acc.Length; i++)
            acc[i] += scale * x[i];
    }

    private static double ScaledMaxErrorDouble(ReadOnlySpan<double> yFull, ReadOnlySpan<double> yHalf, double rtol, double atol, double scaleFloor)
    {
        int vectorSize = Vector<double>.Count;
        double maxErr = 0;
        int i = 0;

        if (vectorSize >= 4)
        {
            Vector<double> vMax = Vector<double>.Zero;
            Vector<double> vRtol = Vector<double>.One * rtol;
            Vector<double> vAtol = Vector<double>.One * atol;
            Vector<double> vFloor = Vector<double>.One * scaleFloor;

            for (; i <= yFull.Length - vectorSize; i += vectorSize)
            {
                Vector<double> full = new(yFull.Slice(i, vectorSize));
                Vector<double> half = new(yHalf.Slice(i, vectorSize));
                Vector<double> scale = vAtol + vRtol * Vector.Max(Vector.Abs(full), Vector.Abs(half));
                Vector<double> err = Vector.Abs(full - half) / Vector.Max(scale, vFloor);
                vMax = Vector.Max(vMax, err);
            }

            for (int j = 0; j < vectorSize; j++)
                if (vMax[j] > maxErr) maxErr = vMax[j];
        }

        for (; i < yFull.Length; i++)
        {
            double scale = atol + rtol * Math.Max(Math.Abs(yFull[i]), Math.Abs(yHalf[i]));
            double err = Math.Abs(yFull[i] - yHalf[i]) / Math.Max(scale, scaleFloor);
            if (err > maxErr) maxErr = err;
        }
        return maxErr;
    }

    private static float ScaledMaxErrorFloat(ReadOnlySpan<float> yFull, ReadOnlySpan<float> yHalf, float rtol, float atol, float scaleFloor)
    {
        int vectorSize = Vector<float>.Count;
        float maxErr = 0;
        int i = 0;

        if (vectorSize >= 4)
        {
            Vector<float> vMax = Vector<float>.Zero;
            Vector<float> vRtol = Vector<float>.One * rtol;
            Vector<float> vAtol = Vector<float>.One * atol;
            Vector<float> vFloor = Vector<float>.One * scaleFloor;

            for (; i <= yFull.Length - vectorSize; i += vectorSize)
            {
                Vector<float> full = new(yFull.Slice(i, vectorSize));
                Vector<float> half = new(yHalf.Slice(i, vectorSize));
                Vector<float> scale = vAtol + vRtol * Vector.Max(Vector.Abs(full), Vector.Abs(half));
                Vector<float> err = Vector.Abs(full - half) / Vector.Max(scale, vFloor);
                vMax = Vector.Max(vMax, err);
            }

            for (int j = 0; j < vectorSize; j++)
                if (vMax[j] > maxErr) maxErr = vMax[j];
        }

        for (; i < yFull.Length; i++)
        {
            float scale = atol + rtol * MathF.Max(MathF.Abs(yFull[i]), MathF.Abs(yHalf[i]));
            float err = MathF.Abs(yFull[i] - yHalf[i]) / MathF.Max(scale, scaleFloor);
            if (err > maxErr) maxErr = err;
        }
        return maxErr;
    }

    private static void SaxpyFloat(Span<float> acc, ReadOnlySpan<float> x, float scale)
    {
        int vectorSize = Vector<float>.Count;
        Vector<float> scaleVec = Vector<float>.One * scale;
        int i = 0;
        for (; i <= acc.Length - vectorSize; i += vectorSize)
        {
            (new Vector<float>(acc.Slice(i, vectorSize)) + new Vector<float>(x.Slice(i, vectorSize)) * scaleVec)
                .CopyTo(acc.Slice(i, vectorSize));
        }

        for (; i < acc.Length; i++)
            acc[i] += scale * x[i];
    }
}
