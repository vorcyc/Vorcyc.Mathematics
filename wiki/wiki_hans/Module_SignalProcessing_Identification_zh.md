当前位置 : [首页](HOME_zh)/[信号处理模块](Module_SignalProcessing_zh)/[系统辨识](Module_SignalProcessing_Identification_zh)

# 信号处理模块
## 系统辨识（0.11.0）

`Vorcyc.Mathematics.SignalProcessing.Identification` 从一对输入、输出记录估计单输入单输出的离散模型。非参数谱仍在 [常用操作](Module_SignalProcessing_Operations_zh#19-谱估计0110)：`TfEstimate` 是 `Pxy / Pxx`，下面的 `Tfest` 是输出误差传递函数。

> 以下类型均位于命名空间：`Vorcyc.Mathematics.SignalProcessing.Identification`

---

:ledger: 目录
- :bookmark: [IdData](#iddata)
- :bookmark: [多项式模型](#多项式模型)
- :bookmark: [状态空间模型](#状态空间模型)
- :bookmark: [SystemIdentification](#systemidentification)

---

## IdData

一次辨识会话包含输出、可选的等长输入，以及大于零的采样时间。

- `public IdData(float[] output, float[]? input, float sampleTime)`
- `public static IdData TimeSeries(float[] output, float sampleTime = 1f)` — 只有输出。
- `public static IdData FromInputOutput(float[] input, float[] output, float sampleTime = 1f)`
- `Output`、`Input`、`SampleTime`、`Samples`

多项式估计需要输入。只有输出的会话用来对已经得到的模型做仿真和残差。

## 多项式模型

`PolynomialModel` 表示 `A y = B/F u + C/D e`。

`A = [1, a1, …]` 表示 `y + a1 y(t-1) + …`。`F` 和 `D` 用同一套左侧约定。`B` 和 `C` 是右侧系数。`Delay` 是输入滞后 `nk`。

- `public static PolynomialModel Arx(IdData data, int na, int nb, int nk = 1)`
- `public static PolynomialModel Armax(IdData data, int na, int nb, int nc, int nk = 1)` — 扩展最小二乘，8 次迭代。
- `public static PolynomialModel Oe(IdData data, int nb, int nf, int nk = 1)` — 先用 ARX 播种，再做 12 次输出误差迭代，保留仿真误差最小的一次。
- `public static PolynomialModel Bj(IdData data, int nb, int nc, int nd, int nf, int nk = 1)` — 先估计输出误差对象，再对仿真残差做 ARMA，得到 `C` 和 `D`。
- `public float[] Simulate(IdData data)` — 无噪声仿真。分母在 `F` 长度大于 1 时用 `F`，否则用 `A`。开头 `max(阶数, 延迟)` 个测量输出被复制为初值。

`Kind`、`A`、`B`、`C`、`D`、`F`、`Delay` 是估计结果。这四个估计器在 `SystemIdentification` 上同样可用。

## 状态空间模型

`StateSpaceModel` 是 `x(t+1) = A x + B u`，`y = C x + D u`。

- `public StateSpaceModel(float[,] a, float[,] b, float[,] c, float[,] d)`
- `public int Order`
- `public static StateSpaceModel N4Sid(IdData data, int order)` — 子空间估计。记录长度至少要有 `4 * max(order + 1, 2 * order)`。谱半径大于 0.98 时，`A` 按 `0.98 / ρ` 缩放。
- `public float[] Simulate(IdData data)` — 初态由该次会话的测量输出估计，然后再施加输入。

## SystemIdentification

- `Arx`、`Armax`、`Oe`、`Bj`、`N4Sid` — 上面这些估计器。
- `public static StateSpaceModel Ssest(IdData data, int order)` — 默认留下 N4SID。若 `nf = order`、`nk = 0` 的输出误差模型 `Compare` 更高，则改用那个模型，并转成可控标准型；延迟写成分子前导零。
- `public static TransferFunction Tfest(IdData data, int poles, int zeros, int delay = 0)` — 输出误差模型，`nb = zeros + 1`，`nf = poles`。`delay` 默认是 0，一点延迟要显式传入。分子是 `b0 + b1 z^{-1} + …`，每个延迟样本对应一个前导零。
- `public static float[] Sim(IIdentifiedModel model, IdData data)`
- `public static float[] Resid(IIdentifiedModel model, IdData data)` — 测量输出减去仿真。
- `public static double Compare(IIdentifiedModel model, IdData data)` — `100 * (1 - ||y - ŷ|| / ||y - mean(y)||)`。

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
