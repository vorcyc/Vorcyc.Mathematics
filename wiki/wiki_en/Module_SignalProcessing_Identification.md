Location: [Home](HOME) / [Signal processing](Module_SignalProcessing) / [System identification](Module_SignalProcessing_Identification)

# Signal processing module
## System identification (0.11.0)

`Vorcyc.Mathematics.SignalProcessing.Identification` estimates single-input, single-output discrete models from an input and an output record. Nonparametric spectra stay on [Operation](Module_SignalProcessing_Operations#19-spectral-estimates-0110): `TfEstimate` is `Pxy / Pxx`, and `Tfest` below is a parametric output-error transfer function.

> All types below are in namespace: `Vorcyc.Mathematics.SignalProcessing.Identification`

---

:ledger: Contents
- :bookmark: [IdData](#iddata)
- :bookmark: [Polynomial models](#polynomial-models)
- :bookmark: [State-space models](#state-space-models)
- :bookmark: [SystemIdentification](#systemidentification)

---

## IdData

A session holds the output, an optional input of the same length, and a sample time greater than zero.

- `public IdData(float[] output, float[]? input, float sampleTime)`
- `public static IdData TimeSeries(float[] output, float sampleTime = 1f)` — output only.
- `public static IdData FromInputOutput(float[] input, float[] output, float sampleTime = 1f)`
- `Output`, `Input`, `SampleTime`, `Samples`

Polynomial estimators need an input. A time-series session is for simulation and residual checks of a model that already exists.

## Polynomial models

`PolynomialModel` is the plant `A y = B/F u + C/D e`.

`A = [1, a1, …]` means `y + a1 y(t-1) + …`. The same left-side convention is used for `F` and `D`. `B` and `C` are the usual right-side coefficients. `Delay` is the input lag `nk`.

- `public static PolynomialModel Arx(IdData data, int na, int nb, int nk = 1)`
- `public static PolynomialModel Armax(IdData data, int na, int nb, int nc, int nk = 1)` — extended least squares, 8 iterations.
- `public static PolynomialModel Oe(IdData data, int nb, int nf, int nk = 1)` — seeded from ARX, then 12 output-error iterations; the lowest simulation error is kept.
- `public static PolynomialModel Bj(IdData data, int nb, int nc, int nd, int nf, int nk = 1)` — output-error plant, then an ARMA fit of the simulation residual for `C` and `D`.
- `public float[] Simulate(IdData data)` — noise-free. The denominator is `F` when it has more than one coefficient, otherwise `A`. The first `max(order, delay)` measured outputs are copied as the initial condition.

`Kind`, `A`, `B`, `C`, `D`, `F`, and `Delay` are the estimated pieces. The same four estimators are also on `SystemIdentification`.

## State-space models

`StateSpaceModel` is `x(t+1) = A x + B u`, `y = C x + D u`.

- `public StateSpaceModel(float[,] a, float[,] b, float[,] c, float[,] d)`
- `public int Order`
- `public static StateSpaceModel N4Sid(IdData data, int order)` — subspace estimate. The record must be at least `4 * max(order + 1, 2 * order)` samples. A spectral radius above 0.98 scales `A` by `0.98 / ρ`.
- `public float[] Simulate(IdData data)` — the initial state is estimated from the measured output of that session, then the input is applied.

## SystemIdentification

- `Arx`, `Armax`, `Oe`, `Bj`, `N4Sid` — the estimators above.
- `public static StateSpaceModel Ssest(IdData data, int order)` — keeps the N4SID model unless an output-error model with `nf = order` and `nk = 0` has a higher `Compare`. The output-error winner is returned in controllable canonical form, with the delay as leading zeros in the numerator.
- `public static TransferFunction Tfest(IdData data, int poles, int zeros, int delay = 0)` — output-error model with `nb = zeros + 1` and `nf = poles`. `delay` defaults to 0, so a one-sample delay has to be passed in. The numerator is `b0 + b1 z^{-1} + …`, with a leading zero for each delay sample.
- `public static float[] Sim(IIdentifiedModel model, IdData data)`
- `public static float[] Resid(IIdentifiedModel model, IdData data)` — measured output minus the simulation.
- `public static double Compare(IIdentifiedModel model, IdData data)` — `100 * (1 - ||y - ŷ|| / ||y - mean(y)||)`.

```csharp
using Vorcyc.Mathematics.SignalProcessing.Identification;

var data = IdData.FromInputOutput(input, output, sampleTime: 1f);
PolynomialModel arx = SystemIdentification.Arx(data, na: 1, nb: 1, nk: 1);
double fit = SystemIdentification.Compare(arx, data);
float[] residual = SystemIdentification.Resid(arx, data);

var tf = SystemIdentification.Tfest(data, poles: 1, zeros: 0, delay: 1);
var ss = SystemIdentification.N4Sid(data, order: 2);
float[] simulated = SystemIdentification.Sim(ss, data);
```
