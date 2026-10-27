using Vorcyc.Mathematics.SignalProcessing.Identification;

namespace SP_module_test;

internal static class SysId_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing 0.11.0 system identification...");
        var data = ArxData();
        Polynomials(data);
        StateSpace(data);
        if (_failures != 0)
            throw new InvalidOperationException($"System identification 0.11.0: {_failures} assertion(s) failed.");
        Console.WriteLine("SysId_011_test: PASS");
    }

    static IdData ArxData()
    {
        var rng = new Random(4);
        int n = 800;
        var u = new float[n];
        var y = new float[n];
        for (int t = 0; t < n; t++)
            u[t] = (float)(rng.NextDouble() * 2 - 1);
        for (int t = 1; t < n; t++)
            y[t] = 0.5f * y[t - 1] + 0.8f * u[t - 1] + 0.01f * (float)(rng.NextDouble() * 2 - 1);
        return IdData.FromInputOutput(u, y, sampleTime: 0.1f);
    }

    static void Polynomials(IdData data)
    {
        var arx = SystemIdentification.Arx(data, na: 1, nb: 1, nk: 1);
        Expect(Math.Abs(arx.A[1] + 0.5f) < 0.08f, $"arx a {arx.A[1]}");
        Expect(Math.Abs(arx.B[0] - 0.8f) < 0.08f, $"arx b {arx.B[0]}");
        Expect(SystemIdentification.Compare(arx, data) > 90, $"arx fit {SystemIdentification.Compare(arx, data)}");
        Expect(SystemIdentification.Sim(arx, data).Length == data.Samples, "arx sim length");
        Expect(SystemIdentification.Resid(arx, data).Length == data.Samples, "arx resid length");

        var armax = SystemIdentification.Armax(data, 1, 1, 1, 1);
        Expect(Math.Abs(armax.A[1] + 0.5f) < 0.1f, $"armax a {armax.A[1]}");
        Expect(Math.Abs(armax.B[0] - 0.8f) < 0.1f, $"armax b {armax.B[0]}");

        var oe = SystemIdentification.Oe(data, nb: 1, nf: 1, nk: 1);
        Expect(Math.Abs(oe.F[1] + 0.5f) < 0.12f, $"oe f {oe.F[1]}");
        Expect(Math.Abs(oe.B[0] - 0.8f) < 0.12f, $"oe b {oe.B[0]}");

        var bj = SystemIdentification.Bj(data, nb: 1, nc: 1, nd: 1, nf: 1, nk: 1);
        Expect(SystemIdentification.Compare(bj, data) > 80, $"bj fit {SystemIdentification.Compare(bj, data)}");

        var tf = SystemIdentification.Tfest(data, poles: 1, zeros: 0, delay: 1);
        Expect(tf.Denominator.Length == 2, "tfest denominator");
        Expect(tf.Numerator.Length == 2, "tfest numerator");
        Expect(Math.Abs(tf.Denominator[1] + 0.5f) < 0.12f, $"tfest a {tf.Denominator[1]}");
        Expect(Math.Abs(tf.Numerator[1] - 0.8f) < 0.12f, $"tfest b {tf.Numerator[1]}");
    }

    static void StateSpace(IdData data)
    {
        var subspace = SystemIdentification.N4Sid(data, order: 1);
        double subspaceFit = SystemIdentification.Compare(subspace, data);
        Expect(subspaceFit > 70, $"n4sid fit {subspaceFit}");
        var pem = SystemIdentification.Ssest(data, order: 1);
        Expect(SystemIdentification.Compare(pem, data) + 1 >= subspaceFit, $"ssest fit {SystemIdentification.Compare(pem, data)}");
    }

    static void Expect(bool condition, string message)
    {
        if (condition)
            return;
        _failures++;
        Console.WriteLine("FAIL: " + message);
    }
}
