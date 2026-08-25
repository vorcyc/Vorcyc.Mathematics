# Vorcyc.Mathematics

High-performance **.NET 10** math library: SIMD/parallel CPU numerics, signal processing, deep learning, machine learning, statistics, linear algebra, and calculus.

![VMath logo](https://raw.githubusercontent.com/vorcyc/Vorcyc.Mathematics/main/docs/logos/logo1.png)

**Version:** 0.10.17 · **TFM:** `net10.0` · **NuGet:** `Vorcyc.Mathematics`

[中文](readme_zh.md)

Vorcyc® Mathematics targets **.NET 10+**, emphasizing SIMD and parallel CPU performance, accurate numerics, and modular APIs for DSP, deep learning, and classical ML.

> **Minimum target: .NET 10.0**

---

## Features

- SIMD-accelerated CPU execution
- Parallel execution via `ComputingContext` (scalar / SIMD / parallel / auto)
- Generic math (`INumber`, spans, tensors)
- **No GPU in the main package** — CPU-only; ILGPU removed (see [GPU policy](wiki/wiki_en/Module_GPU_Policy.md))

## Dependencies

- `System.Numerics.Tensors` 10.0.9

---

## Modules

**Core** — arrays, constants, RNG, trigonometry, pinnable buffers.  
→ [wiki/wiki_en/Module_Core.md](wiki/wiki_en/Module_Core.md)

**ComputingContext** — execution policy, `ComputingScope`; integrated into FFT, statistics, linear algebra, batch DL, MFCC, Mel frontends, and `Trainer.Fit*`.  
→ [wiki/wiki_en/Module_ComputingContext.md](wiki/wiki_en/Module_ComputingContext.md)

**Deep learning (0.9)** — `BatchTensor` (NHWC), `BatchSequential` / `Sequential`, 20+ batch layers, `Trainer.Fit*`, `ModelSerializer` v3.  
→ [wiki/wiki_en/Module_DeepLearning.md](wiki/wiki_en/Module_DeepLearning.md)

**Linear algebra**  
→ [wiki/wiki_en/Module_LinearAlgebra.md](wiki/wiki_en/Module_LinearAlgebra.md)

**Machine learning (0.9+)** — classifiers, regressors, pipelines, trees/forests, boosting, CV, grid search; Isolation Forest + JSON snapshots (0.10.11) & GPR `PredictStd` (0.10.10); CurveFitting SIMD large-N `StackOverflow` fix (0.10.16); classic `CurveFitter` methods take `CancellationToken` (0.10.17).  
→ [wiki/wiki_en/Module_MachineLearning.md](wiki/wiki_en/Module_MachineLearning.md)

**Signal processing** — **`Signal` / `SignalSegment`** as primary time-domain APIs; FFT (`TransformToFrequencyDomain` uses SoA `FftButterflyFp32` from 0.10.12), Welch (`AveragePeriodogram` complete windows + `ComputingContext` from 0.10.13; SciPy onesided scale / periodic windows / spectrum helpers from 0.10.14), `FrequencyDomain.IndexToFrequency` uses FFT `TransformLength` (0.10.15), pink noise via Paul Kellet (`PinkNoiseGenerator`, 0.10.14), filters, MFCC, effects.  
→ [wiki/wiki_en/Module_SignalProcessing.md](wiki/wiki_en/Module_SignalProcessing.md)

**Numerics** — extended integer/float/complex/rational types.  
→ [wiki/wiki_en/Module_Numerics.md](wiki/wiki_en/Module_Numerics.md)

**Statistics** — reductions, distributions, time series, extreme values.  
→ [wiki/wiki_en/Module_Statistics.md](wiki/wiki_en/Module_Statistics.md)

**Calculus** — integration, derivatives, ODEs, root finding, series.  
→ [wiki/wiki_en/Module_Calculus.md](wiki/wiki_en/Module_Calculus.md)

---

## Extension packages

**FFTW** (`Vorcyc.Mathematics.Extensions.FFTW`) — native FFTW bindings for high-performance DFTs.  
→ [wiki/wiki_en/Module_Extensions_FFTW.md](wiki/wiki_en/Module_Extensions_FFTW.md)

---

## Quick start

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

## Solution layout

| Path | Purpose |
|------|---------|
| `shared projects/` | Shared `.projitems` compiled into the portable package |
| `targets/dotNET10_Portable/` | Main NuGet project (`Vorcyc.Mathematics`) |
| `targets/dotNET10_Portable_Extensions_FFTW/` | Optional FFTW native extension |
| `TESTS/` | Module test executables |
| `Examples/` | Standalone sample apps (not on NuGet) |
| `benchmarks/` | BenchmarkDotNet projects |
| `wiki/wiki_en/` | English wiki (`HOME.md`; packed as NuGet README) |
| `wiki/wiki_hans/` | Simplified Chinese wiki (independent) |
| `docs/` | License, logos, documentation index |

Open `Vorcyc.Mathematics.slnx` in Visual Studio or build individual projects with `dotnet`.

---

## Examples

Standalone projects under `Examples/` (not shipped on NuGet):

| Project | Focus | Command |
|---------|--------|---------|
| `Core_example` | ComputingContext, matrix multiply, VectorSpan | `dotnet run --project Examples/Core_example` |
| `SignalProcessing_example` | Signal, FFT, MFCC, filters | `dotnet run --project Examples/SignalProcessing_example` |
| `DeepLearning_example` | Training, CNN+MLP, serialization | `dotnet run --project Examples/DeepLearning_example` |
| `Audio_example` | NAudio WAV I/O, MFCC classifier | `dotnet run --project Examples/Audio_example` |
| `MachineLearning_example` | Tabular ML pipelines | `dotnet run --project Examples/MachineLearning_example` |
| `Calculus_example` | Integration, ODE, root finding | `dotnet run --project Examples/Calculus_example` |
| `Colorization_example` | ColorNet / ChromaGAN (Emgu.CV, Windows) | `dotnet run --project Examples/Colorization_example` |

→ [wiki/wiki_en/Module_Examples.md](wiki/wiki_en/Module_Examples.md)

---

## Documentation

| Document | Role |
|----------|------|
| **README.md** (this file) | English solution overview |
| **readme_zh.md** | Chinese solution overview |
| **wiki/wiki_en/HOME.md** | English wiki home — full API manuals (NuGet README source) |
| **wiki/wiki_hans/HOME_zh.md** | Chinese wiki home — full API manuals |

The two wikis are maintained independently. This solution README is a separate entry point and does not replace the wikis.

---

## License

See [docs/License.txt](docs/License.txt).
