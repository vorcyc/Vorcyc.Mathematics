# Vorcyc.Mathematics

面向 **.NET 10** 的高性能数学库：SIMD/并行 CPU 数值计算、信号处理、深度学习、机器学习、统计、线性代数与微积分。

![VMath logo](https://raw.githubusercontent.com/vorcyc/Vorcyc.Mathematics/main/docs/logos/logo1.png)

**版本：** 0.10.10 · **目标框架：** `net10.0` · **NuGet：** `Vorcyc.Mathematics`

[English readme](README.md)

Vorcyc® Mathematics 是一套主要面向 .NET 的数学库，旨在充分利用 .NET 最新特性并提供高性能和准确度的数学函数与运算，可用 C# 在任何 .NET 应用程序中使用。

主要目标：

1. 充分利用 .NET 最新特性以获得最佳性能（SIMD 串行/并行、`Span<T>` / `Memory<T>`、托管指针与内存池等）。
2. 补充 .NET 内建常用数学函数的不足。
3. 提供额外的数学算法与运算。

> ***面向 .NET 10.0 及以上版本。***

---

## 特性

- SIMD 加速的 CPU 串行计算
- 并行计算（`ComputingContext`：标量 / SIMD / 并行 / 自动）
- 泛型数学
- **主包不含 GPU 加速** — 已移除 ILGPU 与实验性 `CudaFFT`，默认仅在 CPU 上计算  
  → [wiki/wiki_hans/Module_GPU_Policy_zh.md](wiki/wiki_hans/Module_GPU_Policy_zh.md)

## 依赖项

- `System.Numerics.Tensors` 10.0.9

---

## 模块

**核心模块** — 数组操作、常量、随机数、三角函数、可固定数组等基础运算。  
→ [wiki/wiki_hans/Module_Core_zh.md](wiki/wiki_hans/Module_Core_zh.md)

**计算上下文（ComputingContext）** — CPU 执行策略与 `ComputingScope`；已接入 FFT、统计、矩阵/向量、批 DL、MFCC 并行、Mel 音频前端与 `Trainer.Fit*`。  
→ [wiki/wiki_hans/Module_ComputingContext_zh.md](wiki/wiki_hans/Module_ComputingContext_zh.md)

**深度学习（0.9）** — `BatchTensor`（NHWC）、`BatchSequential` / `Sequential`、20+ 批处理层、`Trainer.Fit*`、**ModelSerializer v3**。  
→ [wiki/wiki_hans/Module_DeepLearning_zh.md](wiki/wiki_hans/Module_DeepLearning_zh.md)

**线性代数**  
→ [wiki/wiki_hans/Module_LinearAlgebra_zh.md](wiki/wiki_hans/Module_LinearAlgebra_zh.md)

**机器学习（0.9+）** — 分类/回归流水线、树与森林、提升、交叉验证、网格搜索、聚类与降维等；Isolation Forest 与 GPR `PredictStd`（0.10.10）。  
→ [wiki/wiki_hans/Module_MachineLearning_zh.md](wiki/wiki_hans/Module_MachineLearning_zh.md)

**信号处理** — **0.9 起以 `Signal` / `SignalSegment` 为时域主 API**；FFT、滤波、MFCC、音效等。  
→ [wiki/wiki_hans/Module_SignalProcessing_zh.md](wiki/wiki_hans/Module_SignalProcessing_zh.md)

**数值模块** — 扩展整数、浮点、复数、有理数等高精度类型。  
→ [wiki/wiki_hans/Module_Numerics_zh.md](wiki/wiki_hans/Module_Numerics_zh.md)

**统计模块** — 极值、均值/方差、分布、时间序列等。  
→ [wiki/wiki_hans/Module_Statistics_zh.md](wiki/wiki_hans/Module_Statistics_zh.md)

**微积分** — 积分、导数、ODE、求根、级数展开等。  
→ [wiki/wiki_hans/Module_Calculus_zh.md](wiki/wiki_hans/Module_Calculus_zh.md)

---

## 扩展包

**FFTW 扩展**（`Vorcyc.Mathematics.Extensions.FFTW`）— 基于 FFTW 本地库的高性能 DFT 封装。  
→ [wiki/wiki_hans/Module_Extensions_FFTW_zh.md](wiki/wiki_hans/Module_Extensions_FFTW_zh.md)

---

## 快速开始

```powershell
dotnet add package Vorcyc.Mathematics

dotnet build targets/dotNET10_Portable/dotNET10_Portable.csproj -c Release
dotnet run --project Examples/Core_example -c Release
```

```powershell
dotnet run --project TESTS/core_module_test/core_module_test.csproj -c Release
dotnet run --project TESTS/DL_module_test/DL_module_test.csproj -c Release
```

---

## 解决方案结构

| 路径 | 说明 |
|------|------|
| `shared projects/` | 编入主 NuGet 包的共享 `.projitems` |
| `targets/dotNET10_Portable/` | 主 NuGet 工程（`Vorcyc.Mathematics`） |
| `targets/dotNET10_Portable_Extensions_FFTW/` | 可选 FFTW 原生扩展 |
| `TESTS/` | 各模块测试可执行程序 |
| `Examples/` | 独立示例（不随 NuGet 发布） |
| `benchmarks/` | BenchmarkDotNet 基准工程 |
| `wiki/wiki_en/HOME.md` | 英文 Wiki 首页（独立维护） |
| `wiki/wiki_hans/HOME_zh.md` | 简体中文 Wiki 首页（独立维护） |
| `docs/` | 许可证、Logo、文档索引 |

使用 Visual Studio 打开 `Vorcyc.Mathematics.slnx`，或对单个工程执行 `dotnet build` / `dotnet run`。

---

## 示例

`Examples/` 目录提供独立可运行工程（不随 NuGet 发布）：

| 工程 | 内容 | 命令 |
|------|------|------|
| `Core_example` | ComputingContext、矩阵乘、VectorSpan | `dotnet run --project Examples/Core_example` |
| `SignalProcessing_example` | Signal、FFT、MFCC、滤波 | `dotnet run --project Examples/SignalProcessing_example` |
| `DeepLearning_example` | 训练、CNN+MLP、序列化 | `dotnet run --project Examples/DeepLearning_example` |
| `Audio_example` | NAudio WAV、MFCC 分类 | `dotnet run --project Examples/Audio_example` |
| `MachineLearning_example` | 表格 ML 流水线 | `dotnet run --project Examples/MachineLearning_example` |
| `Calculus_example` | 积分、ODE、求根 | `dotnet run --project Examples/Calculus_example` |
| `Colorization_example` | ColorNet / ChromaGAN（Emgu.CV，Windows） | `dotnet run --project Examples/Colorization_example` |

→ [wiki/wiki_hans/Module_Examples_zh.md](wiki/wiki_hans/Module_Examples_zh.md)

---

## 文档

| 文档 | 用途 |
|------|------|
| **README.md** | 英文解决方案说明 |
| **readme_zh.md**（本文件） | 中文解决方案说明 |
| **wiki/wiki_en/HOME.md** | 英文 Wiki 首页（NuGet 包 README 来源） |
| **wiki/wiki_hans/HOME_zh.md** | 中文 Wiki 首页 |

中英文 Wiki **各自独立维护**；本解决方案 README 为单独入口，不替代 Wiki 全文。

---

## 许可证

见 [docs/License.txt](docs/License.txt)。
