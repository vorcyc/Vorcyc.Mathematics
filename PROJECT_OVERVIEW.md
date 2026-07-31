# Vorcyc.Mathematics v0.10.8 —— 我做了一套面向 .NET 10 的纯 CPU 高性能数学库

*作者第一人称手记*

---

## 一、为什么做这件事

我一直想要一套"装上 NuGet 包就能跑、不挑机器、不装原生驱动、但性能足够顶"的 .NET 数学库。市面上的方案要么偏 GPU、要么偏科研脚本、要么功能不全。所以我做了 `Vorcyc.Mathematics`，目前版本 0.10.8，目标框架 `net10.0`，依赖只有一条 `System.Numerics.Tensors` 10.0.9。

三条主线目标：

1. **把 .NET 最新特性榨干**——SIMD 串行/并行、`Span<T>`/`Memory<T>`、托管指针、内存池、泛型数学 `INumber`/`IFloatingPointIeee754<T>`。
2. **补齐 .NET 内建数学的缺口**——`ComplexFp32`、扩展数值类型、曲线拟合、模态分解等。
3. **提供额外的数学算法与运算**——DSP、ML、DL、统计、微积分、线性代数。

---

## 二、ComputingContext —— 贯穿全库的 CPU 执行策略

这是我最得意的一块基础设施。我不想让"是否并行"成为全局可变状态，所以设计成**轻量、不可变、按调用/按作用域**生效的策略对象。

### 核心类型（`shared projects/Framework/ComputingContext/`）

- `CpuExecutionMode` 枚举：`Normal`（标量）/ `Simd`（`System.Numerics.Vector<T>` 硬件 SIMD）/ `Parallel`（多线程）/ `Auto`（按问题规模启发式）。
- `ComputingContext`：不可变策略，带 `CpuMode` 与可选 `MaxParallelism`。
- `ComputingScope`：基于 `AsyncLocal<ComputingContext?>` 的作用域策略。
- `ComputingContextExecution`：共享的并行阈值与 worker 数量辅助方法。

### 解析顺序（关键）

```
显式 context 参数  >  ComputingContext.Current (Scope)  >  Default
```

`context: null` 表示"未指定"，沿这条链回溯——**不是**强制标量。这点我特意在文档里强调，因为很多人会误以为 `null` 等于 `Normal`。

### 预设与阈值

```csharp
ComputingContext.Normal    // 标量
ComputingContext.Simd      // SIMD
ComputingContext.Parallel  // 并行（大数据）
ComputingContext.Auto      // 按 problemSize 启发式
ComputingContext.Default   // 进程默认（初始为 Auto）
```

`Auto` 阈值：**≥1024 → SIMD，≥16384 → Parallel**。并行还有两道"工作量门"：`ParallelReductionThreshold = 65_536`、`ParallelMatrixMultiplyThreshold = 262_144`，避免在小数据上启动线程反而更慢。

### 四种用法

```csharp
// 1) 默认（不传参）—— 走 Default(Auto)
signal.TransformToFrequencyDomain();
values.Sum();

// 2) 单次显式
FastFourierTransform.Forward(input, output, ComputingContext.Simd);
var sum = span.Sum(ComputingContext.Parallel);
var product = Matrix.Multiply(a, b, ComputingContext.Parallel);

// 3) 作用域（一条流水线统一策略）
using (ComputingScope.Enter(ComputingContext.Parallel))
{
    var spec1 = s1.TransformToFrequencyDomain();
    var spec2 = s2.TransformToFrequencyDomain(ComputingContext.Simd); // 单次覆盖
    var mean = buffer.Average(); // 跟随 Scope=Parallel
}

// 4) 改进程默认
ComputingContext.ConfigureDefault(ComputingContext.Auto);
```

### 已接入的 API 清单

我把 `ComputingContext` 接到了几乎所有热点路径：

- **信号处理**：`ITimeDomainSignal.TransformToFrequencyDomain`、`FastFourierTransform.Forward/Inverse`、`TimeDomainCharacteristicOps.GetRms/...`、模态分解全家桶（EMD/EEMD/CEEMDAN/VMD/MVMD/MEMD/SSA/EWT/HHT，都支持 `CancellationToken` + `IProgress<ModeDecompositionProgress>`）、本轮新加的 `ZTransform.Dtft` / `LaplaceTransform.FrequencyResponse`。
- **统计**：`Basic.Sum/Average/Variance/StandardDeviation`、`Correlation.PearsonCorrelation`、`Standardization.ZScore`、`InformationTheory.Entropy/KL`、`TensorStatistics.*AlongAxis`。
- **线性代数**：`Matrix.Multiply`、`Matrix<T>.Multiply`、`VectorSpan.Dot/Sum/Norm/Add/Subtract/Axpy/Scale`、`MatrixDecomposition.SingularValueDecomposition`（Householder apply + factor combine 并行，QR 迭代仍串行但每轮 poll 取消）。
- **深度学习**：`ComputingContextExecution.ForEach` 调度批维并行；**前向 + 反向都并行**（0.9.2 起），覆盖 `BatchConv2DMath`/`BatchConv2DIm2col`/`BatchFullyConnected`/`BatchMaxPool2D`/`BatchAvgPool2D`/`BatchGlobalAveragePool2D`/`BatchUpsample2D`/`BatchSoftmax`/`BatchConcatenate`/`BatchTransposedConv2D`/`BatchDepthwiseConv2D`/`BatchLayerNorm`/`BatchSqueezeExcite`。每个反向 kernel 按样本/空间位置/输出通道切分写区，**结果与串行逐位一致**（有 `ComputingContext_equivalence_test` 兜底）。`Trainer.Fit/FitBatched/FitBatchSequential/FitBatchParallelConcat/FitCnnMlp` 都有可选 `computingContext`。
- **特征提取 / MFCC**：`FeatureExtractor.ParallelChunksComputeFrom/ParallelComputeFrom`，`MfccExtractor` 走 `ParallelComputeFrom` 或外层 `ComputingScope`。
- **机器学习**：大多数估计器构造器接受可选 `ComputingContext?` 或暴露可写 `Context`。`PredictBatch`、`GradientBoostingClassifier`（按类并行）、`OneVsRestClassifier`（按类）、`NumericRandomForest`（按树，确定性种子）、`KMeans/EM/GMM/DBSCAN/Hierarchical/PCA/LDA/FA/TSNE/ICA` 等都接入了。

---

## 三、GPU 现状：暂时移除（主动决策，不是拖延）

我在 `wiki/wiki_en/Module_GPU_Policy.md` 里写得很明确：**主 NuGet 包不含、也不计划内建 GPU/CUDA 加速**。这是主动的产品与技术路线决策，四条理由：

1. **与库定位冲突**：核心库目标是"纯 .NET、可移植、无原生驱动"。用户应该能在没有 NVIDIA GPU 的机器、服务器、CI、边缘设备上 `dotnet add package Vorcyc.Mathematics` 无摩擦使用。
2. **当前负载更适合 CPU**：表格 ML（树/KNN/Softmax）GPU 无收益；小规模自定义 DL（XOR、小 CNN、MFCC 分类）批小，PCIe 传输开销常吃掉算力收益；信号处理 CPU+SIMD/并行已经够。
3. **历史 GPU 代码不是生产质量**：`CudaFFT` 源码里曾写着"结果不对，暂时别用"。保留 ILGPU 会膨胀包体、制造"支持 GPU"的错觉、分散测试精力。
4. **GPU 训练成本太高**：要在自家 `Trainer` + 手写 `Backward` 上做完整 GPU 训练，需要 `DeviceTensor`、前向/反向 kernel 或 cuDNN 集成、CPU/GPU 数值对齐测试——接近一条独立产品线。

**已移除项**：`ILGPU`/`ILGPU.Algorithms` 依赖、`CudaFFT`/`CudaFastFourierTransform`/`CUDA_FFT` 源码、`CuFFT_test`。`ComputingContext` 只描述 CPU 模式，`ComputingBackend.GPU` 之类枚举只是占位，运行时不用。

**未来如果出现明确需求**，会按 FFTW 扩展包的模式做一个**可选扩展包** `Vorcyc.Mathematics.Extensions.*`，主包永远零 CUDA 依赖。当前版本没有任何 GPU 扩展包存在。

---

## 四、各模块详细清单

### 1. Core（`shared projects/Core/`）

基础数学与工具：`ArrayExtension`/`ArrayExtension2`/`ArrayExtension_Advanced`、`BaseConverter`、`BitMathExtension`、`Combinatorics`、`ConstantsFp32`/`Constants_g`、`ExceptionExtension`、`NumberMapper`、`SimpleRNG`、`TrigonometryHelper`、`VMath`/`VMath2`、`Buffers/`（可固定缓冲）、`Helpers/`。

### 2. Framework

`ComputingContext` 全家桶 + `Guard`、`FilterAttribute`/`FilterAttribute2`、`Direction`、`DataHelper/`、`Utilities/`。

### 3. DeepLearning（0.9）

- **容器**：`BatchSequential`（批处理）、`Sequential`（单样本）、`CnnMlpModel`（CNN+MLP 复合）。
- **张量**：`BatchTensor`（NHWC 布局）、`TensorUtilities`。
- **层（20+）**：`BatchConvolution2D`/`BatchConv2DMath`/`BatchConv2DIm2col`/`BatchDepthwiseConvolution2D`/`BatchTransposedConvolution2D`/`BatchFullyConnected`/`BatchMaxPool2D`/`BatchAvgPool2D`/`BatchGlobalAveragePool2D`/`BatchUpsample2D`/`BatchSoftmax`/`BatchConcatenate`/`BatchFlatten`/`BatchLayerNorm`/`BatchBatchNorm`/`BatchSqueezeExcite`/`BatchDropout`、激活 `BatchReLU`/`BatchSigmoid` 等；单样本层 `FullyConnected`/`SigmoidActivation`/`Conv2D`/`MaxPool2D`/`ReLU`/`BatchNorm`/`JoinLayer`/`Fusion`。
- **训练**：`Trainer.Fit/FitBatched/FitBatchSequential/FitBatchParallelConcat/FitCnnMlp`、`IBatchLoss`、`MeanSquaredErrorLoss` 等、`SgdOptimizer`/`AdamOptimizer`、`TrainingSample`/`BatchTrainingSample`、`MlpTrainingOptions`。
- **音频前端**：`BatchPreEmphasis`/`BatchStftMagnitude`/`BatchMelFilterbank`。
- **序列化**：`ModelSerializer` v3（架构元数据 + 权重）。

### 4. LinearAlgebra

`Matrix`/`Matrix<T>`、`VectorSpan`、`MatrixDecomposition.SingularValueDecomposition` 等。

### 5. MachineLearning（0.9）

- **分类/回归**：`LogisticRegression`/`SoftmaxRegression`/`GaussianNaiveBayes`/`OneVsRestClassifier`/`KnnClassifier`/`KnnRegressor`/`MultipleLinearRegression`/`MultivariateRidgeRegression`/`NumericDecisionTree`/`NumericRandomForest`/`GradientBoostingClassifier`/`AdaBoostClassifier`/`SupportVectorMachine`。
- **流水线**：`ClassificationPipeline`/`RegressionPipeline`。
- **聚类/降维/分离**：`KMeansClusterer`/`ExpectationMaximization`/`GMM`/`DBSCAN`/`HierarchicalClustering`/`VectorQuantization`/`PCA`/`LinearDiscriminantAnalysis`/`FactorAnalysis`/`TSNE`/`Decomposition.ICA`。
- **工具**：`ClassificationMetrics.ConfusionMatrix`、`ModelBatchExtensions.PredictBatch`、`CurveFitter<T>` 全家桶。

### 6. SignalProcessing（最重的一块）

- **Signals**：0.9 起以 `Signal`/`SignalSegment` 为时域主 API（`float` 采样率、零拷贝切片、`ReadOnlySpan` 热路径），`DiscreteSignal` 保留但标 obsolete；`ComplexDiscreteSignal` 及泛型 `_g` 版本；`ModifiableTimeDomainSignal`；`SignalGeneratingExtension`（`GenerateWave` Sine/Square/Triangle/Sawtooth/Noise）、`SignalResamplingExtension`；`FrequencyDomain`/`IFrequencyDomain`/`ITimeDomainCharacteristics`。
- **Fourier**：`FastFourierTransform`/`FastFourierTransformNormal`/`FastFourierTransformParallel`/`FastFourierTransformSIMD`、统一入口 `Fft`/`Fft64`、`RealFft`/`RealFft64`、`FftButterflyFp32`/`FftButterflyFp64`、`Stft`。
- **Filters**：经典原型 `Butterworth`/`ChebyshevI`/`ChebyshevII`/`Elliptic`/`Bessel`/`BiQuad`/`OnePole`/`Polyphase`；自适应 `Adaptive/`；`Kalman/`；`Fda/`（`DesignFilter.IirLpTf` 等）；以及 `MedianFilter`/`MovingAverageFilter`/`SavitzkyGolayFilter`/`WienerFilter`/`HilbertFilter`/`ThiranFilter`/`CombFeedback`/`CombFeedforward`/`DcRemoval`/`DeEmphasis`/`PreEmphasis`/`RastaFilter`。
- **Transforms**：`CepstralTransform`/`Goertzel`/`HartleyTransform`/`HilbertTransform`/`MellinTransform`/`Dct/`/`Wavelets/`/`Base/`；**本轮新增** `LaplaceTransform`/`AnalogDigitalTransform`/重写的 `ZTransform`；**模态分解** `ModeDecomposition/`（EMD/EEMD/CEEMDAN/VMD/MVMD/MEMD/HHT/EWT/SSA + facade `ModeDecomposer`，全部支持 `CancellationToken`+`IProgress<ModeDecompositionProgress>`）。
- **FeatureExtractors / Features**：`MfccExtractor`（含 HTK 预设 `MfccHtkOptions`）、`MelExtractor`、`LpcExtractor` 等。
- **Operations / Effects / Windowing**：信号运算、音效、窗函数族。
- 顶层文件：`FractionalDelayLine`/`InterpolationMode`/`Lpc`/`Scale`/`Scale_g`。

### 7. Numerics

扩展整数、浮点、`ComplexFp32`/`Complex<T>`、有理数/分数等高精度类型与互转。

### 8. Statistics

`Basic`（Sum/Average/Variance/StandardDeviation）、`Correlation`、`Standardization`、`InformationTheory`、`IComparableExtension`、`INumberExtension`、`Distribution`、`TimeSeriesAnalysis`、`ExtremeValueFinder`、`TensorStatistics`。

### 9. Calculus

极限、积分、导数、Taylor 级数、Fourier 级数、Runge-Kutta ODE 求解、Newton-Raphson 求根。

### 扩展包

`Vorcyc.Mathematics.Extensions.FFTW`：FFTW 原生库封装，支持 1D/2D/3D/N-D 的 C2C/R2C/C2R/R2R，单/双精度，含 FFT-based Hilbert 变换与包络。

---

## 五、本轮未提交的增量（v0.10.8 重点：Z / Laplace / 模数转换）

`git status` 显示的改动是我刚加完还没提交的一组，落在 `shared projects/SignalProcessing/Transforms/`：

### 1. `LaplaceTransform.cs`（新增，323 行）

连续时间 Laplace 域工具，针对有理 H(s)，系数升幂约定（`b0 + b1·s + b2·s² …`）。

- `Evaluate(num, den, s)`：Horner 法算 H(s)。
- `FrequencyResponse(num, den, ω, context?)`：H(jω) 频率响应，三路派发（并行扫 ω / ω-lane SIMD Horner / 标量）。SIMD 核心用 `(-accI·ω, accR·ω)` 这条复乘 `jω` 的捷径。
- `IsStable(poles, eps)`：模拟 BIBO 稳定判据，所有极点实部严格 < 0。
- `BilinearMap` / `BilinearMapInPlace`：s→z 双线性 `(1+s)/(1−s)`，可选 `tan(πf_c)` 预扭曲；空零点按 `DesignFilter.IirLpTf` 约定补 `z=−1`。
- `InverseBilinearMap` / `InverseBilinearMapInPlace`：z→s 反映射 `(z−1)/(z+1)`，`z≈−1` 视为模拟域无穷远零点丢弃。

### 2. `AnalogDigitalTransform.cs`（新增，54 行）

模拟原型到离散 `TransferFunction` 的桥，复用 `LaplaceTransform.BilinearMap`：

- `Bilinear(analogZeros, analogPoles, gain, fc?, normalizeAt?)`：Tustin 映射得到离散 `TransferFunction`，可选在某个归一化频率处做增益归一。
- `BilinearLowpass(fc, analogPoles, analogZeros?, gain)`：低通原型路径，等价于 `DesignFilter.IirLpTf`。
- `InverseBilinear(...)`：反方向，把离散零极点还原回模拟域。

### 3. `ZTransform.cs`（重写，+537 行）

离散时间 Z 变换，TF 系数用负幂约定（与 `Filters.Base.TransferFunction` 对齐）：

- `Dtft(input, N, context?)`：单位圆上 Z 变换，对有限长序列**精确**。三路派发：`UseParallelIndexed` → 并行标量 worker；否则显式/解析 `Simd` → 频率 lane SIMD；`Normal` → 标量。
- `Transform<T>(...)`：泛型版，`float`/`double` 走 SIMD，其它走标量。
- `Evaluate(b, a, z)`：任意复 z 处算 H(z)。
- `EvaluateFrequencyResponse(b, a, ω)`（单点）与网格版（SIMD）。
- `IsStable(poles, eps)`：离散 BIBO 判据 `|p| < 1`。
- `MaxPoleRadius(poles)`：最大极点半径。
- 老的 `GetPolesAndZeros` 标 `[Obsolete]`——从 DTFT 样本反推极零点是错的，正确路径是 `TransferFunction.Poles/Zeros`。

### 4. `ZLaplace_test.cs`（新增测试，189 行）

冒烟 + 等价性核对：

- `δ[n] → X(e^{jω}) = 1`；
- `Normal/Simd/Parallel` 三路一致（容差 1e-4）；
- 多点网格 H(e^{jω}) 与逐点 `EvaluateFrequencyResponse` 逐位相等；
- `H(s)=1/(s+1)` 的 `H(0)=1`、`H(j∞)→0`；
- 模拟/离散稳定判据；
- **关键等价性**：`AnalogDigitalTransform.BilinearLowpass` 与 `DesignFilter.IirLpTf` 在 Butterworth 4 阶原型上系数逐项一致（容差 2e-5）；
- 双线性往返：模拟极点 → 数字 → 反映射能找回原极点，`z=−1` 处零点被正确丢弃。

`SP_module_test/Program.cs` 里挂上 `ZLaplace_test.Go()`，与 EMD/VMD/HHT/ExtendedModeDecomposition 测试并列。

---

## 六、示例代码

下面这些片段都取自我在 `Examples/` 下写的可运行工程。

### ComputingContext 解析优先级（`Examples/Core_example/ComputingContextDemo.cs`）

```12:38:Examples/Core_example/ComputingContextDemo.cs
Span<float> data = stackalloc float[64];
for (var i = 0; i < data.Length; i++) data[i] = i + 1;

float explicitParallel = data.Sum(ComputingContext.Parallel);     // 显式 Parallel
Console.WriteLine($"显式 Parallel Sum: {explicitParallel:F1}");

float scoped;
using (ComputingScope.Enter(ComputingContext.Simd))
{
    scoped = data.Sum(context: null);                              // 走 Scope=Simd
}
Console.WriteLine($"Scope SIMD + context:null Sum: {scoped:F1}");

using (ComputingScope.Enter(ComputingContext.Parallel))
{
    float scopeWins = data.Sum(ComputingContext.Normal);            // 显式 Normal 优先于 Scope
    Console.WriteLine($"Scope Parallel 内显式 Normal 优先: {scopeWins:F1}");
}

Console.WriteLine($"Resolve(null) → Default: {ComputingContext.Resolve(null).CpuMode}");
Console.WriteLine($"ParallelReductionThreshold: {ComputingContextExecution.ParallelReductionThreshold:N0}");
```

### FFT + 频域分析（`Examples/SignalProcessing_example/FftDemo.cs`）

```13:40:Examples/SignalProcessing_example/FftDemo.cs
const float rate = 8000f;
const float targetHz = 440f;
const int length = 4096;

var tone = new Signal(length, rate);
tone.GenerateWave(WaveShape.Sine, targetHz, Behaviour.Replace);

var spectrum = tone.TransformToFrequencyDomain(context: null, WindowType.Hamming);
Console.WriteLine($"FFT 检测主频: {spectrum.Frequency:F1} Hz");
Console.WriteLine($"频谱质心: {spectrum.Centroid:F1} Hz");
Console.WriteLine($"频率分辨率: {spectrum.Resolution:F2} Hz/bin");

float explicitSimd = tone.TransformToFrequencyDomain(ComputingContext.Simd, WindowType.Hamming).Frequency;
Console.WriteLine($"显式 SIMD FFT 主频: {explicitSimd:F1} Hz");

using (ComputingScope.Enter(ComputingContext.Parallel))
{
    var scopedTone = new Signal(length, rate);
    scopedTone.GenerateWave(WaveShape.Sine, targetHz, Behaviour.Replace);
    float scopedFreq = scopedTone.TransformToFrequencyDomain(context: null, WindowType.Hamming).Frequency;
    Console.WriteLine($"Scope Parallel FFT 主频: {scopedFreq:F1} Hz");
}
```

### Butterworth 低通滤波（`Examples/SignalProcessing_example/FilterDemo.cs`）

```17:32:Examples/SignalProcessing_example/FilterDemo.cs
var clean = new Signal(length, rate);
clean.GenerateWave(WaveShape.Sine, toneHz, Behaviour.Replace);

var noisy = clean.Clone();
var rng = new Random(7);
var samples = noisy.Samples;
for (var i = 0; i < noisy.Length; i++)
    samples[i] += (rng.NextSingle() - 0.5f) * 0.35f;
noisy.NotifySamplesModified();

float cutoffNorm = 400f / rate;
var lowPass = new LowPassFilter(cutoffNorm, order: 4);
var filtered = lowPass.FilterOnline(noisy);
```

### MFCC 特征（HTK 预设）（`Examples/SignalProcessing_example/MfccDemo.cs`）

```17:28:Examples/SignalProcessing_example/MfccDemo.cs
var signal = new Signal(samplingRate, samplingRate);
signal.GenerateWave(WaveShape.Sine, speechHz, Behaviour.Replace);

var options = new MfccHtkOptions(
    samplingRate,
    featureCount: 13,
    frameDuration: 0.025f,
    lowFrequency: 80f,
    highFrequency: 7000f);

var extractor = new MfccExtractor(options);
var frames = extractor.ComputeFrom(signal);

Console.WriteLine($"帧数: {frames.Count}, 每帧维度: {extractor.FeatureCount}");
```

### XOR 训练（`Examples/DeepLearning_example/XorDemo.cs`）

```23:43:Examples/DeepLearning_example/XorDemo.cs
var model = new Sequential<float>(
    new FullyConnectedLayer<float>(2, 8, null, new Random(42)),
    new SigmoidActivation<float>(),
    new FullyConnectedLayer<float>(8, 1, null, new Random(42)),
    new SigmoidActivation<float>());

var trainer = new Trainer<float>();
trainer.Fit(
    model,
    new MeanSquaredErrorLoss<float>(),
    new SgdOptimizer<float>(0.5f),
    dataset,
    epochs: 5000,
    shuffle: true,
    onEpochEnd: (epoch, loss) =>
    {
        if (epoch is 1000 or 3000 or 5000)
            Console.WriteLine($"  epoch {epoch,4}: loss = {float.CreateTruncating(loss):F4}");
    });
```

### CNN + MLP（`Examples/DeepLearning_example/CnnMlpDemo.cs`）

```20:46:Examples/DeepLearning_example/CnnMlpDemo.cs
var backbone = new BatchSequential<float>(
    new BatchConvolution2DLayer<float>(1, 4, kernelSize: 3),
    new BatchReLUActivation<float>(),
    new BatchMaxPool2DLayer<float>(),
    new BatchFlattenLayer<float>());

var head = new Sequential<float>(
    new FullyConnectedLayer<float>(16, 8),
    new SigmoidActivation<float>(),
    new FullyConnectedLayer<float>(8, 1),
    new SigmoidActivation<float>());

var model = new CnnMlpModel<float>(backbone, head);
var trainer = new Trainer<float>();
trainer.FitCnnMlp(
    model,
    new MeanSquaredErrorLoss<float>(),
    new AdamOptimizer<float>(0.05f),
    [new BatchTrainingSample<float>(batch, targets)],
    epochs: 2000,
    onEpochEnd: (epoch, loss) =>
    {
        if (epoch is 1000 or 2000)
            Console.WriteLine($"  epoch {epoch,4}: loss = {float.CreateTruncating(loss):F4}");
    });
```

### 机器学习（`Examples/MachineLearning_example` 典型用法）

```csharp
using Vorcyc.Mathematics.MachineLearning;

var forest = new NumericRandomForest<double>(numTrees: 80, seed: 11)
    { Context = ComputingContext.Parallel };
forest.Fit(xTrain, yTrain);
int[] preds = forest.PredictBatch(xTest);

// 或用 Scope 统一治理多个估计器
var knn = new KnnClassifier<double>(k: 3);
knn.Fit(xTrain, yTrain);
using (ComputingScope.Enter(ComputingContext.Parallel))
{
    int[] p = knn.PredictBatch(xTest);
}
```

### 本轮新增 Z / Laplace / 模数转换（来自 `ZLaplace_test.cs` 的真实用法）

```csharp
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;

// DTFT：δ[n] → 全 1
float[] x = [1f, 0, 0, 0, 0, 0, 0, 0];
var X = ZTransform.Dtft(x, 32);

// 同一份数据，三种执行策略必须一致
var serial   = ZTransform.Dtft(longX, 512, ComputingContext.Normal);
var simd     = ZTransform.Dtft(longX, 512, ComputingContext.Simd);
var parallel = ZTransform.Dtft(longX, 512, ComputingContext.Parallel);

// Laplace 频率响应 H(s)=1/(s+1)
float[] num = [1f];
float[] den = [1f, 1f];
var H = LaplaceTransform.FrequencyResponse(num, den, [0f, 1f, 10f, 100f]);
// |H[0]|≈1, |H[^1]|≈0

// 模拟 Butterworth 4 阶 → 离散低通，与 DesignFilter.IirLpTf 等价
float fc = 0.1f;
var proto = PrototypeButterworth.Poles(4);
var viaApi    = AnalogDigitalTransform.BilinearLowpass(fc, proto);
var viaDesign = DesignFilter.IirLpTf(fc, proto);
// 两者系数逐项一致（容差 2e-5）

// 双线性往返
var (z, p) = LaplaceTransform.BilinearMap(ReadOnlySpan<ComplexFp32>.Empty, proto, fc);
var (zBack, pBack) = LaplaceTransform.InverseBilinearMap(z, p, fc);
// zBack 为空（z=−1 处零点被丢弃），pBack 找回原极点
```

---

## 七、性能视角：高性能是怎么落地的

"高性能"不是一句口号，是我用 BenchmarkDotNet 量出来的。具体落到六件事上：

### 1. SIMD 贯穿全库，不止 FFT

`System.Numerics.Vector<T>` 硬件 SIMD 不是只在 Fourier 里用，而是铺到了几乎所有热点模块。grep `Vector.IsHardwareAccelerated` 能在 `FftButterflyFp32/Fp64`、`FastFourierTransformSIMD`、`ZTransform`、`LaplaceTransform`、`Statistics/Basic_FloatingPointNumberExtension`、`LinearAlgebra/Matrix_f`、`LinearAlgebra/Tensor`、`Calculus/CalculusVectorOps`、`SignalProcessing/Windowing/WindowApplier_real_f`、`MachineLearning/CurveFitting/*`、`DeepLearning/Layers/BatchNormLayer` 等十几个文件里命中。本轮新加的 `ZTransform.DtftSimd` / `LaplaceTransform.EvalPolySimd` 也是同一套思路：把频率 ω 作为 lane 维度，用 `(-accI·ω, accR·ω)` 这条复乘 `jω` 的捷径做向量化 Horner。

### 2. 实测阈值，不是拍脑袋

`FftButterflyFp32` 里有两个带注释的常量，注释直接写明"measured"：

```19:31:shared projects/SignalProcessing/Fourier/FftButterflyFp32.cs
/// <summary>
/// Minimum transform length before the parallel path is used, even when explicitly requested.
/// Below this the threading overhead dominates (measured: parallel is a net loss until ~16K,
/// where it draws even with SIMD and pulls ahead beyond). Smaller sizes fall back to SIMD.
/// </summary>
private const int ParallelMinSize = 16384;

/// <summary>
/// Minimum transform length before the SIMD path is used, even when explicitly requested.
/// Measured: the vector path only starts winning around 8K; below that the per-stage twiddle
/// setup makes it a slight net loss versus the scalar recurrence, so we stay scalar.
/// </summary>
private const int SimdMinSize = 8192;
```

`ComputingContext.Auto` 的 1024/16384 两档、`ParallelReductionThreshold = 65_536`、`ParallelMatrixMultiplyThreshold = 262_144`，都是同一类"小数据别启动线程/向量"的实测护栏。

### 3. SoA 布局 + 连续内存，向量化不掉 gather/scatter

FFT 蝶形核用 **SoA（structure-of-arrays）**——实部 `re[]`、虚部 `im[]` 分开存。注释里写得很直白：

```97:100:shared projects/SignalProcessing/Fourier/FftButterflyFp32.cs
/// <summary>
/// Runs all decimation-in-frequency stages. Each stage rewrites disjoint index ranges, so the
/// block loop is embarrassingly parallel and the inner twiddle loop is contiguous (SoA), which
/// vectorizes without gather/scatter.
/// </summary>
```

蝶形块内 i-run 和 p-run 各自连续，`new Vector<float>(re.Slice(i, w))` 直接连续加载，不需要 gather/scatter，SIMD 利用率才上得去。

### 4. 内存池 + 可固定缓冲，少分配少 GC

热点路径不 `new` 临时数组。FFT 每级 twiddle 用 `ArrayPool<float>.Shared.Rent` 借、`finally` 里 `Return`：

```113:115:shared projects/SignalProcessing/Fourier/FftButterflyFp32.cs
        int half = size >> 1;
        float[] wReBuf = ArrayPool<float>.Shared.Rent(half);
        float[] wImBuf = ArrayPool<float>.Shared.Rent(half);
```

`Core/Buffers/` 下有 `PinnableArray<T>` / `IPinnedBuffer` / `PinnableArrayOption` / `PinnableArrayExtension`，给需要固定地址的本地代码互操作（如 FFTW 扩展）提供托管指针钉住能力。参数层一律 `Span<T>`/`ReadOnlySpan<T>`，字段层 `Memory<T>`，零拷贝切片贯穿 `Signal`/`SignalSegment`。

### 5. 并行核按写区切分，与串行逐位一致

并行不是"加个 `Parallel.For` 就完"。每个反向 kernel 都按"样本/空间位置/输出通道"切分写区，保证线程间无写竞争。`ComputingContext_equivalence_test` 兜底验证：**并行路径与串行路径结果 bit-for-bit 一致**。这意味着用户可以放心切到 `Parallel`，不会因为浮点累加顺序变化而拿到不同的模型/频谱。

### 6. BenchmarkDotNet 直接对标原生 FFTW

`benchmarks/basic_benchmark/FFT_new_old_benchmark.cs` 里我把自家 FFT 和 FFTW 原生库放同一个 BenchmarkDotNet 矩阵里跑，参数从 256 一路扫到 1 048 576：

```21:73:benchmarks/basic_benchmark/FFT_new_old_benchmark.cs
    [Params(256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 63356, 1048576)]
    public int N;
    ...
    [Benchmark]
    public bool my_method() => FastFourierTransformNormal.Forward(_realArray, _complexArray);

    [Benchmark]
    public void realFFT() => _fft.Direct(_realArray, _outReal, _outImg);

    [Benchmark]
    public void realFFT_new() => _realOnly.Forward(_realArray, _complexArray);

    [Benchmark]
    public void FFTW() => Vorcyc.Mathematics.Extensions.FFTW.Dft1D.Forward(_realArray, _complexArray);
```

`benchmarks/` 下还有 `Calculus_benchmark`、`ExtremeValueFinder_benchmark`、`MachineLearning_batch_benchmark`、`SimpleLinearRegression_benchmark`、`SpanMathExtensionBenchmarks`、`Statistics_benchmark`、`Windowing_benchmark`——每个性能敏感模块都有独立基准工程。

---

## 八、全面性视角：到底"全"在哪里

"全"也不是自夸，可以数。`shared projects/` 下共 **630 个 `.cs` 文件**，按模块分布：

| 模块 | .cs 文件数 |
|------|-----------|
| SignalProcessing | 297 |
| MachineLearning | 100 |
| DeepLearning | 90 |
| Calculus | 31 |
| Statistics | 30 |
| Core | 25 |
| LinearAlgebra | 19 |
| Framework | 17 |
| Numerics | 14 |

### 1. 信号处理一个模块顶别人一个库

297 个文件，分九个子域：

- **Signals**：`Signal`/`SignalSegment`（0.9 主 API）+ `DiscreteSignal`（兼容）+ `ComplexDiscreteSignal` + 泛型 `_g` 版本 + `ModifiableTimeDomainSignal` + 生成器/重采样扩展 + 频域特性接口。
- **Fourier**：11 个变体——`FastFourierTransform` / `Normal` / `Parallel` / `SIMD` / `Fft` / `Fft64` / `RealFft` / `RealFft64` / `FftButterflyFp32` / `FftButterflyFp64` / `Stft`。
- **Filters**：10 个经典原型家族（Butterworth/ChebyshevI/ChebyshevII/Elliptic/Bessel/BiQuad/OnePole/Polyphase/Adaptive/Kalman）+ `Fda`（含 `DesignFilter.IirLpTf`、`Remez`、`VtlnWarper`）+ 15+ 特种滤波器（Median/MovingAverage/SavitzkyGolay/Wiener/Hilbert/Thiran/Comb/DcRemoval/DeEmphasis/PreEmphasis/Rasta）。
- **Transforms**：8+ 数学变换（Cepstral/Goertzel/Hartley/Hilbert/Mellin/Dct×3/Wavelets）+ **本轮新增** Laplace/AnalogDigital/Z + **9 种模态分解**（EMD/EEMD/CEEMDAN/VMD/MVMD/MEMD/HHT/EWT/SSA）+ `ModeDecomposer` facade。
- **FeatureExtractors / Features**：MFCC（含 HTK 预设）、Mel、Lpc、Pitch、Filterbank 等。
- **Operations / Effects / Windowing**：信号运算、音效（Wahwah/Robot/...）、窗函数族。

### 2. 机器学习一个模块 100 个文件、11 个子目录

`MachineLearning/` 下子目录：`Classfication` / `Clustering` / `CurveFitting` / `Decomposition` / `DimensionalityReduction` / `Distances` / `Internal` / `Preprocessing` / `Regression` / `Serialization` + 顶层 25 个文件（`AdaBoostClassifier`/`GradientBoostingClassifier`/`NumericRandomForest`/`SupportVectorMachine`/`CrossValidation`/`GridSearch`/`DataSplit`/`EvaluationMetrics`/`ClassificationPipeline`/`RegressionPipeline`/`ModelBatchExtensions`...）。

距离度量单独一个子目录：Euclidean/Manhattan/Chebyshev/Levenshtein/...。曲线拟合单独一个子目录：线性/对数/幂/多项式/...回归。降维单独一个子目录：PCA/LDA/FA/TSNE/ICA。

### 3. 深度学习 90 个文件，从张量到训练到序列化一条龙

`BatchTensor`（NHWC）+ `TensorUtilities` → 20+ 批处理层（Conv/Depthwise/TransposedConv/FC/Pool×3/Upsample/Softmax/Concat/Flatten/LayerNorm/BatchNorm/SqueezeExcite/Dropout/Residual + 激活 ReLU/Sigmoid/Tanh/LeakyReLU）→ `Trainer.Fit*` 五个入口 + `IBatchLoss` + Adam/SGD + 学习率调度 → `ModelSerializer` v3（架构目录 + 权重）→ 音频前端集成（PreEmphasis/StftMagnitude/MelFilterbank）。

### 4. 从底层数值到高层 API 的完整栈

- **底层**：`Numerics`（`ComplexFp32`/`Complex<T>`/`Rational`/`RectangleFP32` 等扩展数值类型）→ `Core`（`VMath`/`BitMath`/`Trigonometry`/`SimpleRNG`/`PinnableArray`）。
- **中层**：`LinearAlgebra`（`Matrix`/`Tensor`/SVD）、`Statistics`（`Basic`/`Correlation`/`Standardization`/`InformationTheory`/`Distribution`/`TimeSeriesAnalysis`/`ExtremeValueFinder`/`TensorStatistics`/`Divergence`）、`Calculus`（积分/导数/Taylor/Fourier 级数/RungeKutta/NewtonRaphson/LineSearch）。
- **高层**：`Trainer.FitCnnMlp`、`ModeDecomposer.Hht`、`AnalogDigitalTransform.BilinearLowpass`、`ClassificationPipeline`、`MfccExtractor`——一行调用搞定一整套流程。

### 5. 文档、示例、测试三件套

- **双语言 Wiki**：`wiki/wiki_en/`（英文，NuGet README 源）与 `wiki/wiki_hans/`（简体中文）**各自独立维护**，34 篇 `Module_*.md` 覆盖每个模块的完整 API 手册。
- **示例**：`Examples/` 下 7 个独立可运行工程（Core/SignalProcessing/DeepLearning/Audio/MachineLearning/Calculus/Colorization），不随 NuGet 发布。
- **测试**：`TESTS/` 下 6 个模块测试可执行工程（core/DL/DSP/SP/ML/Calculus），本轮新增的 `ZLaplace_test` 就挂在 `SP_module_test` 里。
- **基准**：`benchmarks/` 下 BenchmarkDotNet 工程，对标 FFTW 原生库。

### 6. 可扩展性预留

主包 CPU-only，但留了扩展包通道：`Vorcyc.Mathematics.Extensions.FFTW` 已经是范本（1D/2D/3D/N-D 的 C2C/R2C/C2R/R2R，单/双精度）。未来 GPU 若回归，会以 `Vorcyc.Mathematics.Extensions.*` 形式出现，主包永远零原生依赖。

---

## 九、工程组织

`Vorcyc.Mathematics.slnx` 用 shared project 模式：

| 路径 | 用途 |
|------|------|
| `shared projects/` | 编入 NuGet 包的共享 `.projitems` |
| `targets/dotNET10_Portable/` | 主 NuGet 工程 `Vorcyc.Mathematics` |
| `targets/dotNET10_Portable_Extensions_FFTW/` | 可选 FFTW 原生扩展 |
| `TESTS/` | 各模块可执行测试（core/DL/DSP/SP/ML/Calculus） |
| `Examples/` | 独立示例（不随 NuGet 发布） |
| `benchmarks/` | BenchmarkDotNet 基准工程 |
| `wiki/wiki_en/`、`wiki/wiki_hans/` | 中英文 Wiki，独立维护 |
| `docs/` | License、Logo、文档索引 |

---

## 十、一句话总结

**我给 .NET 10 写了一套 SIMD/并行驱动的纯 CPU 数学库，覆盖 Core、ComputingContext、Linear Algebra、Statistics、Calculus、Numerics、Signal Processing、Machine Learning、Deep Learning 九大方向；GPU（ILGPU/CudaFFT）已主动移除，未来若需要会以可选扩展包形式回归；这一轮刚把信号处理里的 Z 变换、Laplace 变换和模拟/数字双线性变换补齐，并验证了它与现有 IIR 设计通路 `DesignFilter.IirLpTf` 的逐位等价。**




