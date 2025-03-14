﻿namespace Vorcyc.Mathematics.SignalProcessing;

/*
 *你的 Lpc 类功能属于数字信号处理 (DSP) 的范畴。具体来说，它涉及到线性预测编码 (LPC)，
 *这是数字信号处理中的一种技术，用于分析和合成语音信号。
 *LPC 通过预测当前样本值与之前样本值的线性组合来进行信号建模，这在语音编码和语音识别中非常常见。 
 * 
 */

using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Operations;

/// <summary>
/// Provides functions related to Linear Predictive Coding (LPC).
/// </summary>
public static class Lpc
{
    /// <summary>
    /// Evaluates LP coefficients using Levinson-Durbin algorithm and returns prediction error.
    /// </summary>
    /// <param name="input">Auto-correlation vector</param>
    /// <param name="a">LP coefficients</param>
    /// <param name="order">Order of LPC</param>
    /// <param name="offset">Optional offset in auto-correlation vector</param>
    public static float LevinsonDurbin(float[] input, float[] a, int order, int offset = 0)
    {
        var err = input[offset];

        a[0] = 1.0f;

        for (var i = 1; i <= order; i++)
        {
            var lambda = 0.0f;
            for (var j = 0; j < i; j++)
            {
                lambda -= a[j] * input[offset + i - j];
            }

            lambda /= err;

            for (var n = 0; n <= i / 2; n++)
            {
                var tmp = a[i - n] + lambda * a[n];
                a[n] = a[n] + lambda * a[i - n];
                a[i - n] = tmp;
            }

            err *= (1.0f - lambda * lambda);
        }

        return err;
    }

    /// <summary>
    /// Converts LPC coefficients to LPC cepstrum (LPCC).
    /// </summary>
    /// <param name="lpc">LPC vector</param>
    /// <param name="gain">Gain</param>
    /// <param name="lpcc">LPC cepstrum</param>
    public static void ToCepstrum(float[] lpc, float gain, float[] lpcc)
    {
        var n = lpcc.Length;
        var p = lpc.Length;     // must be lpcOrder + 1 (!)

        lpcc[0] = MathF.Log(gain);

        for (var m = 1; m < MathF.Min(n, p); m++)
        {
            var acc = 0.0f;
            for (var k = 1; k < m; k++)
            {
                acc += k * lpcc[k] * lpc[m - k];
            }
            lpcc[m] = -lpc[m] - acc / m;
        }

        for (var m = p; m < n; m++)
        {
            var acc = 0.0f;
            for (var k = 1; k < p; k++)
            {
                acc += (m - k) * lpcc[m - k] * lpc[k];
            }
            lpcc[m] = -acc / m;
        }
    }

    /// <summary>
    /// Converts LPC cepstrum to LPC coefficients and returns gain.
    /// </summary>
    /// <param name="lpcc">LPC cepstrum</param>
    /// <param name="lpc">LPC vector</param>
    public static float FromCepstrum(float[] lpcc, float[] lpc)
    {
        // Formulae: https://www.mathworks.com/help/dsp/ref/lpctofromcepstralcoefficients.html

        var p = lpc.Length;     // must be lpcOrder + 1 (!)

        lpc[0] = 1;

        for (var m = 1; m < p; m++)
        {
            var acc = 0.0f;
            for (var k = 1; k < m; k++)
            {
                acc += k * lpcc[k] * lpc[m - k];
            }
            lpc[m] = -lpcc[m] - acc / m;
        }

        return MathF.Exp(lpcc[0]);
    }

    /// <summary>
    /// Estimates LPC order for a given <paramref name="samplingRate"/> according to the best practices.
    /// </summary>
    /// <param name="samplingRate">Sampling rate</param>
    public static int EstimateOrder(int samplingRate)
    {
        return 2 + samplingRate / 1000;
    }

    /// <summary>
    /// Converts LPC coefficients to Line Spectral Frequencies <paramref name="lsf"/>. 
    /// The length of <paramref name="lsf"/> must be equal to <paramref name="lpc"/> length. Last element will be PI.
    /// </summary>
    /// <param name="lpc">LPC vector</param>
    /// <param name="lsf">Line spectral frequencies</param>
    public static void ToLsf(float[] lpc, float[] lsf)
    {
        var first = lpc[0];

        if (MathF.Abs(first - 1) > 1e-10f)
        {
            for (var i = 0; i < lpc.Length; lpc[i] /= first, i++) ;
        }

        var p = new float[lpc.Length + 1];
        var q = new float[lpc.Length + 1];

        p[0] = q[0] = 1;

        for (var i = 1; i < p.Length - 1; i++)
        {
            p[i] = lpc[i] - lpc[p.Length - i - 1];
            q[i] = lpc[i] + lpc[p.Length - i - 1];
        }

        p[p.Length - 1] = -1;
        q[q.Length - 1] = 1;

        var pRoots = VMath.PolynomialRoots(p).Select(r => r.Phase).ToArray();
        var qRoots = VMath.PolynomialRoots(q).Select(r => r.Phase).ToArray();

        Array.Sort(pRoots);
        Array.Sort(qRoots);

        var k = 0;
        for (var i = 0; i < qRoots.Length; i++)
        {
            if (qRoots[i] > 0) lsf[k++] = qRoots[i];
        }
        for (var i = 0; i < pRoots.Length; i++)
        {
            if (pRoots[i] > 0) lsf[k++] = pRoots[i];
        }

        Array.Sort(lsf);
    }

    /// <summary>
    /// Converts Line Spectral Frequencies <paramref name="lsf"/> to LPC coefficients. 
    /// The length of <paramref name="lsf"/> must be equal to <paramref name="lpc"/> length. Last element must be PI.
    /// </summary>
    /// <param name="lsf">Line spectral frequencies</param>
    /// <param name="lpc">LPC vector</param>
    public static void FromLsf(float[] lsf, float[] lpc)
    {
        var n = lsf.Length - 1;
        var halfOrder = n / 2;

        var pPoles = new ComplexFp32[n];
        var qPoles = new ComplexFp32[n + 2 * (n % 2)];

        var k = 0;
        for (var i = 0; k < halfOrder; i += 2, k++)
        {
            qPoles[k] = new ComplexFp32(MathF.Cos(lsf[i]), MathF.Sin(lsf[i]));
            pPoles[k] = new ComplexFp32(MathF.Cos(lsf[i + 1]), MathF.Sin(lsf[i + 1]));
        }
        for (var i = 0; k < 2 * halfOrder; i += 2, k++)
        {
            qPoles[k] = new ComplexFp32(MathF.Cos(lsf[i]), MathF.Sin(-lsf[i]));
            pPoles[k] = new ComplexFp32(MathF.Cos(lsf[i + 1]), MathF.Sin(-lsf[i + 1]));
        }

        if (n % 2 == 1)
        {
            qPoles[n] = new ComplexFp32(MathF.Cos(lsf[n - 1]), MathF.Sin(lsf[n - 1]));
            qPoles[n + 1] = new ComplexFp32(MathF.Cos(lsf[n - 1]), MathF.Sin(-lsf[n - 1]));
        }

        var ps = new ComplexDiscreteSignal(1, TransferFunction.ZpToTf(pPoles));
        var qs = new ComplexDiscreteSignal(1, TransferFunction.ZpToTf(qPoles));

        if (n % 2 == 1)
        {
            ps = Operation.Convolve(ps, new ComplexDiscreteSignal(1, new[] { 1.0f, 0f, -1.0f }));
        }
        else
        {
            ps = Operation.Convolve(ps, new ComplexDiscreteSignal(1, new[] { 1.0f, -1.0f }));
            qs = Operation.Convolve(qs, new ComplexDiscreteSignal(1, new[] { 1.0f, 1.0f }));
        }

        for (var i = 0; i < lpc.Length; i++)
        {
            lpc[i] = 0.5f * (ps.Real[i] + qs.Real[i]);
        }
    }
}