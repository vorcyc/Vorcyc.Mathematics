using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Kalman;

namespace SP_module_test;

/// <summary>
/// 0.11.2: Kalman filter and RTS smoother of a slowly varying signal (<see cref="KalmanTrend"/>, <see cref="KalmanTrendFilter"/>).
/// The reference is independent of the implementation: the smoother of the local level model with a flat start is the solution of the tridiagonal
/// system (W + a D'D) x = W z, that of the smooth trend the solution of the pentadiagonal system (W + a D2'D2) x = W z (the discrete cubic smoothing
/// spline), a = measurement variance / process variance, W = 1 for a measured and 0 for a missing sample; and a textbook full-storage Kalman filter with
/// the Joseph form and an RTS smoother, written with plain 2 x 2 matrices.
/// </summary>
internal static class Kalman_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing the Kalman trend filter and smoother (0.11.2)...");

        LevelSmootherIsPenalizedLeastSquares();
        TrendSmootherIsCubicSmoothingSpline();
        SlopeIsDifferenceOfLevels();
        MissingSamplesAreBridged();
        GateRejectsOutliersLikeMissingSamples();
        GateFollowsAStepOfTheSignal();
        StreamingFilterEqualsBatchFilter();
        BlockedSmootherEqualsTextbookSmoother();
        InnovationsAreCalibrated();
        EquivalentWindow();
        MeasurementVarianceEstimate();
        ScaleAndOffsetInvariance();
        EdgeCases();
        LongSignal();
        ArgumentValidation();

        if (_failures != 0)
            throw new InvalidOperationException($"Kalman 0.11.2: {_failures} assertion(s) failed.");

        Console.WriteLine("Kalman_011_test: PASS");
    }

    // ---------------------------------------------------------------- the smoothers against penalized least squares

    static void LevelSmootherIsPenalizedLeastSquares()
    {
        foreach (var ratio in new[] { 1e-1, 1e-2, 1e-3, 1e-4 })
        {
            var (z, _) = Noisy(KalmanTrendModel.LocalLevel, 600, 1.0, ratio, seed: 11);
            var observed = Enumerable.Repeat(true, z.Length).ToArray();
            var reference = LevelReference(z, observed, 1 / ratio);

            var result = KalmanTrend.Smooth(ToFloat(z), KalmanTrendModel.LocalLevel, ratio, 1.0);
            Expect($"level smoother, ratio {ratio}", MaxDiff(result.Level, reference) < 2e-3, $"maxΔ={MaxDiff(result.Level, reference):E2}");
            Expect($"level smoother is smoothed, ratio {ratio}", result.IsSmoothed && result.MissingCount == 0 && result.RejectedCount == 0);
        }
    }

    static void TrendSmootherIsCubicSmoothingSpline()
    {
        foreach (var ratio in new[] { 1e-2, 1e-3, 1e-4, 1e-5 })
        {
            var (z, _) = Noisy(KalmanTrendModel.LocalTrend, 300, 1.0, ratio, seed: 23);
            var observed = Enumerable.Repeat(true, z.Length).ToArray();
            var reference = TrendReference(z, observed, 1 / ratio);

            var result = KalmanTrend.Smooth(ToFloat(z), KalmanTrendModel.LocalTrend, ratio, 1.0);
            Expect($"trend smoother, ratio {ratio}", MaxDiff(result.Level, reference) < 5e-3, $"maxΔ={MaxDiff(result.Level, reference):E2}");
        }
    }

    /// <summary>The model has level[t+1] = level[t] + slope[t] exactly, so the smoothed slope is the difference of the smoothed levels.</summary>
    static void SlopeIsDifferenceOfLevels()
    {
        var (z, _) = Noisy(KalmanTrendModel.LocalTrend, 500, 1.0, 1e-4, seed: 5);
        var result = KalmanTrend.Smooth(ToFloat(z), KalmanTrendModel.LocalTrend, 1e-4, 1.0, 0, KalmanTrendOutputs.Slope);
        var slope = result.Slope!;
        double worst = 0;
        for (var t = 0; t + 1 < z.Length; t++)
            worst = Math.Max(worst, Math.Abs(slope[t] - (result.Level[t + 1] - result.Level[t])));
        Expect("smoothed slope = difference of smoothed levels", worst < 1e-3, $"maxΔ={worst:E2}");

        var level = KalmanTrend.Smooth(ToFloat(z), KalmanTrendModel.LocalLevel, 1e-3, 1.0, 0, KalmanTrendOutputs.All);
        Expect("the local level model has no slope", level.Slope is null);
        Expect("slope is not allocated unless asked for", KalmanTrend.Smooth(ToFloat(z), KalmanTrendModel.LocalTrend, 1e-4, 1.0).Slope is null);
    }

    static void MissingSamplesAreBridged()
    {
        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var ratio = model == KalmanTrendModel.LocalLevel ? 5e-3 : 5e-5;
            var (z, _) = Noisy(model, 500, 1.0, ratio, seed: 31);
            var rng = new Random(77);
            var observed = new bool[z.Length];
            for (var i = 0; i < z.Length; i++)
                observed[i] = rng.NextDouble() > 0.2;
            for (var i = 200; i < 260; i++)
                observed[i] = false;                    // a gap, longer than the equivalent window of either model
            observed[0] = observed[1] = true;
            var data = new float[z.Length];
            for (var i = 0; i < z.Length; i++)
                data[i] = observed[i] ? (float)z[i] : (i % 2 == 0 ? float.NaN : float.PositiveInfinity);   // NaN and infinity are both missing

            var reference = model == KalmanTrendModel.LocalLevel ? LevelReference(z, observed, 1 / ratio) : TrendReference(z, observed, 1 / ratio);
            var result = KalmanTrend.Smooth(data, model, ratio, 1.0, 0, KalmanTrendOutputs.StdDev);
            Expect($"{model}: missing samples are bridged like penalized least squares", MaxDiff(result.Level, reference) < 5e-3, $"maxΔ={MaxDiff(result.Level, reference):E2}");
            Expect($"{model}: every estimate is finite", result.Level.All(float.IsFinite));
            Expect($"{model}: missing count", result.MissingCount == observed.Count(o => !o), $"{result.MissingCount}");

            // the middle of the gap is the least certain place
            var std = result.StdDev!;
            Expect($"{model}: the gap is less certain than its surroundings", std[230] > 1.3 * std[120] && std[230] > 1.3 * std[340], $"{std[230]:F3} vs {std[120]:F3}/{std[340]:F3}");
            Expect($"{model}: the middle is the least certain place of the gap", std[230] > std[199] && std[230] > std[260], $"{std[230]:F3} vs {std[199]:F3}/{std[260]:F3}");

            // the filter coasts: no measurement, so the estimate only moves along the model and the uncertainty only grows
            var filtered = KalmanTrend.Filter(data, model, ratio, 1.0, 0, KalmanTrendOutputs.StdDev);
            Expect($"{model}: the filter is uncertain at the end of the gap", filtered.StdDev![259] > 1.5 * filtered.StdDev[120], $"{filtered.StdDev[259]:F3} vs {filtered.StdDev[120]:F3}");
            Expect($"{model}: the filter's uncertainty only grows across the gap", Enumerable.Range(201, 59).All(t => filtered.StdDev[t] >= filtered.StdDev[t - 1]));
        }
    }

    // ---------------------------------------------------------------- gate

    static void GateRejectsOutliersLikeMissingSamples()
    {
        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var ratio = model == KalmanTrendModel.LocalLevel ? 2e-3 : 2e-5;
            var (z, _) = Noisy(model, 800, 1.0, ratio, seed: 41);
            var spikes = new[] { 100, 101, 350, 600, 777 };      // 100 and 101 are two in a row (less than the patience)
            var withSpikes = ToFloat(z);
            var withoutSpikes = ToFloat(z);
            foreach (var t in spikes)
            {
                withSpikes[t] += t % 2 == 0 ? 40f : -40f;
                withoutSpikes[t] = float.NaN;
            }

            var gated = KalmanTrend.Smooth(withSpikes, model, ratio, 1.0, 4.0);
            var bridged = KalmanTrend.Smooth(withoutSpikes, model, ratio, 1.0, 0.0);
            Expect($"{model}: the gate rejects exactly the spikes", gated.RejectedCount == spikes.Length && gated.BreakCount == 0, $"rejected {gated.RejectedCount}, breaks {gated.BreakCount}");
            Expect($"{model}: a gated spike is bridged like a missing sample", MaxDiff(gated.Level, bridged.Level) < 1e-5, $"maxΔ={MaxDiff(gated.Level, bridged.Level):E2}");

            var ungated = KalmanTrend.Smooth(withSpikes, model, ratio, 1.0, 0.0);
            Expect($"{model}: without the gate the spikes leak into the estimate", MaxDiff(ungated.Level, bridged.Level) > 0.1);

            var filteredGated = KalmanTrend.Filter(withSpikes, model, ratio, 1.0, 4.0);
            var filteredBridged = KalmanTrend.Filter(withoutSpikes, model, ratio, 1.0, 0.0);
            Expect($"{model}: the same for the filter", filteredGated.RejectedCount == spikes.Length && MaxDiff(filteredGated.Level, filteredBridged.Level) < 1e-5);
        }
    }

    static void GateFollowsAStepOfTheSignal()
    {
        const int n = 400, step = 200;
        var rng = new Random(61);
        var z = new float[n];
        for (var t = 0; t < n; t++)
            z[t] = (float)(Gauss(rng) + (t >= step ? 40 : 0));

        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var ratio = model == KalmanTrendModel.LocalLevel ? 1e-3 : 1e-5;
            var filtered = KalmanTrend.Filter(z, model, ratio, 1.0, 4.0);
            Expect($"{model}: the step is one break", filtered.BreakCount == 1, $"{filtered.BreakCount}");
            Expect($"{model}: the gate rejects the first samples of the step, then gives way", filtered.RejectedCount == 3, $"{filtered.RejectedCount}");

            // 3 rejected samples, then the 4th is taken: the filter is on the new level from the 4th sample on
            var errors = Enumerable.Range(step + 3, 20).Select(t => Math.Abs(filtered.Level[t] - 40)).ToArray();
            Expect($"{model}: the filter is on the new level after the step", errors.Max() < 4.0, $"max error {errors.Max():F2}");
            Expect($"{model}: it stays there", Enumerable.Range(step + 60, n - step - 60).All(t => Math.Abs(filtered.Level[t] - 40) < 1.5));

            var smoothed = KalmanTrend.Smooth(z, model, ratio, 1.0, 4.0);
            Expect($"{model}: the smoother does not lock on the old level", Math.Abs(smoothed.Level[n - 1] - 40) < 1.5 && Math.Abs(smoothed.Level[step - 40]) < 1.5);
            Expect($"{model}: the smoother does not smooth across the break", Math.Abs(smoothed.Level[step - 10]) < 2 && Math.Abs(smoothed.Level[step + 10] - 40) < 2,
                $"{smoothed.Level[step - 10]:F2} / {smoothed.Level[step + 10]:F2}");
        }
    }

    // ---------------------------------------------------------------- streaming, blocks

    static void StreamingFilterEqualsBatchFilter()
    {
        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var ratio = model == KalmanTrendModel.LocalLevel ? 3e-3 : 3e-5;
            var (z, _) = Noisy(model, 2000, 1.0, ratio, seed: 71);
            var data = ToFloat(z);
            var rng = new Random(5);
            for (var i = 10; i < data.Length; i++)
            {
                var u = rng.NextDouble();
                if (u < 0.05) data[i] = float.NaN;
                else if (u < 0.06) data[i] += 30f;
            }

            var batch = KalmanTrend.Filter(data, model, ratio, 1.0, 4.0, KalmanTrendOutputs.All);
            var filter = new KalmanTrendFilter(model, ratio, 1.0, 4.0);
            IOnlineFilter online = filter;
            var identical = true;
            var rejected = 0;
            for (var t = 0; t < data.Length; t++)
            {
                var y = online.Process(data[t]);
                identical &= y.Equals(batch.Level[t])
                    && filter.Slope.Equals(batch.Slope is null ? 0f : batch.Slope[t])
                    && filter.LevelStdDev.Equals(batch.StdDev![t])
                    && filter.StandardizedInnovation.Equals(batch.StandardizedInnovation![t]);
                if (filter.LastSampleRejected)
                    rejected++;
            }

            Expect($"{model}: the streaming filter gives the samples of the batch filter", identical);
            Expect($"{model}: the same rejections", rejected == batch.RejectedCount && rejected > 0, $"{rejected} vs {batch.RejectedCount}");

            filter.Reset();
            Expect($"{model}: Reset starts again", float.IsNaN(filter.Level) && online.Process(2.5f) == 2.5f);
        }

        var cold = new KalmanTrendFilter();
        Expect("the filter returns NaN before the first finite sample", float.IsNaN(cold.Process(float.NaN)) && float.IsNaN(cold.Process(float.PositiveInfinity)) && cold.Process(7f) == 7f);
        Expect("the model parameters are kept", cold.Model == KalmanTrendModel.LocalLevel && cold.ProcessToMeasurementRatio == 1e-3 && cold.MeasurementVariance == 1 && cold.GateSigma == 0);
    }

    /// <summary>The smoother stores a checkpoint every 4096 samples; more than three blocks must give what a full-storage smoother gives.</summary>
    static void BlockedSmootherEqualsTextbookSmoother()
    {
        const int n = 3 * 4096 + 17;
        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var ratio = model == KalmanTrendModel.LocalLevel ? 2e-3 : 2e-5;
            var (z, _) = Noisy(model, n, 1.0, ratio, seed: 83);
            var data = ToFloat(z);
            var rng = new Random(9);
            for (var i = 1; i < n; i++)                  // the first sample stays: the filter has no estimate before the first finite sample
                if (rng.NextDouble() < 0.1)
                    data[i] = float.NaN;
            for (var i = 4090; i < 4110; i++)
                data[i] = float.NaN;                    // a gap over a block boundary
            for (var i = 8190; i < 8194; i++)
                data[i] = float.NaN;

            var expected = TextbookSmoother(data, model == KalmanTrendModel.LocalTrend, ratio, 1.0);
            var result = KalmanTrend.Smooth(data, model, ratio, 1.0, 0, KalmanTrendOutputs.Slope | KalmanTrendOutputs.StdDev);
            Expect($"{model}: blocked smoother, level", MaxDiff(result.Level, expected.Level) < 1e-4, $"maxΔ={MaxDiff(result.Level, expected.Level):E2}");
            Expect($"{model}: blocked smoother, standard deviation", MaxDiff(result.StdDev!, expected.StdDev) < 1e-4, $"maxΔ={MaxDiff(result.StdDev!, expected.StdDev):E2}");
            if (model == KalmanTrendModel.LocalTrend)
                Expect($"{model}: blocked smoother, slope", MaxDiff(result.Slope!, expected.Slope) < 1e-4, $"maxΔ={MaxDiff(result.Slope!, expected.Slope):E2}");

            var filtered = KalmanTrend.Filter(data, model, ratio, 1.0, 0, KalmanTrendOutputs.StdDev);
            Expect($"{model}: the filter is the textbook forward pass", MaxDiff(filtered.Level, expected.Filtered) < 1e-4, $"maxΔ={MaxDiff(filtered.Level, expected.Filtered):E2}");

            // looking ahead can only reduce the uncertainty
            var worse = 0;
            for (var t = 0; t < n; t++)
                if (result.StdDev![t] > filtered.StdDev![t] + 1e-4f)
                    worse++;
            Expect($"{model}: the smoothed standard deviation is not above the filtered one", worse == 0, $"{worse} samples");
        }
    }

    // ---------------------------------------------------------------- statistics

    /// <summary>With the true model the standardized innovations are white with unit variance.</summary>
    static void InnovationsAreCalibrated()
    {
        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var ratio = model == KalmanTrendModel.LocalLevel ? 1e-2 : 1e-4;
            var (z, _) = Noisy(model, 40000, 2.0, ratio, seed: 97);        // measurement variance 4
            var result = KalmanTrend.Filter(ToFloat(z), model, ratio, 4.0, 0, KalmanTrendOutputs.StandardizedInnovation);
            var e = result.StandardizedInnovation!.Skip(2000).Select(v => (double)v).ToArray();
            var mean = e.Average();
            var variance = e.Select(v => (v - mean) * (v - mean)).Average();
            double lag1 = 0;
            for (var i = 1; i < e.Length; i++)
                lag1 += (e[i] - mean) * (e[i - 1] - mean);
            lag1 /= e.Length * variance;
            Expect($"{model}: innovations have zero mean", Math.Abs(mean) < 0.03, $"{mean:F3}");
            Expect($"{model}: innovations have unit variance", Math.Abs(variance - 1) < 0.05, $"{variance:F3}");
            Expect($"{model}: innovations are white", Math.Abs(lag1) < 0.03, $"{lag1:F3}");
            Expect($"{model}: the process variance is the ratio times the measurement variance", Math.Abs(result.ProcessVariance - 4 * ratio) < 1e-12 && result.MeasurementVariance == 4);
        }
    }

    static void EquivalentWindow()
    {
        // the asymptotics for a small ratio
        Expect("window, local level, small ratio", Math.Abs(KalmanTrend.EquivalentWindow(KalmanTrendModel.LocalLevel, 1e-6) / 4000 - 1) < 1e-3, $"{KalmanTrend.EquivalentWindow(KalmanTrendModel.LocalLevel, 1e-6):F1}");
        Expect("window, trend, small ratio", Math.Abs(KalmanTrend.EquivalentWindow(KalmanTrendModel.LocalTrend, 1e-8) / 377.12 - 1) < 2e-3, $"{KalmanTrend.EquivalentWindow(KalmanTrendModel.LocalTrend, 1e-8):F2}");
        Expect("window of a huge ratio is one sample", Math.Abs(KalmanTrend.EquivalentWindow(KalmanTrendModel.LocalLevel, 1e12) - 1) < 1e-5 && Math.Abs(KalmanTrend.EquivalentWindow(KalmanTrendModel.LocalTrend, 1e12) - 1) < 1e-3);

        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            // the window shrinks as the ratio grows, and the inverse finds the ratio again
            double previous = double.PositiveInfinity;
            foreach (var ratio in new[] { 1e-12, 1e-9, 1e-6, 1e-4, 1e-2, 1, 100 })
            {
                var w = KalmanTrend.EquivalentWindow(model, ratio);
                Expect($"{model}: window decreases with the ratio ({ratio})", w < previous && w >= 1 - 1e-9, $"{w}");
                previous = w;
                if (w > 1.5)
                {
                    var back = KalmanTrend.RatioForWindow(model, w);
                    Expect($"{model}: RatioForWindow inverts EquivalentWindow ({ratio})", Math.Abs(back / ratio - 1) < 1e-6, $"{back:E3}");
                }
            }

            foreach (var window in new[] { 1.5, 3.0, 10.0, 100.0, 1000.0, 20000.0 })
            {
                var ratio = KalmanTrend.RatioForWindow(model, window);
                Expect($"{model}: window {window} round trip", Math.Abs(KalmanTrend.EquivalentWindow(model, ratio) / window - 1) < 1e-6, $"{KalmanTrend.EquivalentWindow(model, ratio):F4}");
            }

            // the window is the variance reduction for white noise: the smoother of pure noise has the variance 1 / window
            foreach (var window in new[] { 8.0, 40.0 })
            {
                var ratio = KalmanTrend.RatioForWindow(model, window);
                var rng = new Random(101);
                var noise = new float[60000];
                for (var i = 0; i < noise.Length; i++)
                    noise[i] = (float)Gauss(rng);
                var smoothed = KalmanTrend.Smooth(noise, model, ratio, 1.0).Level;
                var interior = smoothed.Skip(1000).Take(noise.Length - 2000).Select(v => (double)v).ToArray();
                var variance = interior.Select(v => v * v).Average();
                Expect($"{model}: the smoother of white noise has variance 1/window ({window})", Math.Abs(variance * window - 1) < 0.12, $"{variance * window:F3}");
            }
        }
    }

    static void MeasurementVarianceEstimate()
    {
        var rng = new Random(113);
        var n = 20000;
        var x = new float[n];
        for (var i = 0; i < n; i++)
        {
            var slow = 10 * Math.Sin(2 * Math.PI * i / 5000.0) + 3 * Math.Sin(2 * Math.PI * i / 1300.0);
            x[i] = (float)(slow + 2.0 * Gauss(rng));                     // variance 4
        }

        var clean = KalmanTrend.EstimateMeasurementVariance(x);
        Expect("variance estimate", Math.Abs(clean / 4 - 1) < 0.08, $"{clean:F3}");

        for (var i = 0; i < n; i += 97)
            x[i] += 80f;                                                  // 1 % spikes
        var robust = KalmanTrend.EstimateMeasurementVariance(x);
        Expect("the variance estimate ignores outliers", Math.Abs(robust / 4 - 1) < 0.10, $"{robust:F3}");

        x[5] = float.NaN;
        x[6] = float.PositiveInfinity;
        Expect("the variance estimate skips samples that are not finite", Math.Abs(KalmanTrend.EstimateMeasurementVariance(x) / 4 - 1) < 0.10);

        Expect("too short to tell", KalmanTrend.EstimateMeasurementVariance(new float[] { 1, 2, 3 }) == 0 && KalmanTrend.EstimateMeasurementVariance(new float[] { 1, float.NaN, 3, float.NaN, 5, float.NaN }) == 0);
        Expect("a constant signal has no spread", KalmanTrend.EstimateMeasurementVariance(Enumerable.Repeat(3f, 100).ToArray()) == 0);

        // 0 and NaN estimate it from the signal, and a constant signal still gives a usable (positive) variance
        var a = KalmanTrend.Smooth(x, KalmanTrendModel.LocalLevel, 1e-3, double.NaN);
        var b = KalmanTrend.Smooth(x, KalmanTrendModel.LocalLevel, 1e-3, 0);
        Expect("NaN and 0 both mean: estimate", a.MeasurementVariance > 0 && a.MeasurementVariance == b.MeasurementVariance && MaxDiff(a.Level, b.Level) == 0);
        var constant = KalmanTrend.Smooth(Enumerable.Repeat(3f, 100).ToArray(), KalmanTrendModel.LocalTrend, 1e-4);
        Expect("a constant signal stays constant", constant.MeasurementVariance > 0 && constant.Level.All(v => Math.Abs(v - 3) < 1e-4));
        var zeros = KalmanTrend.Smooth(new float[50], KalmanTrendModel.LocalLevel, 1e-3);
        Expect("a signal of zeros", zeros.MeasurementVariance == 1 && zeros.Level.All(v => v == 0));
    }

    static void ScaleAndOffsetInvariance()
    {
        var (z, _) = Noisy(KalmanTrendModel.LocalTrend, 3000, 1.0, 1e-4, seed: 131);
        var data = ToFloat(z);
        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var reference = KalmanTrend.Smooth(data, model, 1e-4, double.NaN, 4.0, KalmanTrendOutputs.StdDev);
            foreach (var scale in new[] { 1e-6, 1e6 })
            {
                var scaled = KalmanTrend.Smooth(data.Select(v => v * (float)scale).ToArray(), model, 1e-4, double.NaN, 4.0, KalmanTrendOutputs.StdDev);
                var worst = 0.0;
                for (var t = 0; t < data.Length; t++)
                    worst = Math.Max(worst, Math.Abs(scaled.Level[t] / scale - reference.Level[t]));
                Expect($"{model}: the estimate scales with the signal (x{scale:E0})", worst < 1e-3, $"maxΔ={worst:E2}");
                Expect($"{model}: the variance scales with the square (x{scale:E0})", Math.Abs(scaled.MeasurementVariance / (scale * scale) / reference.MeasurementVariance - 1) < 1e-4);
                Expect($"{model}: the same rejections (x{scale:E0})", scaled.RejectedCount == reference.RejectedCount && scaled.BreakCount == reference.BreakCount);
            }

            var shifted = KalmanTrend.Smooth(data.Select(v => v + 1000f).ToArray(), model, 1e-4, double.NaN, 4.0);
            Expect($"{model}: an offset only shifts the estimate", MaxDiff(shifted.Level.Select(v => v - 1000f).ToArray(), reference.Level) < 5e-3, $"maxΔ={MaxDiff(shifted.Level.Select(v => v - 1000f).ToArray(), reference.Level):E2}");
        }
    }

    static void EdgeCases()
    {
        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var empty = KalmanTrend.Smooth(ReadOnlySpan<float>.Empty, model, 1e-3);
            Expect($"{model}: empty signal", empty.Level.Length == 0 && empty.MissingCount == 0);
            Expect($"{model}: empty signal, filter", KalmanTrend.Filter(ReadOnlySpan<float>.Empty, model, 1e-3).Level.Length == 0);

            var allMissing = KalmanTrend.Smooth(new[] { float.NaN, float.NaN, float.PositiveInfinity }, model, 1e-3, 1.0, 0, KalmanTrendOutputs.All);
            Expect($"{model}: nothing measured gives NaN everywhere", allMissing.Level.All(float.IsNaN) && allMissing.StdDev!.All(float.IsNaN) && allMissing.MissingCount == 3);
            Expect($"{model}: nothing measured, filter", KalmanTrend.Filter(new[] { float.NaN, float.NaN }, model, 1e-3).Level.All(float.IsNaN));

            var one = KalmanTrend.Smooth(new[] { 4.5f }, model, 1e-3, 1.0);
            Expect($"{model}: one sample", one.Level.Length == 1 && one.Level[0] == 4.5f);

            // missing samples before the first measurement: the filter does not know them yet, the smoother does
            var data = new[] { float.NaN, float.NaN, 5f, 5.1f, 4.9f, 5f, 5.05f, 4.95f };
            var filtered = KalmanTrend.Filter(data, model, 1e-3, 0.01);
            var smoothed = KalmanTrend.Smooth(data, model, 1e-3, 0.01);
            Expect($"{model}: leading missing samples, filter", float.IsNaN(filtered.Level[0]) && float.IsNaN(filtered.Level[1]) && Math.Abs(filtered.Level[2] - 5) < 1e-5 && filtered.MissingCount == 2);
            Expect($"{model}: leading missing samples, smoother", smoothed.Level.All(v => float.IsFinite(v) && Math.Abs(v - 5) < 0.5), string.Join(",", smoothed.Level.Select(v => v.ToString("F2"))));

            // extreme values stay finite
            var large = KalmanTrend.Smooth(Enumerable.Range(0, 200).Select(i => 1e30f * (1 + 0.01f * MathF.Sin(i))).ToArray(), model, 1e-2);
            Expect($"{model}: huge values", large.Level.All(float.IsFinite) && large.MeasurementVariance > 0);
            var tiny = KalmanTrend.Smooth(Enumerable.Range(0, 200).Select(i => 1e-30f * (1 + 0.01f * MathF.Sin(i))).ToArray(), model, 1e-2);
            Expect($"{model}: tiny values", tiny.Level.All(float.IsFinite) && tiny.MeasurementVariance > 0);

            // a very small ratio, a very large one
            var (z, _) = Noisy(model, 2000, 1.0, 1e-3, seed: 3);
            foreach (var ratio in new[] { 1e-18, 1e-12, 1e8 })
            {
                var r = KalmanTrend.Smooth(ToFloat(z), model, ratio, 1.0, 0, KalmanTrendOutputs.All);
                Expect($"{model}: ratio {ratio:E0} stays finite", r.Level.All(float.IsFinite) && r.StdDev!.All(v => float.IsFinite(v) && v >= 0));
            }

            // a huge ratio follows the data; a tiny ratio is a straight line (level: a constant)
            var follow = KalmanTrend.Smooth(ToFloat(z), model, 1e8, 1.0).Level;
            Expect($"{model}: a huge ratio follows the data", MaxDiff(follow, z) < 1e-2);
        }

        var line = Enumerable.Range(0, 500).Select(i => (float)(0.5 + 0.02 * i)).ToArray();
        var straight = KalmanTrend.Smooth(line, KalmanTrendModel.LocalTrend, 1e-12, 1.0, 0, KalmanTrendOutputs.Slope);
        Expect("the smooth trend model keeps a straight line", MaxDiff(straight.Level, line.Select(v => (double)v).ToArray()) < 1e-3 && Math.Abs(straight.Slope![250] - 0.02) < 1e-4);
    }

    static void LongSignal()
    {
        const int n = 2_000_000;
        var rng = new Random(7);
        var data = new float[n];
        double walk = 0;
        for (var i = 0; i < n; i++)
        {
            walk += 0.01 * Gauss(rng);
            data[i] = (float)(walk + Gauss(rng));
        }

        foreach (var model in new[] { KalmanTrendModel.LocalLevel, KalmanTrendModel.LocalTrend })
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = KalmanTrend.Smooth(data, model, 1e-4, double.NaN, 4.0, KalmanTrendOutputs.All);
            sw.Stop();
            Expect($"{model}: {n:N0} samples", result.Level.All(float.IsFinite) && result.MeasurementVariance > 0.5 && result.MeasurementVariance < 2, $"R = {result.MeasurementVariance:F3}");
            Console.WriteLine($"  {model}: {n:N0} samples smoothed with all outputs in {sw.ElapsedMilliseconds} ms");
        }
    }

    static void ArgumentValidation()
    {
        var x = new float[16];
        foreach (var bad in new[] { double.NaN, 0, -1e-3, double.PositiveInfinity, double.NegativeInfinity })
        {
            ExpectThrows<ArgumentOutOfRangeException>($"Smooth ratio {bad}", () => KalmanTrend.Smooth(x, KalmanTrendModel.LocalLevel, bad));
            ExpectThrows<ArgumentOutOfRangeException>($"Filter ratio {bad}", () => KalmanTrend.Filter(x, KalmanTrendModel.LocalLevel, bad));
            ExpectThrows<ArgumentOutOfRangeException>($"filter ratio {bad}", () => new KalmanTrendFilter(KalmanTrendModel.LocalLevel, bad));
            ExpectThrows<ArgumentOutOfRangeException>($"EquivalentWindow ratio {bad}", () => KalmanTrend.EquivalentWindow(KalmanTrendModel.LocalLevel, bad));
        }

        foreach (var bad in new[] { -1.0, double.PositiveInfinity, double.NegativeInfinity })
            ExpectThrows<ArgumentOutOfRangeException>($"measurement variance {bad}", () => KalmanTrend.Smooth(x, KalmanTrendModel.LocalLevel, 1e-3, bad));
        foreach (var bad in new[] { double.NaN, 0, -1, double.PositiveInfinity })
            ExpectThrows<ArgumentOutOfRangeException>($"filter measurement variance {bad}", () => new KalmanTrendFilter(KalmanTrendModel.LocalLevel, 1e-3, bad));

        foreach (var bad in new[] { 0.5, 0.999, -3, double.NaN, double.PositiveInfinity })
        {
            ExpectThrows<ArgumentOutOfRangeException>($"Smooth gate {bad}", () => KalmanTrend.Smooth(x, KalmanTrendModel.LocalLevel, 1e-3, 1.0, bad));
            ExpectThrows<ArgumentOutOfRangeException>($"filter gate {bad}", () => new KalmanTrendFilter(KalmanTrendModel.LocalLevel, 1e-3, 1.0, bad));
        }

        ExpectNoThrow("gate 1 (the least)", () => KalmanTrend.Smooth(x, KalmanTrendModel.LocalLevel, 1e-3, 1.0, 1.0));
        ExpectThrows<ArgumentOutOfRangeException>("unknown model", () => KalmanTrend.Smooth(x, (KalmanTrendModel)7, 1e-3));
        ExpectThrows<ArgumentOutOfRangeException>("filter unknown model", () => new KalmanTrendFilter((KalmanTrendModel)(-1)));
        ExpectThrows<ArgumentOutOfRangeException>("EquivalentWindow unknown model", () => KalmanTrend.EquivalentWindow((KalmanTrendModel)3, 1e-3));
        foreach (var bad in new[] { double.NaN, 1.0, 0.5, -4, double.PositiveInfinity })
            ExpectThrows<ArgumentOutOfRangeException>($"RatioForWindow {bad}", () => KalmanTrend.RatioForWindow(KalmanTrendModel.LocalTrend, bad));
    }

    // ---------------------------------------------------------------- reference solvers

    /// <summary>Solves (W + a D'D) x = W z for the first-difference penalty with the Thomas algorithm.</summary>
    static double[] LevelReference(double[] z, bool[] observed, double a)
    {
        var n = z.Length;
        var lower = new double[n];
        var diag = new double[n];
        var upper = new double[n];
        var rhs = new double[n];
        for (var i = 0; i < n; i++)
        {
            var w = observed[i] ? 1.0 : 0.0;
            diag[i] = w + a * ((i > 0 ? 1 : 0) + (i < n - 1 ? 1 : 0));
            lower[i] = i > 0 ? -a : 0;
            upper[i] = i < n - 1 ? -a : 0;
            rhs[i] = w * z[i];
        }

        for (var i = 1; i < n; i++)
        {
            var m = lower[i] / diag[i - 1];
            diag[i] -= m * upper[i - 1];
            rhs[i] -= m * rhs[i - 1];
        }

        var x = new double[n];
        x[n - 1] = rhs[n - 1] / diag[n - 1];
        for (var i = n - 2; i >= 0; i--)
            x[i] = (rhs[i] - upper[i] * x[i + 1]) / diag[i];
        return x;
    }

    /// <summary>Solves (W + a D2'D2) x = W z (the discrete cubic smoothing spline) by Gaussian elimination.</summary>
    static double[] TrendReference(double[] z, bool[] observed, double a)
    {
        var n = z.Length;
        var m = new double[n, n + 1];
        for (var i = 0; i < n; i++)
        {
            m[i, i] += observed[i] ? 1.0 : 0.0;
            m[i, n] = observed[i] ? z[i] : 0.0;
        }

        for (var i = 0; i + 2 < n; i++)
        {
            int[] idx = [i, i + 1, i + 2];
            double[] c = [1, -2, 1];
            for (var p = 0; p < 3; p++)
                for (var q = 0; q < 3; q++)
                    m[idx[p], idx[q]] += a * c[p] * c[q];
        }

        for (var col = 0; col < n; col++)
        {
            var pivot = col;
            for (var r = col + 1; r < n; r++)
                if (Math.Abs(m[r, col]) > Math.Abs(m[pivot, col]))
                    pivot = r;
            if (pivot != col)
                for (var k = col; k <= n; k++)
                    (m[col, k], m[pivot, k]) = (m[pivot, k], m[col, k]);
            for (var r = col + 1; r < n; r++)
            {
                var f = m[r, col] / m[col, col];
                if (f == 0)
                    continue;
                for (var k = col; k <= n; k++)
                    m[r, k] -= f * m[col, k];
            }
        }

        var x = new double[n];
        for (var i = n - 1; i >= 0; i--)
        {
            var s = m[i, n];
            for (var k = i + 1; k < n; k++)
                s -= m[i, k] * x[k];
            x[i] = s / m[i, i];
        }

        return x;
    }

    /// <summary>Plain 2 x 2 matrices: the textbook filter (Joseph form) and smoother, in full storage and with no block structure.</summary>
    readonly record struct M2(double A, double B, double C, double D)
    {
        public static M2 operator *(M2 x, M2 y) => new(x.A * y.A + x.B * y.C, x.A * y.B + x.B * y.D, x.C * y.A + x.D * y.C, x.C * y.B + x.D * y.D);
        public static M2 operator +(M2 x, M2 y) => new(x.A + y.A, x.B + y.B, x.C + y.C, x.D + y.D);
        public static M2 operator -(M2 x, M2 y) => new(x.A - y.A, x.B - y.B, x.C - y.C, x.D - y.D);
        public M2 T => new(A, C, B, D);
        public M2 Inverse()
        {
            var det = A * D - B * C;
            return new M2(D / det, -B / det, -C / det, A / det);
        }
    }

    static (double[] Level, double[] Slope, double[] StdDev, double[] Filtered) TextbookSmoother(float[] data, bool trend, double ratio, double r)
    {
        var n = data.Length;
        var f = trend ? new M2(1, 1, 0, 1) : new M2(1, 0, 0, 0);
        var q = trend ? new M2(0, 0, 0, ratio * r) : new M2(ratio * r, 0, 0, 0);
        var diffuse = 1e4 * r;
        var first = Array.FindIndex(data, float.IsFinite);

        var xf = new (double L, double S)[n];
        var pf = new M2[n];
        var xp = new (double L, double S)[n];
        var pp = new M2[n];

        (double L, double S) x = (data[first], 0);
        var p = trend ? new M2(diffuse, 0, 0, diffuse) : new M2(diffuse, 0, 0, 0);
        for (var t = 0; t < n; t++)
        {
            // predict
            x = trend ? (x.L + x.S, x.S) : (x.L, 0);
            p = f * p * f.T + q;
            xp[t] = x;
            pp[t] = p;
            if (float.IsFinite(data[t]))
            {
                var s = p.A + r;
                var k0 = p.A / s;
                var k1 = trend ? p.C / s : 0;
                var e = data[t] - x.L;
                x = (x.L + k0 * e, trend ? x.S + k1 * e : 0);
                // Joseph form: (I - K H) P (I - K H)' + K R K'
                var ikh = new M2(1 - k0, 0, -k1, 1);
                var kkt = new M2(k0 * k0, k0 * k1, k1 * k0, k1 * k1);
                p = ikh * p * ikh.T + new M2(r * kkt.A, r * kkt.B, r * kkt.C, r * kkt.D);
            }

            xf[t] = x;
            pf[t] = p;
        }

        var level = new double[n];
        var slope = new double[n];
        var std = new double[n];
        (double L, double S) xs = xf[n - 1];
        var ps = pf[n - 1];
        level[n - 1] = xs.L;
        slope[n - 1] = xs.S;
        std[n - 1] = Math.Sqrt(ps.A);
        for (var t = n - 2; t >= 0; t--)
        {
            M2 c;
            if (trend)
            {
                c = pf[t] * f.T * pp[t + 1].Inverse();
            }
            else
            {
                c = new M2(pf[t].A / pp[t + 1].A, 0, 0, 0);
            }

            xs = (xf[t].L + c.A * (xs.L - xp[t + 1].L) + c.B * (xs.S - xp[t + 1].S), xf[t].S + c.C * (xs.L - xp[t + 1].L) + c.D * (xs.S - xp[t + 1].S));
            ps = pf[t] + c * (ps - pp[t + 1]) * c.T;
            level[t] = xs.L;
            slope[t] = trend ? xs.S : 0;
            std[t] = Math.Sqrt(Math.Max(0, ps.A));
        }

        return (level, slope, std, xf.Select(v => v.L).ToArray());
    }

    // ---------------------------------------------------------------- data and helpers

    /// <summary>A realization of the model: a random walk (level) or an integrated random walk (trend) with the process variance ratio · sd², plus white noise of standard deviation sd.</summary>
    static (double[] Noisy, double[] Truth) Noisy(KalmanTrendModel model, int n, double sd, double ratio, int seed)
    {
        var rng = new Random(seed);
        var truth = new double[n];
        double level = 0, slope = 0;
        var q = Math.Sqrt(ratio) * sd;
        for (var t = 0; t < n; t++)
        {
            if (model == KalmanTrendModel.LocalLevel)
            {
                level += q * Gauss(rng);
            }
            else
            {
                level += slope;
                slope += q * Gauss(rng);
            }

            truth[t] = level;
        }

        var noisy = new double[n];
        for (var t = 0; t < n; t++)
            noisy[t] = truth[t] + sd * Gauss(rng);
        return (noisy, truth);
    }

    static double Gauss(Random rng)
    {
        var u1 = 1.0 - rng.NextDouble();
        var u2 = rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    static float[] ToFloat(double[] x) => x.Select(v => (float)v).ToArray();

    static double MaxDiff(float[] a, double[] b)
    {
        if (a.Length != b.Length)
            return double.PositiveInfinity;
        double max = 0;
        for (var i = 0; i < a.Length; i++)
            max = Math.Max(max, Math.Abs(a[i] - b[i]));          // Math.Max propagates NaN
        return max;
    }

    static double MaxDiff(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            return double.PositiveInfinity;
        double max = 0;
        for (var i = 0; i < a.Length; i++)
            max = Math.Max(max, Math.Abs((double)a[i] - b[i]));
        return max;
    }

    static void ExpectThrows<T>(string name, Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        catch (Exception ex)
        {
            Expect(name, false, $"threw {ex.GetType().Name} instead of {typeof(T).Name}");
            return;
        }

        Expect(name, false, "no exception");
    }

    static void ExpectNoThrow(string name, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Expect(name, false, $"threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok)
            return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
