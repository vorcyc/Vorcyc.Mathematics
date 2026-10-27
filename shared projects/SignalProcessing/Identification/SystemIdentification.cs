using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Identification;

/// <summary>
/// System-identification entry points: polynomial and state-space estimators, plus simulation, residuals, and fit.
/// </summary>
public static class SystemIdentification
{
    public static PolynomialModel Arx(IdData data, int na, int nb, int nk = 1)
        => PolynomialModel.Arx(data, na, nb, nk);

    public static PolynomialModel Armax(IdData data, int na, int nb, int nc, int nk = 1)
        => PolynomialModel.Armax(data, na, nb, nc, nk);

    public static PolynomialModel Oe(IdData data, int nb, int nf, int nk = 1)
        => PolynomialModel.Oe(data, nb, nf, nk);

    public static PolynomialModel Bj(IdData data, int nb, int nc, int nd, int nf, int nk = 1)
        => PolynomialModel.Bj(data, nb, nc, nd, nf, nk);

    public static StateSpaceModel N4Sid(IdData data, int order)
        => StateSpaceModel.N4Sid(data, order);

    /// <summary>
    /// Prediction-error state-space estimate. Starts from <see cref="N4Sid"/> and keeps an output-error polynomial realization when that simulates better.
    /// </summary>
    public static StateSpaceModel Ssest(IdData data, int order)
    {
        var subspace = N4Sid(data, order);
        var outputError = Oe(data, nb: Math.Max(order, 1), nf: order, nk: 0);
        if (Compare(outputError, data) > Compare(subspace, data))
            return StateSpaceModel.FromPlant(outputError.B, outputError.F, outputError.Delay);
        return subspace;
    }

    /// <summary>
    /// Discrete transfer function with <paramref name="poles"/> poles and <paramref name="zeros"/> zeros, estimated as an output-error model.
    /// </summary>
    public static TransferFunction Tfest(IdData data, int poles, int zeros, int delay = 0)
    {
        if (poles < 1 || zeros < 0 || delay < 0)
            throw new ArgumentOutOfRangeException(nameof(poles));
        var model = Oe(data, nb: zeros + 1, nf: poles, nk: delay);
        var numerator = new float[delay + model.B.Length];
        model.B.CopyTo(numerator, delay);
        return new TransferFunction(numerator, (float[])model.F.Clone());
    }

    public static float[] Sim(IIdentifiedModel model, IdData data)
    {
        ArgumentNullException.ThrowIfNull(model);
        return model.Simulate(data);
    }

    public static float[] Resid(IIdentifiedModel model, IdData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        float[] simulated = Sim(model, data);
        var residuals = new float[data.Output.Length];
        for (int i = 0; i < residuals.Length; i++)
            residuals[i] = data.Output[i] - simulated[i];
        return residuals;
    }

    /// <summary>
    /// Percent fit <c>100 * (1 - ||y - ŷ|| / ||y - mean(y)||)</c> on the simulated output.
    /// </summary>
    public static double Compare(IIdentifiedModel model, IdData data)
    {
        float[] residual = Resid(model, data);
        double mean = 0;
        for (int i = 0; i < data.Output.Length; i++)
            mean += data.Output[i];
        mean /= data.Output.Length;

        double error = 0;
        double scale = 0;
        for (int i = 0; i < data.Output.Length; i++)
        {
            error += residual[i] * (double)residual[i];
            double centered = data.Output[i] - mean;
            scale += centered * centered;
        }

        if (scale == 0)
            return error == 0 ? 100 : 0;
        return 100.0 * (1.0 - Math.Sqrt(error / scale));
    }
}
