当前位置 : [根目录](README.md)/[FFTW 扩展](Module_Extensions_FFTW.md)

# FFTW 扩展 - FFTW Extension

`Vorcyc.Mathematics.Extensions.FFTW` 是一个独立的 NuGet 包，提供基于 FFTW（Fastest Fourier Transform in the West）本地库的 .NET 封装。该扩展依赖于主库 `Vorcyc.Mathematics`，并利用其 `PinnableArray<T>`、`ComplexFp32`、`Complex<T>` 等类型实现高效的内存固定与复数运算。

> 程序集名称：Vorcyc.Mathematics.Extensions.FFTW  
> 命名空间：Vorcyc.Mathematics.Extensions.FFTW  
> 依赖：Vorcyc.Mathematics（主库）+ FFTW 本地 DLL（随包分发于 `runtimes/win-x64/native`）

---

:ledger:目录  
- :bookmark: [单精度 DFT](#单精度-dft)
  - :bookmark: [Dft1D — 一维变换](#dft1d--一维变换)
  - :bookmark: [Dft2D — 二维变换](#dft2d--二维变换)
  - :bookmark: [Dft3D — 三维变换](#dft3d--三维变换)
  - :bookmark: [DftND — N 维变换](#dftnd--n-维变换)
  - :bookmark: [RealToRealTransforms — 实数到实数变换](#realtorealtransforms--实数到实数变换单精度)
  - :bookmark: [Utilities — 工具方法](#utilities--工具方法单精度)
- :bookmark: [双精度 DFT](#双精度-dft)
  - :bookmark: [Dft1D — 一维变换（双精度）](#dft1d--一维变换双精度)
  - :bookmark: [Dft2D — 二维变换（双精度）](#dft2d--二维变换双精度)
  - :bookmark: [Dft3D — 三维变换（双精度）](#dft3d--三维变换双精度)
  - :bookmark: [DftND — N 维变换（双精度）](#dftnd--n-维变换双精度)
  - :bookmark: [RealToRealTransforms — 实数到实数变换（双精度）](#realtorealtransforms--实数到实数变换双精度)
  - :bookmark: [Utilities — 工具方法（双精度）](#utilities--工具方法双精度)
- :bookmark: [HilbertTransform 类](#hilberttransform-类)
- :bookmark: [EnvelopeComputer 类](#envelopecomputer-类)
- :bookmark: [SignalExtension 类](#signalextension-类)
- :bookmark: [PinnableArraySignalExtension 类](#pinnablearraysignalextension-类)

---

## 单精度 DFT

以下类均为静态类（`partial`），位于命名空间 `Vorcyc.Mathematics.Extensions.FFTW`。输入/输出使用 `ComplexFp32`（单精度复数）或 `float`。

### Dft1D — 一维变换

提供一维离散傅里叶变换的静态方法，封装 FFTW 单精度接口。

| 方法 | 说明 |
|------|------|
| `Forward(...)` | 复数→复数正向 DFT（非原地） |
| `Inverse(...)` | 复数→复数逆向 DFT（非原地） |
| `ForwardInPlace(...)` | 复数→复数正向 DFT（原地） |
| `InverseInPlace(...)` | 复数→复数逆向 DFT（原地） |
| `Forward(ReadOnlySpan<float>, ...)` | 实数→复数 (R2C) 正向 DFT |
| `Inverse(..., Span<float>, ...)` | 复数→实数 (C2R) 逆向 DFT |

每个方法均提供 `PinnableArray<T>`、`Span<T>` 和裸指针三种重载。

> **注意**：FFTW 默认不执行归一化，逆变换通常需按 `1/N` 缩放。C2R 重载提供可选的 `scale` 参数。

---

### Dft2D — 二维变换

提供二维复数到复数 (C2C) 的正向与逆向 DFT。

| 方法 | 说明 |
|------|------|
| `Forward(input, output, nx, ny, ...)` | 二维 C2C 正向 DFT |
| `Inverse(input, output, nx, ny, ...)` | 二维 C2C 逆向 DFT |

---

### Dft3D — 三维变换

提供三维复数到复数 (C2C) 的正向与逆向 DFT。

| 方法 | 说明 |
|------|------|
| `Forward(input, output, nx, ny, nz, ...)` | 三维 C2C 正向 DFT |
| `Inverse(input, output, nx, ny, nz, ...)` | 三维 C2C 逆向 DFT |

---

### DftND — N 维变换

提供任意维度的 C2C、R2C 和 C2R 变换。

| 方法 | 说明 |
|------|------|
| `Forward(input, output, dims, ...)` | N 维 C2C 正向 DFT |
| `Inverse(input, output, dims, ...)` | N 维 C2C 逆向 DFT |
| `Forward(ReadOnlySpan<float>, ..., dims)` | N 维 R2C 正向 DFT |
| `Inverse(..., Span<float>, dims, ...)` | N 维 C2R 逆向 DFT |

---

### RealToRealTransforms — 实数到实数变换（单精度）

基于 FFTW `fftw_kind` 的实数到实数 (R2R) 变换，支持 1D / 2D / 3D / N-D。

| 方法 | 说明 |
|------|------|
| `R2HC1D(...)` | 一维实数到半复数谱 |
| `HC2R1D(...)` | 一维半复数谱到实数 |
| `DHT1D(...)` | 一维离散哈特利变换 |
| `R2R_2D(...)` | 二维 R2R 变换 |
| `R2R_3D(...)` | 三维 R2R 变换 |
| `R2R_ND(...)` | N 维 R2R 变换 |

---

### Utilities — 工具方法（单精度）

| 方法 | 说明 |
|------|------|
| `EstimateFlops(...)` | 估算 1D C2C DFT 的浮点运算量 (adds, muls, fmas) |
| `PrintPlan(...)` | 打印 FFTW 计划的可读描述 |

---

## 双精度 DFT

与单精度版本结构完全一致，使用 `Complex<double>` 或 `double` 类型。所有类均为 `partial`，与单精度类共享类名（通过 partial 分文件组织）。

### Dft1D — 一维变换（双精度）

双精度版本的一维 C2C、R2C、C2R 变换，API 与单精度版本对称。

### Dft2D — 二维变换（双精度）

双精度版本的二维 C2C 变换。

### Dft3D — 三维变换（双精度）

双精度版本的三维 C2C 变换。

### DftND — N 维变换（双精度）

双精度版本的任意维度 C2C、R2C、C2R 变换。

### RealToRealTransforms — 实数到实数变换（双精度）

双精度版本的 R2R 变换（1D / 2D / 3D / N-D），API 与单精度版本对称。

### Utilities — 工具方法（双精度）

双精度版本的 FLOPs 估算与计划打印工具。

---

## HilbertTransform 类

提供基于 FFT 的解析信号构造方法来计算实值信号的希尔伯特变换。常用于生成解析信号、提取瞬时相位与幅值或执行包络检测。

| 方法 | 说明 |
|------|------|
| `ComputeHilbertTransform(ReadOnlySpan<float>)` | 计算单精度实值信号的希尔伯特变换 |
| `ComputeHilbertTransform(ReadOnlySpan<double>)` | 计算双精度实值信号的希尔伯特变换 |

---

## EnvelopeComputer 类

提供使用希尔伯特变换和峰值检测来计算信号包络的方法。

### LowerEnvelopeMode 枚举

| 值 | 说明 |
|----|------|
| `Symmetric` | 下包络为上包络的取反（假设信号围绕零对称振荡） |
| `Independent` | 独立计算下包络（先取反信号再做希尔伯特变换） |

| 方法 | 说明 |
|------|------|
| `ComputeHilbertEnvelope(...)` | 计算信号的上/下包络 |

---

## SignalExtension 类

为实验性信号类型（`Signal`、`SignalSegment`、`ModifiableTimeDomainSignal`）提供基于 FFTW 的频域变换扩展方法。由于 FFTW 支持任意长度的 DFT，不需要将信号长度补零到 2 的幂。

| 扩展目标 | 方法 | 说明 |
|----------|------|------|
| `Signal` | `TransformToFrequencyDomainFFTW(WindowType?)` | 将信号变换到频域，可选窗函数 |
| `SignalSegment` | `TransformToFrequencyDomainFFTW(WindowType?)` | 将信号段变换到频域 |
| `ModifiableTimeDomainSignal` | `TransformToFrequencyDomainFFTW(WindowType?)` | 将可修改信号变换到频域 |

---

## PinnableArraySignalExtension 类

为 `PinnableArray<float>` 和 `PinnableArray<double>` 提供信号/频谱相关的扩展方法。

| 扩展目标 | 方法 | 说明 |
|----------|------|------|
| `PinnableArray<float>` | `ScaleInPlace()` | 就地按 `1/N` 缩放（用于逆 FFT 归一化） |
| `PinnableArray<double>` | `ScaleInPlace()` | 就地按 `1/N` 缩放（双精度版本） |

---
