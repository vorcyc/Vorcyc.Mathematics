using System.Reflection;
using System.Runtime.ExceptionServices;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

namespace SP_module_test;

/// <summary>
/// 0.11.1, the findings of the review of the adaptive filters that were left open after the first round:
/// (1) <c>mu</c>, <c>eps</c>, <c>leakage</c> and the other numeric arguments accepted NaN, infinity or values that make the update diverge on the first sample,
/// (2) the sizes of the matrices and lattices were calculated in <c>int</c> and wrapped around for large orders,
/// (3) one NaN or infinite sample (an input or a desired sample) poisoned the weights and the state of nearly every algorithm for good,
/// (4) <c>eps = 0</c> let the step of the complex NLMS and of the NSS-LMS overflow for a regressor with a tiny power,
/// (5) the sliding-window RLS retried its refresh at every sample after a failure, GNGD adapted its regularization from a stale previous step,
/// (6) <c>ComplexAdaptiveFilter.Weights</c> and <c>MultichannelNlmsFilter.GetWeights</c> were live views of the taps.
/// </summary>
internal static class AdaptiveRobust_011_test
{
    // samples of an identification run (a multiple of every block size used), the sample where the plant changes and the tail of the run where the filter
    // has to be back at the error of the clean run: a filter that the non-finite samples (all of them before the change) left dead or frozen cannot follow the change
    const int N = 6000;
    const int Change = 3000;
    const int Tail = 1000;

    static readonly float[] Plant = [0.5f, -0.3f, 0.2f, 0.1f, -0.05f, 0.03f, 0.02f, -0.01f];
    static readonly float[] Plant2 = [-0.4f, 0.5f, 0.1f, -0.2f, 0.15f, -0.05f, 0.02f, 0.01f];

    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing adaptive filter arguments, sizes and non-finite samples (0.11.1)...");

        Run(ConstructorArgumentsAreValidated);
        Run(VariableStepAndFxLmsArguments);
        Run(SizesThatOverflowAreRejected);
        Run(NonFiniteSamplesAdaptiveFilters);
        Run(NonFiniteSamplesComplex);
        Run(NonFiniteSamplesMultichannel);
        Run(NonFiniteSamplesFxLms);
        Run(NonFiniteSamplesFrequencyDomain);
        Run(NonFiniteSamplesLattices);
        Run(NonFiniteSamplesNotchCancellerEnhancer);
        Run(ZeroEpsDoesNotOverflow);
        Run(SlidingWindowRlsRefreshIsNotRetriedEverySample);
        Run(GngdIgnoresTheFrozenGap);
        Run(WeightsAreCopies);
        Run(InitRejectsNonFiniteWeights);
        Run(OcNlmsKeepsItsMeansForANonFiniteInput);

        if (_failures != 0)
            throw new InvalidOperationException($"Adaptive filters 0.11.1 (robustness): {_failures} assertion(s) failed.");

        Console.WriteLine("AdaptiveRobust_011_test: PASS");
    }

    // an exception is a failure of that group, the other groups still run
    static void Run(Action group)
    {
        try
        {
            group();
        }
        catch (Exception ex)
        {
            Expect(group.Method.Name, false, $"threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    // ---------------------------------------------------------------- arguments

    static void ConstructorArgumentsAreValidated()
    {
        var ns = typeof(LmsFilter).Namespace;
        var types = typeof(LmsFilter).Assembly.GetTypes()
            .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && t.Namespace == ns)
            .OrderBy(t => t.Name)
            .ToArray();
        Expect("the adaptive classes are found", types.Length >= 35, $"{types.Length} classes");

        var tested = 0;
        foreach (var type in types)
        foreach (var ctor in type.GetConstructors())
        {
            var parameters = ctor.GetParameters();

            // FxLmsFilter (a span) and the line enhancer around a given filter are checked on their own
            if (parameters.Any(p => p.ParameterType.IsByRefLike || typeof(AdaptiveFilter).IsAssignableFrom(p.ParameterType)))
                continue;

            var label = $"{type.Name}({string.Join(", ", parameters.Select(p => p.Name))})";

            var valid = new object?[parameters.Length];
            var known = true;
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!TryValidArgument(parameters[i], out valid[i]))
                {
                    Expect($"{label}: a valid value of '{parameters[i].Name}' is known to the test", false);
                    known = false;
                }
            }
            if (!known)
                continue;

            var constructs = true;
            try
            {
                Construct(ctor, valid);
            }
            catch (Exception ex)
            {
                Expect($"{label}: valid arguments", false, $"threw {ex.GetType().Name}: {ex.Message}");
                constructs = false;
            }
            if (!constructs)
                continue;

            tested++;
            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                var nullable = Nullable.GetUnderlyingType(p.ParameterType);
                var numeric = nullable ?? p.ParameterType;
                if (numeric != typeof(float) && numeric != typeof(double))
                    continue;

                // NaN and the infinities are no values of any numeric argument
                var bad = new List<double> { double.NaN, double.PositiveInfinity, double.NegativeInfinity };
                if (nullable is null && (p.Name is "mu" or "muLattice" or "muLadder" or "quadraticMu"))
                    bad.AddRange([0.0, -1.0]);
                if (p.Name == "leakage")
                    bad.AddRange([-1.0, 1e9]);      // leakage · mu above 1 turns the decay of the weights into a sign flip that grows
                if (p.Name == "eps")
                    bad.Add(-1.0);

                foreach (var value in bad)
                {
                    var args = (object?[])valid.Clone();
                    args[i] = numeric == typeof(float) ? (object)(float)value : value;     // a boxed double for a float parameter would be rejected by the reflection itself
                    ExpectThrows<ArgumentException>($"{label}: {p.Name} = {value}", () => Construct(ctor, args));
                }
            }
        }
        Expect("the constructors are exercised", tested >= 34, $"{tested} constructors");

        // what is valid stays valid
        ExpectNoThrow("LMS leakage · mu = 0.5", () => new LmsFilter(8, 0.5f, 1f));
        ExpectNoThrow("LMS leakage · mu = 1", () => new LmsFilter(8, 0.5f, 2f));
        ExpectNoThrow("NLMS eps = 0", () => new NlmsFilter(8, 0.5f, 0f));
        ExpectNoThrow("NLMS with leakage", () => new NlmsFilter(8, 1f, 1f, 0.5f));
        ExpectNoThrow("VSS-LMS with explicit bounds", () => new VssLmsFilter(8, 0.01f, 0.9f, 1e-3f, 0.001f, 0.05f, 0.5f));
        ExpectNoThrow("GASS-LMS with explicit bounds", () => new GassLmsFilter(8, GassVariant.Ang, 0.01f, 1e-3f, 0.9f, 0.001f, 0.05f, 0.5f));
        ExpectNoThrow("complex NLMS eps = 0", () => new ComplexNlmsFilter(4, 0.5f, 0f));
    }

    static bool TryValidArgument(ParameterInfo p, out object? value)
    {
        value = p.Name switch
        {
            "order" => 8,
            "stages" => 4,
            "channels" => 2,
            "window" => 32,
            "blockSize" => 16,
            "quadraticOrder" => 3,
            "harmonics" => 2,
            "frequency" => 0.1,
            _ => null,
        };
        if (value is not null)
            return true;
        if (!p.HasDefaultValue)
            return false;

        value = p.DefaultValue is null or DBNull ? null
              : p.ParameterType.IsEnum ? Enum.ToObject(p.ParameterType, p.DefaultValue)
              : p.DefaultValue;
        return true;
    }

    static object Construct(ConstructorInfo ctor, object?[] args)
    {
        try
        {
            return ctor.Invoke(args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    static void VariableStepAndFxLmsArguments()
    {
        foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f, -1f })
        {
            var steps = Enumerable.Repeat(0.01f, 8).ToArray();
            steps[3] = bad;
            ExpectThrows<ArgumentException>($"variable step LMS: mu[3] = {bad}", () => new VariableStepLmsFilter(8, steps));
        }
        ExpectThrows<ArgumentException>("variable step LMS: leakage · mu above 1", () => new VariableStepLmsFilter(8, Enumerable.Repeat(0.01f, 8).ToArray(), 1e9f));
        ExpectThrows<ArgumentException>("variable step LMS: 7 steps for the order 8", () => new VariableStepLmsFilter(8, Enumerable.Repeat(0.01f, 7).ToArray()));
        ExpectNoThrow("variable step LMS: valid steps and leakage", () => new VariableStepLmsFilter(8, Enumerable.Repeat(0.01f, 8).ToArray(), 10f));

        // the steps are copied: the array of the caller cannot be changed to NaN behind the validation
        var shared = Enumerable.Repeat(0.01f, 4).ToArray();
        var filter = new VariableStepLmsFilter(4, shared);
        shared[0] = float.NaN;
        var rng = new Random(7);
        for (var i = 0; i < 100; i++)
            filter.Process((float)(rng.NextDouble() - 0.5), (float)(rng.NextDouble() - 0.5));
        Expect("variable step LMS keeps its own copy of the steps", filter.Kernel.All(float.IsFinite));

        // FxLMS
        float[] path = [0f, 0.8f, 0.3f];
        foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f, -1f })
            ExpectThrows<ArgumentException>($"FxLMS: mu = {bad}", () => new FxLmsFilter(8, path, bad));
        foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, -1f })
        {
            ExpectThrows<ArgumentException>($"FxLMS: eps = {bad}", () => new FxLmsFilter(8, path, 0.1f, true, bad));
            ExpectThrows<ArgumentException>($"FxLMS: leakage = {bad}", () => new FxLmsFilter(8, path, 0.1f, true, 1e-3f, bad));
        }
        ExpectThrows<ArgumentException>("FxLMS: leakage · mu above 1", () => new FxLmsFilter(8, path, 0.1f, true, 1e-3f, 1e9f));
        foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            ExpectThrows<ArgumentException>($"FxLMS: secondary path with {bad}", () => new FxLmsFilter(8, new[] { 0f, bad, 0.3f }));
        ExpectNoThrow("FxLMS: valid arguments", () => new FxLmsFilter(8, path, 0.1f, true, 0f, 1f));
    }

    // ---------------------------------------------------------------- sizes

    static void SizesThatOverflowAreRejected()
    {
        // every size is picked so that the product wraps around in int arithmetic (or the array could not be created), while nothing large is allocated before the check
        ExpectThrows<ArgumentException>("LMS: order int.MaxValue", () => new LmsFilter(int.MaxValue));
        ExpectThrows<ArgumentException>("complex LMS: order int.MaxValue", () => new ComplexLmsFilter(int.MaxValue));
        ExpectThrows<ArgumentException>("RLS: order 46341", () => new RlsFilter(46341));
        ExpectThrows<ArgumentException>("QRD-RLS: order 46341", () => new QrdRlsFilter(46341));
        ExpectThrows<ArgumentException>("complex RLS: order 46341", () => new ComplexRlsFilter(46341));
        ExpectThrows<ArgumentException>("sliding-window RLS: order 46341, window 46341", () => new SlidingWindowRlsFilter(46341, 46341));
        ExpectThrows<ArgumentException>("LSL: stages 46341", () => new LslLatticeFilter(46341));
        ExpectThrows<ArgumentException>("GAL: stages int.MaxValue", () => new GalLatticeFilter(int.MaxValue));
        ExpectThrows<ArgumentException>("multichannel NLMS: 65536 channels of order 32768", () => new MultichannelNlmsFilter(65536, 32768));
        ExpectThrows<ArgumentException>("Volterra NLMS: quadratic order 65536", () => new VolterraNlmsFilter(65536, 65536));
        ExpectThrows<ArgumentException>("APA: projection order 46341", () => new ApaFilter(8, 46341));
        ExpectThrows<ArgumentException>("kernel LMS: 100000 centers of order 100000", () => new KernelLmsFilter(100000, 0.5f, 1f, 0.1f, 100000));
        ExpectThrows<ArgumentException>("sinusoidal canceller: 2^30 harmonics", () => new SinusoidalInterferenceCanceller(1e-10, 1 << 30));

        ExpectNoThrow("RLS: order 256", () => new RlsFilter(256));
        ExpectNoThrow("LSL: stages 256", () => new LslLatticeFilter(256));
        ExpectNoThrow("Volterra NLMS: quadratic order 64", () => new VolterraNlmsFilter(64, 64));
    }

    // ---------------------------------------------------------------- non-finite samples

    // white noise in [-1, 1] and the output of the plant for it (the plant changes at Change)
    static (float[] X, float[] D) Data(int seed)
    {
        var rng = new Random(seed);
        var x = new float[N];
        for (var i = 0; i < N; i++)
            x[i] = (float)(rng.NextDouble() * 2 - 1);

        var d = new float[N];
        for (var i = 0; i < N; i++)
        {
            var plant = i < Change ? Plant : Plant2;
            var sum = 0f;
            for (var k = 0; k < plant.Length && k <= i; k++)
                sum += plant[k] * x[i - k];
            d[i] = sum;
        }
        return (x, d);
    }

    static void Poison(float[] x, float[] d)
    {
        x[1000] = float.NaN;
        d[1500] = float.NaN;
        x[2000] = float.PositiveInfinity;
        d[2500] = float.NegativeInfinity;
    }

    // mean square error of the last Tail samples against the clean desired signal; NaN when an output of the tail is not finite
    static double TailError(float[] y, float[] cleanDesired)
    {
        double sum = 0;
        for (var i = N - Tail; i < N; i++)
        {
            if (!float.IsFinite(y[i]))
                return double.NaN;
            var e = (double)cleanDesired[i] - y[i];
            sum += e * e;
        }
        return sum / Tail;
    }

    static double TailPower(float[] y)
    {
        double sum = 0;
        for (var i = N - Tail; i < N; i++)
        {
            if (!float.IsFinite(y[i]))
                return double.NaN;
            sum += (double)y[i] * y[i];
        }
        return sum / Tail;
    }

    // The clean run has to converge; after the non-finite samples the filter has to be as good as in the clean run (non-finite outputs are allowed
    // only while the bad sample is inside the filter, long before the tail).
    static void CheckRecovery(string name, double clean, double dirty, double limit = 0.05)
    {
        Expect($"{name}: the clean run converges", clean < limit, $"error of the tail {clean:E2}");
        Expect($"{name}: the outputs are finite again after the non-finite samples", !double.IsNaN(dirty));
        if (!double.IsNaN(dirty))
            Expect($"{name}: the error is that of the clean run after the non-finite samples", dirty <= Math.Max(3 * clean, clean + 1e-3), $"clean {clean:E2}, with non-finite samples {dirty:E2}");
    }

    static void NonFiniteSamplesAdaptiveFilters()
    {
        (string Name, Func<AdaptiveFilter> Make)[] filters =
        [
            ("LMS", () => new LmsFilter(8, 0.02f)),
            ("LMF", () => new LmfFilter(8, 0.02f)),
            ("NLMS", () => new NlmsFilter(8, 0.5f, 1e-3f)),
            ("NLMF", () => new NlmfFilter(8, 0.05f, 1e-3f)),
            ("sign-sign LMS", () => new SignLmsFilter(8, 0.002f)),
            ("sign-error LMS", () => new SignErrorLmsFilter(8, 0.002f)),
            ("sign-data LMS", () => new SignDataLmsFilter(8, 0.005f)),
            ("NSS-LMS", () => new NssLmsFilter(8, 0.05f, 1e-3f)),
            ("variable step LMS", () => new VariableStepLmsFilter(8, Enumerable.Repeat(0.02f, 8).ToArray())),
            ("VSS-LMS", () => new VssLmsFilter(8)),
            ("GASS-LMS", () => new GassLmsFilter(8)),
            ("GMCC", () => new GmccFilter(8, 0.05f)),
            ("least-lncosh", () => new LlncoshFilter(8, 0.05f)),
            ("PNLMS", () => new PnlmsFilter(8, 0.5f)),
            ("IPNLMS", () => new IpnlmsFilter(8, 0.5f)),
            ("block LMS", () => new BlockLmsFilter(8, 16, 0.05f)),
            ("GNGD", () => new GngdFilter(8, 0.5f)),
            ("Volterra NLMS", () => new VolterraNlmsFilter(8, 3, 0.3f, 0.3f)),
            ("APA", () => new ApaFilter(8, 4, 0.5f)),
            ("RLS", () => new RlsFilter(8)),
            ("QRD-RLS", () => new QrdRlsFilter(8)),
            ("sliding-window RLS", () => new SlidingWindowRlsFilter(8, 64)),
            ("OCNLMS", () => new OcNlmsFilter(8, 0.5f, 1e-2f)),
        ];

        var (x, d) = Data(1);
        var xd = (float[])x.Clone();
        var dd = (float[])d.Clone();
        Poison(xd, dd);

        double Run(string name, Func<AdaptiveFilter> make, float[] input, float[] desired)
        {
            var filter = make();
            var y = new float[N];
            var weightsFinite = true;
            for (var i = 0; i < N; i++)
            {
                y[i] = filter.Process(input[i], desired[i]);

                // the weights are not touched by a non-finite sample, not even while it is inside the filter
                if (i is >= 1000 and < 1010 or >= 1500 and < 1510 or >= 2000 and < 2010 or >= 2500 and < 2510)
                    weightsFinite &= filter.Kernel.All(float.IsFinite);
            }
            Expect($"{name}: the weights stay finite while a non-finite sample is inside the filter", weightsFinite);
            return TailError(y, d);
        }

        foreach (var (name, make) in filters)
            CheckRecovery(name, Run(name, make, x, d), Run(name, make, xd, dd));
    }

    static void NonFiniteSamplesComplex()
    {
        ComplexFp32[] plant = [new(0.5f, 0.2f), new(-0.3f, 0.1f), new(0.2f, -0.1f), new(0.1f, 0.05f)];
        ComplexFp32[] plant2 = [new(-0.4f, 0.1f), new(0.5f, -0.2f), new(0.1f, 0.1f), new(-0.2f, 0.05f)];

        var rng = new Random(2);
        var x = new ComplexFp32[N];
        for (var i = 0; i < N; i++)
            x[i] = new ComplexFp32((float)(rng.NextDouble() * 2 - 1), (float)(rng.NextDouble() * 2 - 1));

        var d = new ComplexFp32[N];
        for (var i = 0; i < N; i++)
        {
            var h0 = i < Change ? plant : plant2;
            float re = 0, im = 0;
            for (var k = 0; k < h0.Length && k <= i; k++)
            {
                var h = h0[k];
                var v = x[i - k];
                re += h.Real * v.Real - h.Imaginary * v.Imaginary;
                im += h.Real * v.Imaginary + h.Imaginary * v.Real;
            }
            d[i] = new ComplexFp32(re, im);
        }

        var xd = (ComplexFp32[])x.Clone();
        var dd = (ComplexFp32[])d.Clone();
        xd[1000] = new ComplexFp32(float.NaN, 0);
        dd[1500] = new ComplexFp32(0, float.NaN);
        xd[2000] = new ComplexFp32(float.PositiveInfinity, 0);
        dd[2500] = new ComplexFp32(float.NegativeInfinity, 0);

        double Run(Func<ComplexAdaptiveFilter> make, ComplexFp32[] input, ComplexFp32[] desired)
        {
            var y = new ComplexFp32[N];
            make().ProcessAll(input, desired, y);

            double sum = 0;
            for (var i = N - Tail; i < N; i++)
            {
                if (!float.IsFinite(y[i].Real) || !float.IsFinite(y[i].Imaginary))
                    return double.NaN;
                double er = d[i].Real - y[i].Real, ei = d[i].Imaginary - y[i].Imaginary;
                sum += er * er + ei * ei;
            }
            return sum / Tail;
        }

        (string Name, Func<ComplexAdaptiveFilter> Make)[] filters =
        [
            ("complex LMS", () => new ComplexLmsFilter(4, 0.02f)),
            ("complex NLMS", () => new ComplexNlmsFilter(4, 0.5f)),
            ("complex RLS", () => new ComplexRlsFilter(4)),
        ];
        foreach (var (name, make) in filters)
            CheckRecovery(name, Run(make, x, d), Run(make, xd, dd));
    }

    static void NonFiniteSamplesMultichannel()
    {
        float[][] plant = [[0.5f, -0.3f, 0.2f, 0.1f], [0.3f, 0.2f, -0.1f, 0.05f]];
        float[][] plant2 = [[-0.4f, 0.5f, 0.1f, -0.2f], [0.2f, -0.1f, 0.3f, 0.05f]];

        var rng = new Random(3);
        var x = new float[2][];
        for (var c = 0; c < 2; c++)
        {
            x[c] = new float[N];
            for (var i = 0; i < N; i++)
                x[c][i] = (float)(rng.NextDouble() * 2 - 1);
        }

        var d = new float[N];
        for (var i = 0; i < N; i++)
        {
            var h = i < Change ? plant : plant2;
            var sum = 0f;
            for (var c = 0; c < 2; c++)
                for (var k = 0; k < h[c].Length && k <= i; k++)
                    sum += h[c][k] * x[c][i - k];
            d[i] = sum;
        }

        var xd = new[] { (float[])x[0].Clone(), (float[])x[1].Clone() };
        var dd = (float[])d.Clone();
        xd[0][1000] = float.NaN;
        dd[1500] = float.NaN;
        xd[1][2000] = float.PositiveInfinity;
        dd[2500] = float.NegativeInfinity;

        double Run(float[][] input, float[] desired)
        {
            var y = new float[N];
            new MultichannelNlmsFilter(2, 4, 0.5f).ProcessAll(input, desired, y);
            return TailError(y, d);
        }

        CheckRecovery("multichannel NLMS", Run(x, d), Run(xd, dd));
    }

    static void NonFiniteSamplesFxLms()
    {
        float[] primary = [0f, 0.5f, -0.3f, 0.2f];
        float[] primary2 = [0f, -0.4f, 0.5f, 0.1f];
        float[] secondary = [0f, 0.8f, 0.3f];

        // the control loop: the error sensor hears the disturbance minus the controller output that went through the secondary path
        double Run(bool dirty)
        {
            var rng = new Random(4);
            var filter = new FxLmsFilter(16, secondary, 0.1f);
            var x = new float[N];
            var y = new float[N];
            double sum = 0;
            var finite = true;
            for (var n = 0; n < N; n++)
            {
                x[n] = (float)(rng.NextDouble() * 2 - 1);

                var path = n < Change ? primary : primary2;
                var disturbance = 0f;
                for (var k = 0; k < path.Length && k <= n; k++)
                    disturbance += path[k] * x[n - k];

                var anti = 0f;
                for (var j = 0; j < secondary.Length && j <= n; j++)
                    anti += secondary[j] * y[n - j];
                var error = disturbance - anti;

                // glitches of the sensors: the reference sensor and the error sensor
                var reference = dirty && n == 1000 ? float.NaN : x[n];
                var measured = dirty && n == 2500 ? float.PositiveInfinity : error;

                y[n] = filter.Process(reference, measured);

                if (n >= N - Tail)
                {
                    finite &= float.IsFinite(error);
                    sum += (double)error * error;
                }
            }
            return finite ? sum / Tail : double.NaN;
        }

        CheckRecovery("FxLMS", Run(false), Run(true));
    }

    static void NonFiniteSamplesFrequencyDomain()
    {
        var (x, d) = Data(5);
        var xd = (float[])x.Clone();
        var dd = (float[])d.Clone();
        Poison(xd, dd);

        double Run(float[] input, float[] desired)
        {
            var y = new float[N];
            new FrequencyDomainAdaptiveFilter(16, 2, 0.5f).ProcessAll(input, desired, y);
            return TailError(y, d);
        }

        CheckRecovery("frequency-domain adaptive filter", Run(x, d), Run(xd, dd));
    }

    static void NonFiniteSamplesLattices()
    {
        var (x, d) = Data(6);
        var xd = (float[])x.Clone();
        var dd = (float[])d.Clone();
        Poison(xd, dd);

        double RunGal(float[] input, float[] desired)
        {
            var filter = new GalLatticeFilter(8, 0.005f);
            var y = new float[N];
            for (var i = 0; i < N; i++)
                y[i] = filter.Process(input[i], desired[i]);
            return TailError(y, d);
        }

        double RunLsl(float[] input, float[] desired, out float[] kernel)
        {
            var filter = new LslLatticeFilter(8);
            var y = new float[N];
            for (var i = 0; i < N; i++)
                y[i] = filter.Process(input[i], desired[i]);
            kernel = filter.GetKernel();
            return TailError(y, d);
        }

        CheckRecovery("GAL lattice", RunGal(x, d), RunGal(xd, dd));

        // With a NaN state the LSL returned the desired sample itself (a prediction error of zero), which no error measure of its output can tell from a perfect filter: the kernel can.
        var lslClean = RunLsl(x, d, out var kernelClean);
        var lslDirty = RunLsl(xd, dd, out var kernelDirty);
        CheckRecovery("LSL lattice", lslClean, lslDirty);
        foreach (var (kind, kernel) in new[] { ("clean run", kernelClean), ("non-finite samples", kernelDirty) })
        {
            var worst = Math.Abs(kernel[8]);
            for (var k = 0; k < 8; k++)
                worst = Math.Max(worst, Math.Abs(kernel[k] - Plant2[k]));
            Expect($"LSL lattice: the kernel is the plant after the {kind}", worst < 1e-3, $"worst tap off by {worst:E2}");
        }
    }

    static void NonFiniteSamplesNotchCancellerEnhancer()
    {
        // a sinusoid with a little noise: the output of the notch / canceller is the noise, the line enhancer's residual is the noise as well
        float[] Signal(double frequency, double noise)
        {
            var rng = new Random(8);
            var s = new float[N];
            for (var i = 0; i < N; i++)
                s[i] = (float)(Math.Sin(2 * Math.PI * frequency * i + 0.3) + noise * (rng.NextDouble() * 2 - 1));
            return s;
        }

        void Glitch(float[] s)
        {
            s[1000] = float.NaN;
            s[2000] = float.PositiveInfinity;
            s[2500] = float.NegativeInfinity;
        }

        var notch = Signal(0.2, 0.05);
        var notchDirty = (float[])notch.Clone();
        Glitch(notchDirty);
        double RunNotch(float[] input)
        {
            var filter = new AdaptiveNotchFilter(0.18, 0.95, 0.01);
            var y = new float[N];
            filter.ProcessAll(input, y);
            return TailPower(y);
        }
        CheckRecovery("adaptive notch filter", RunNotch(notch), RunNotch(notchDirty));

        var hum = Signal(0.05, 0.1);
        var humDirty = (float[])hum.Clone();
        Glitch(humDirty);
        double RunCanceller(float[] input)
        {
            var filter = new SinusoidalInterferenceCanceller(0.05, 1, 0.02);
            var y = new float[N];
            filter.ProcessAll(input, y);
            return TailPower(y);
        }
        CheckRecovery("sinusoidal interference canceller", RunCanceller(hum), RunCanceller(humDirty));

        var line = Signal(0.1, 0.5);
        var lineDirty = (float[])line.Clone();
        Glitch(lineDirty);
        double RunEnhancer(float[] input)
        {
            var filter = new AdaptiveLineEnhancer(16, 1, 0.05f);
            var enhanced = new float[N];
            var residual = new float[N];
            filter.ProcessAll(input, enhanced, residual);
            return TailPower(residual);
        }
        CheckRecovery("adaptive line enhancer", RunEnhancer(line), RunEnhancer(lineDirty), 0.5);
    }

    // ---------------------------------------------------------------- eps = 0

    static void ZeroEpsDoesNotOverflow()
    {
        var rng = new Random(9);

        // The power of the regressor is about 3e-40 (1e-20) and 3e-80 (1e-40, denormal samples): too small for the step mu / power in single precision.
        // In the second case even the change of a tap that this step would make does not fit into single precision: the sample has to be skipped.
        foreach (var amplitude in new[] { 1e-20, 1e-40 })
        {
            var complex = new ComplexNlmsFilter(4, 0.5f, 0f);
            var finite = true;
            for (var i = 0; i < 200; i++)
            {
                var input = new ComplexFp32((float)((rng.NextDouble() * 2 - 1) * amplitude), (float)((rng.NextDouble() * 2 - 1) * amplitude));
                var desired = new ComplexFp32((float)(rng.NextDouble() * 2 - 1), (float)(rng.NextDouble() * 2 - 1));
                var y = complex.Process(input, desired);
                finite &= float.IsFinite(y.Real) && float.IsFinite(y.Imaginary);
            }
            Expect($"complex NLMS, eps = 0, a regressor of the amplitude {amplitude}: the taps stay finite", finite && complex.Weights.ToArray().All(w => float.IsFinite(w.Real) && float.IsFinite(w.Imaginary)));
        }

        // 8 · (1e-22)² is a denormal number: mu / power overflows
        var nss = new NssLmsFilter(8, 0.1f, 0f);
        var nssFinite = true;
        for (var i = 0; i < 200; i++)
        {
            var y = nss.Process((float)(rng.NextDouble() * 2e-22 - 1e-22), (float)(rng.NextDouble() * 2 - 1));
            nssFinite &= float.IsFinite(y);
        }
        Expect("NSS-LMS, eps = 0, a regressor of tiny power: the weights stay finite", nssFinite && nss.Kernel.All(float.IsFinite));
    }

    // ---------------------------------------------------------------- sliding-window RLS, GNGD

    static void SlidingWindowRlsRefreshIsNotRetriedEverySample()
    {
        const int order = 4, window = 16, interval = 8;
        var rng = new Random(10);
        var f = new SlidingWindowRlsFilter(order, window, 1e-3f, interval);
        for (var i = 0; i < 40; i++)
            f.Process((float)(rng.NextDouble() * 2 - 1), (float)(rng.NextDouble() * 2 - 1));

        var type = typeof(SlidingWindowRlsFilter);
        var regressors = (float[])type.GetField("_windowRegressors", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(f)!;
        var head = (int)type.GetField("_head", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(f)!;

        // a window that cannot be solved (the newest entry stays inside it for the next 15 samples): every refresh fails
        regressors[((head + window - 1) % window) * order] = float.NaN;
        for (var i = 0; i < 12; i++)
            f.Process((float)(rng.NextDouble() * 2 - 1), (float)(rng.NextDouble() * 2 - 1));

        var since = (long)type.GetField("_sinceRefresh", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(f)!;
        Expect("sliding-window RLS: a failed refresh is not retried at every sample", since < interval, $"{since} samples since the last attempt, the interval is {interval}");
    }

    static void GngdIgnoresTheFrozenGap()
    {
        var rng = new Random(11);
        var x = new float[400];
        var d = new float[400];
        for (var i = 0; i < x.Length; i++)
            x[i] = (float)(rng.NextDouble() * 2 - 1);
        for (var i = 0; i < d.Length; i++)
        {
            var sum = 0f;
            for (var k = 0; k < Plant.Length && k <= i; k++)
                sum += Plant[k] * x[i - k];
            d[i] = sum + 0.3f * (float)(rng.NextDouble() * 2 - 1);      // the noise keeps the error, and so the adaptation of epsilon, alive
        }

        var f = new GngdFilter(8, 0.5f, 5f, 0.5f);
        for (var i = 0; i < 300; i++)
            f.Process(x[i], d[i]);

        var before = f.Epsilon;
        f.Process(x[300]);                  // a frozen sample: the next regressor is not the successor of the regressor of the last adapted sample
        f.Process(x[301], d[301]);
        Expect("GNGD: epsilon is not adapted across a frozen sample", f.Epsilon == before, $"epsilon {before} -> {f.Epsilon}");

        // the control: adapted samples one after the other do adapt it
        var control = f.Epsilon;
        f.Process(x[302], d[302]);
        Expect("GNGD: epsilon is adapted from consecutive adapted samples", f.Epsilon != control, $"epsilon {control} -> {f.Epsilon}");
    }

    // ---------------------------------------------------------------- copies

    static void WeightsAreCopies()
    {
        var complex = new ComplexLmsFilter(2, 0.1f);
        var snapshot = complex.Weights;
        var first = snapshot[0];
        for (var i = 0; i < 20; i++)
            complex.Process(new ComplexFp32(1, 0.5f), new ComplexFp32(1, 1));

        Expect("complex Weights is a copy of the taps", snapshot[0].Real == first.Real && snapshot[0].Imaginary == first.Imaginary);
        Expect("the taps moved on (the check above is not trivial)", complex.Weights[0].Real != first.Real);

        var multi = new MultichannelNlmsFilter(2, 3, 0.5f);
        var weights = multi.GetWeights(1);
        var before = weights[0];
        for (var i = 0; i < 20; i++)
            multi.Process(new float[] { 1f, 0.5f }, 1f);

        Expect("multichannel GetWeights is a copy of the taps", weights[0] == before);
        Expect("the multichannel taps moved on (the check above is not trivial)", multi.GetWeights(1)[0] != before);
    }

    // ---------------------------------------------------------------- Init with weights that are not finite

    static void InitRejectsNonFiniteWeights()
    {
        // NaN taps make every output NaN, so every error is skipped: the filter would never adapt (or recover) and nothing would tell the caller
        foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            var weights = Enumerable.Repeat(0.25f, 8).ToArray();
            weights[3] = bad;

            foreach (var (name, make) in new (string, Func<AdaptiveFilter>)[]
            {
                ("LMS", () => new LmsFilter(8)),
                ("RLS", () => new RlsFilter(8)),
                ("sliding-window RLS", () => new SlidingWindowRlsFilter(8, 32)),
            })
            {
                var filter = make();
                var before = filter.Kernel;
                ExpectThrows<ArgumentException>($"{name}: Init with {bad}", () => filter.Init(weights));
                Expect($"{name}: Init with {bad}: the taps are untouched", filter.Kernel.SequenceEqual(before));
            }

            var complex = new ComplexLmsFilter(2);
            ExpectThrows<ArgumentException>($"complex LMS: Init with {bad} (real part)", () => complex.Init([new ComplexFp32(1, 1), new ComplexFp32(bad, 0)]));
            ExpectThrows<ArgumentException>($"complex LMS: Init with {bad} (imaginary part)", () => complex.Init([new ComplexFp32(1, bad), new ComplexFp32(0, 0)]));
            Expect($"complex LMS: Init with {bad}: the taps are untouched", complex.Weights.All(w => w.Real == 0 && w.Imaginary == 0));

            var frequency = new FrequencyDomainAdaptiveFilter(16, 2);
            var frequencyWeights = new float[32];
            frequencyWeights[20] = bad;
            ExpectThrows<ArgumentException>($"frequency-domain adaptive filter: Init with {bad}", () => frequency.Init(frequencyWeights));
            Expect($"frequency-domain adaptive filter: Init with {bad}: the taps are untouched", frequency.Kernel.All(v => v == 0));

            var fx = new FxLmsFilter(8, [0f, 0.8f, 0.3f]);
            ExpectThrows<ArgumentException>($"FxLMS: Init with {bad}", () => fx.Init(weights));
            Expect($"FxLMS: Init with {bad}: the taps are untouched", fx.Kernel.All(v => v == 0));

            var multi = new MultichannelNlmsFilter(2, 4);
            var multiWeights = new float[8];
            multiWeights[5] = bad;
            ExpectThrows<ArgumentException>($"multichannel NLMS: Init with {bad}", () => multi.Init(multiWeights));
            Expect($"multichannel NLMS: Init with {bad}: the taps are untouched", multi.GetWeights(1).All(v => v == 0));

            var volterra = new VolterraNlmsFilter(4, 3);
            var quadratic = new float[6];
            quadratic[2] = bad;
            ExpectThrows<ArgumentException>($"Volterra NLMS: InitQuadratic with {bad}", () => volterra.InitQuadratic(quadratic));
            Expect($"Volterra NLMS: InitQuadratic with {bad}: the coefficients are untouched", volterra.QuadraticKernel.All(v => v == 0));
        }

        // finite weights are what they were
        var lms = new LmsFilter(4);
        lms.Init([1f, 2f, 3f, 4f]);
        Expect("LMS: Init with finite weights", lms.Kernel.SequenceEqual(new[] { 1f, 2f, 3f, 4f }));
        var cx = new ComplexLmsFilter(2);
        cx.Init([new ComplexFp32(1, 2), new ComplexFp32(3, 4)]);
        Expect("complex LMS: Init with finite weights", cx.Weights[1].Real == 3 && cx.Weights[1].Imaginary == 4);
        var fd = new FrequencyDomainAdaptiveFilter(8, 1);
        fd.Init([1f, 0f, 0f, 0f, 0f, 0f, 0f, 2f]);
        Expect("frequency-domain adaptive filter: Init with finite weights", Math.Abs(fd.Kernel[7] - 2f) < 1e-5f);
        var mc = new MultichannelNlmsFilter(2, 3);
        mc.Init([1f, 2f, 3f, 4f, 5f, 6f]);
        Expect("multichannel NLMS: Init with finite weights", mc.GetWeights(1).SequenceEqual(new[] { 4f, 5f, 6f }));
        var vo = new VolterraNlmsFilter(3, 2);
        vo.InitQuadratic([1f, 2f, 3f]);
        Expect("Volterra NLMS: InitQuadratic with finite coefficients", vo.QuadraticKernel.SequenceEqual(new[] { 1f, 2f, 3f }));
        var fxOk = new FxLmsFilter(2, [1f]);
        fxOk.Init([5f, 6f]);
        Expect("FxLMS: Init with finite weights", fxOk.Kernel.SequenceEqual(new[] { 5f, 6f }));
    }

    // ---------------------------------------------------------------- OCNLMS with a non-finite input and a large offset

    static void OcNlmsKeepsItsMeansForANonFiniteInput()
    {
        // The filter exists for signals with a large offset. A non-finite input is replaced by zero in the running sum of the inputs, which biases every mean
        // whose window holds it by offset / memory for the next <memory> samples (and once more when it leaves): the centered regressor and the mean of the target
        // no longer fit each other and the error is that of a bias of 10 for an offset of 1000.
        const int order = 4, memory = 100, glitch = 1000, length = 1400;
        float[] plant = [0.5f, -0.3f, 0.2f, 0.1f];

        var rng = new Random(12);
        var x = new float[length];
        for (var i = 0; i < length; i++)
            x[i] = 1000f + (float)(rng.NextDouble() * 2 - 1);
        var d = new float[length];
        for (var i = 0; i < length; i++)
        {
            var sum = 0f;
            for (var k = 0; k < plant.Length && k <= i; k++)
                sum += plant[k] * x[i - k];
            d[i] = sum;
        }

        // the worst error from the moment the glitch has left the delay line to the moment it has left the running sums, and after that
        (double During, double After) Run(bool dirty)
        {
            var filter = new OcNlmsFilter(order, 0.5f, 1e-3f, memory);
            double during = 0, after = 0;
            for (var i = 0; i < length; i++)
            {
                var y = filter.Process(dirty && i == glitch ? float.NaN : x[i], d[i]);
                var error = Math.Abs(d[i] - y);
                if (i >= glitch + order && i < glitch + memory + order)
                    during = Math.Max(during, error);
                else if (i >= glitch + memory + order)
                    after = Math.Max(after, error);
            }
            return (during, after);
        }

        var clean = Run(false);
        var withGlitch = Run(true);
        Expect("OCNLMS with an offset: the clean run converges", clean.During < 0.01 && clean.After < 0.01, $"worst error {clean.During:E2} / {clean.After:E2}");

        // What remains is the unknown value of the lost sample: about the noise (0.6) over memory (100) times the sum of the taps, against 5 and 0.13 with a zero for it
        Expect("OCNLMS with an offset: a non-finite input does not bias the means while it is inside them", withGlitch.During < 0.03, $"worst error {withGlitch.During:E2} (clean {clean.During:E2})");
        Expect("OCNLMS with an offset: ... nor when it leaves them", withGlitch.After < 0.02, $"worst error {withGlitch.After:E2} (clean {clean.After:E2})");
    }

    // ---------------------------------------------------------------- helpers

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

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok)
            return;
        _failures++;
        Console.WriteLine($"  FAIL {name}{(detail is null ? "" : ": " + detail)}");
    }
}
