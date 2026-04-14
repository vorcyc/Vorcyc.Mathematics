#!/usr/bin/env python3
"""Build line-level translations and write English wiki files for Batch C."""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HANS = ROOT / "wiki_hans"
EN = ROOT / "wiki_en"
LINES_JSON = ROOT / "tools" / "chinese_lines_batch_c.json"
TRANS_JSON = ROOT / "tools" / "line_translations_batch_c.json"

MAPPINGS = [
    ("Module_MachineLearning_zh.md", "Module_MachineLearning.md"),
    ("Module_LinearAlgebra_zh.md", "Module_LinearAlgebra.md"),
    ("Module_Experimental_zh.md", "Module_Experimental.md"),
]

BREADCRUMB = {
    "机器学习模块": ("Machine learning", "Module_MachineLearning.md"),
    "线性代数模块": ("Linear algebra", "Module_LinearAlgebra.md"),
    "实验性模块": ("Experimental", "Module_Experimental.md"),
}

# Word-level fragments (longest first when applied)
FRAGMENTS = [
    ("机器学习模块", "Machine learning module"),
    ("线性代数模块", "Linear algebra module"),
    ("实验性模块", "Experimental module"),
    ("模块概览与新增 API", "Module overview and new APIs"),
    ("0.9 分类器完整示例", "0.9 classifier walkthrough"),
    ("距离度量类", "Distance metric classes"),
    ("见上文表格", "see table above"),
    ("回归流水线演示", "Regression pipeline demo"),
    ("运行完整分类演示（默认）", "Run full classification demo (default)"),
    ("控制输入为0", "zero control input"),
    ("当前测量值", "current measurement"),
    ("脱离为独立信号", "decouple to standalone signal"),
    ("定义滤波器参数", "Filter parameters"),
    ("创建平方根卡尔曼滤波器实例", "Create square-root Kalman filter"),
    ("准确率", "Accuracy"),
    ("结构体", "struct"),
    ("枚举", "enum"),
    ("接口", "interface"),
    ("类", "class"),
    ("目录", "Contents"),
    ("属性", "Property"),
    ("类型", "Type"),
    ("说明", "Description"),
    ("方法", "Method"),
    ("模型", "Model"),
    ("值", "Value"),
    ("子系统", "Subsystem"),
    ("命名空间 / 类型", "Namespace / types"),
    ("命名空间", "namespace"),
    ("获取", "Gets"),
    ("返回", "Returns"),
    ("计算", "Computes"),
    ("使用", "Uses"),
    ("判断", "Determines whether"),
    ("初始化", "Initializes"),
    ("执行", "Performs"),
    ("训练", "Trains"),
    ("预测", "Predicts"),
    ("创建", "Creates"),
    ("扩展", "Expands"),
    ("更新", "Updates"),
    ("估计", "Estimates"),
    ("添加", "Adds"),
    ("保存", "Saves"),
    ("加载", "Loads"),
    ("从文件中加载码书", "Loads codebook from file"),
    ("将码书保存到文件中", "Saves codebook to file"),
    ("参数:", "Parameters:"),
    ("返回值:", "Returns:"),
    ("异常:", "Exceptions:"),
    ("当基的维度不匹配时抛出", "Thrown when basis dimensions do not match"),
    ("当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出",
     "Thrown when A is not square or dimensions of A and b do not match"),
    ("当矩阵的维度不兼容时抛出", "Thrown when matrix dimensions are incompatible"),
    ("当输入数组的长度小于 2 或第一个系数为零时抛出",
     "Thrown when coefficient array length < 2 or first coefficient is zero"),
    ("第一个点", "First point"),
    ("第二个点", "Second point"),
    ("第一个向量", "First vector"),
    ("第二个向量", "Second vector"),
    ("第一个矩阵", "First matrix"),
    ("第二个矩阵", "Second matrix"),
    ("第一个四元数", "First quaternion"),
    ("第二个四元数", "Second quaternion"),
    ("第一个张量", "First tensor"),
    ("第二个张量", "Second tensor"),
    ("第一个二维数组", "First 2D array"),
    ("第二个二维数组", "Second 2D array"),
    ("第一个锯齿数组", "First jagged array"),
    ("第二个锯齿数组", "Second jagged array"),
    ("第一个锯齿数组矩阵", "First jagged matrix"),
    ("第二个锯齿数组矩阵", "Second jagged matrix"),
    ("行数", "row count"),
    ("列数", "column count"),
    ("矩阵", "matrix"),
    ("向量", "vector"),
    ("张量", "tensor"),
    ("四元数", "quaternion"),
    ("锯齿数组", "jagged array"),
    ("二维数组", "2D array"),
    ("标量", "scalar"),
    ("元素", "element"),
    ("维度", "dimension"),
    ("宽度", "width"),
    ("高度", "height"),
    ("深度", "depth"),
    ("转置", "transpose"),
    ("行列式", "determinant"),
    ("逆矩阵", "inverse"),
    ("单位矩阵", "identity matrix"),
    ("伴随矩阵", "companion matrix"),
    ("深拷贝", "deep copy"),
    ("聚类", "clustering"),
    ("回归", "regression"),
    ("分类", "classification"),
    ("降维", "dimensionality reduction"),
    ("滤波", "filtering"),
    ("采样", "sample"),
    ("采样率", "sample rate"),
    ("信号", "signal"),
    ("波形", "waveform"),
    ("正弦波", "sine wave"),
    ("余弦波", "cosine wave"),
    ("方波", "square wave"),
    ("锯齿波", "sawtooth wave"),
    ("三角波", "triangle wave"),
    ("白噪声", "white noise"),
    ("粉红噪声", "pink noise"),
    ("过零率", "zero-crossing rate"),
    ("均方根值", "RMS value"),
    ("功率谱密度", "power spectral density"),
    ("频域", "frequency domain"),
    ("时域", "time domain"),
    ("重采样", "resample"),
    ("窗函数", "window function"),
    ("核函数", "kernel function"),
    ("学习率", "learning rate"),
    ("迭代次数", "iterations"),
    ("收敛容差", "convergence tolerance"),
    ("混淆矩阵", "confusion matrix"),
    ("超参数", "hyperparameters"),
    ("持久化", "persistence"),
    ("码书", "codebook"),
    ("权重", "weights"),
    ("协方差矩阵", "covariance matrices"),
    ("聚类中心", "cluster centers"),
    ("因子载荷矩阵", "factor loadings matrix"),
    ("共同性数组", "communalities array"),
    ("特异性方差数组", "specific variances array"),
    ("训练误差", "training errors"),
    ("滤波结果", "filtered output"),
    ("欧几里得距离", "Euclidean distance"),
    ("曼哈顿", "Manhattan"),
    ("不相似度", "dissimilarity"),
    ("不遵守三角不等式", "violates triangle inequality"),
    ("梯度提升", "gradient boosting"),
    ("逻辑回归", "logistic regression"),
    ("随机森林", "random forest"),
    ("决策树", "decision tree"),
    ("朴素贝叶斯", "naive Bayes"),
    ("线性判别分析", "linear discriminant analysis"),
    ("网格搜索", "grid search"),
    ("交叉验证", "cross-validation"),
    ("数据划分", "data split"),
    ("评估指标", "evaluation metrics"),
    ("批量预测", "batch prediction"),
    ("全量数据", "full dataset"),
    ("最佳超参", "best hyperparameters"),
    ("后验概率", "posterior probabilities"),
    ("整数类别", "integer class labels"),
    ("二维数值特征", "2D numeric features"),
    ("整数类别标签", "integer class labels"),
    ("无公开属性", "No public properties"),
    ("重写方法", "Overrides"),
    ("索引器", "Indexers"),
    ("隐式转换", "Implicit conversion"),
    ("运算符", "operator"),
    ("加法运算符", "Addition operator"),
    ("减法运算符", "Subtraction operator"),
    ("乘法运算符", "Multiplication operator"),
    ("除法运算符", "Division operator"),
    ("相等运算符", "Equality operator"),
    ("不相等运算符", "Inequality operator"),
    ("矩阵加法", "Matrix addition"),
    ("矩阵减法", "Matrix subtraction"),
    ("矩阵乘法", "Matrix multiplication"),
    ("矩阵与标量乘法", "Matrix–scalar multiplication"),
    ("矩阵与标量除法", "Matrix–scalar division"),
    ("水平合并", "Horizontally concatenates"),
    ("垂直合并", "Vertically stacks"),
    ("广义转置", "generalized transpose"),
    ("局部加权回归", "locally weighted regression"),
    ("三次样条插值", "cubic spline interpolation"),
    ("移动平均", "moving average"),
    ("非线性回归", "nonlinear regression"),
    ("高斯过程回归", "Gaussian process regression"),
    ("神经网络回归", "neural-network regression"),
    ("贝叶斯线性回归", "Bayesian linear regression"),
    ("线性回归", "linear regression"),
    ("多项式回归", "polynomial regression"),
    ("指数回归", "exponential regression"),
    ("对数回归", "logarithmic regression"),
    ("幂回归", "power regression"),
    ("正弦回归", "sinusoidal regression"),
    ("支持向量回归", "support vector regression"),
    ("贝叶斯回归", "Bayesian regression"),
    ("均方误差", "mean squared error"),
    ("拟合参数", "fitted parameters"),
    ("预测函数", "prediction function"),
    ("分段三次多项式", "piecewise cubic polynomial"),
    ("局部线性", "local linear"),
    ("窗口平均", "window average"),
    ("自定义", "custom"),
    ("核函数", "kernel"),
    ("带先验线性", "linear with prior"),
    ("替换现有信号", "replace existing signal"),
    ("逐元素相加", "element-wise add"),
    ("逐元素相减", "element-wise subtract"),
    ("逐元素相乘", "element-wise multiply"),
    ("逐元素相除", "element-wise divide"),
    ("线程安全", "thread-safe"),
    ("延迟计算", "lazy evaluation"),
    ("只读结构体", "readonly struct"),
    ("固定缓冲区", "pinned buffer"),
    ("父信号", "parent signal"),
    ("信号段", "signal segment"),
    ("起始位置", "start index"),
    ("起始时间", "start time"),
    ("持续时间", "duration"),
    ("采样数据", "sample data"),
    ("测量数据", "measurement data"),
    ("测量值", "measurement"),
    ("控制输入", "control input"),
    ("状态转移", "state transition"),
    ("观测", "observation"),
    ("过程噪声", "process noise"),
    ("测量噪声", "measurement noise"),
    ("粒子数量", "number of particles"),
    ("标准差", "standard deviation"),
    ("信息矩阵", "information matrix"),
    ("平方根", "square root"),
    ("无迹", "unscented"),
    ("扩展卡尔曼", "extended Kalman"),
    ("标准卡尔曼", "standard Kalman"),
    ("粒子滤波", "particle filter"),
    ("信息滤波", "information filter"),
    ("岭回归", "ridge regression"),
    ("法方程", "normal equations"),
    ("病态时自动 SVD", "SVD when ill-conditioned"),
    ("秩亏设计矩阵", "rank-deficient design matrices"),
    ("对称正定", "symmetric positive definite"),
    ("薄 SVD", "thin SVD"),
    ("伪逆", "pseudoinverse"),
    ("条件数", "condition number"),
    ("特征分解", "eigendecomposition"),
    ("特征向量在结果矩阵的**列**中", "eigenvectors in **columns** of result matrix"),
    ("按特征值降序排列", "sorted by eigenvalue descending"),
    ("点积", "dot product"),
    ("欧几里得范数", "Euclidean norm"),
    ("向量加法", "vector addition"),
    ("向量减法", "vector subtraction"),
    ("标量乘法", "scalar multiplication"),
    ("归一化", "normalization"),
    ("批量操作", "batch operations"),
    ("大矩阵", "Large matrices"),
    ("小矩阵", "Small matrices"),
    ("未收敛时回退", "fallback when not converged"),
    ("与库内 batch 卷积一致", "consistent with in-library batch convolution"),
    ("列矩阵散射回 NHWC 梯度（累加）", "scatter column matrix back to NHWC gradient (accumulate)"),
    ("展开为", "expand to"),
    ("是否在容差内对称", "whether square matrix is symmetric within tolerance"),
    ("由薄 SVD 估计", "estimated from thin SVD"),
    ("由 U、Σ、Vᵀ 重构 A", "reconstruct A from U, Σ, Vᵀ"),
    ("形状与", "shape and"),
    ("索引器", "indexer"),
    ("获取指定列的值", "Gets value at column index"),
    ("获取列数", "Gets column count"),
    ("初始化一行数据", "Initializes one data row"),
    ("仅支持", "only supports"),
    ("使用其他类型时请选择", "use"),
    ("用于多变量输入场景", "for multivariate input"),
    ("其", "its"),
    ("属性类型为", "property type is"),
    ("默认为", "defaults to"),
    ("可选的", "optional"),
    ("默认为", "default"),
    ("线程安全", "thread-safe"),
    ("异步追加采样数据（可从其他线程调用）", "async append samples (callable from other threads)"),
    ("刷新待追加的采样数据，返回实际追加的数量", "flush pending appends; returns count appended"),
    ("在指定索引处插入采样数据", "insert samples at index"),
    ("在指定时间点处插入采样数据", "insert samples at time"),
    ("移除指定索引开始的指定数量的采样数据", "remove samples from index"),
    ("移除指定时间段的采样数据", "remove samples in time range"),
    ("重采样信号，返回", "resample signal; returns"),
    ("转换到频域", "transform to frequency domain"),
    ("释放资源", "release resources"),
    ("异步追加采样数据（线程安全）", "async append samples (thread-safe)"),
    ("刷新待追加的采样数据", "flush pending appends"),
    ("在指定索引处插入", "insert at index"),
    ("在指定时间点处插入", "insert at time"),
    ("移除指定范围", "remove range"),
    ("移除指定时间段", "remove time range"),
    ("与", "similar to"),
    ("类似，包含", "similar; includes"),
    ("以及所有", "and all"),
    ("所有", "All"),
    ("均可用，采用", "available with"),
    ("优化", "optimization"),
    ("并返回新的信号", "returns new signal"),
    ("从所在信号中脱离成为单独的信号", "decouple into standalone signal"),
    ("支持", "supports"),
    ("之间的运算", "operations between"),
    ("及", "and"),
    ("之间", "between"),
    ("用于", "for"),
    ("表示", "represents"),
    ("提供", "provides"),
    ("定义了", "defines"),
    ("继承自", "extends"),
    ("实现了", "implements"),
    ("是一个", "is a"),
    ("是一个用于", "is used for"),
    ("是一个静态类", "is a static class"),
    ("是一个泛型静态类", "is a generic static class"),
    ("是一个**只读结构体**", "is a **readonly struct**"),
    ("是一个用于表示", "represents"),
    ("是一个用于实现", "implements"),
    ("是一个用于执行", "performs"),
    ("是一个用于一维", "is a 1D"),
    ("是一个用于二维", "is a 2D"),
    ("是一个简单的", "is a simple"),
    ("内部使用", "uses internally"),
    ("存储采样数据", "stores sample data"),
    ("根据采样数量和采样率初始化信号", "initializes signal from sample count and rate"),
    ("根据时间长度和采样率初始化信号", "initializes signal from duration and rate"),
    ("根据采样数量和采样率初始化", "initializes from sample count and rate"),
    ("根据时间长度和采样率初始化", "initializes from duration and rate"),
    ("创建信号的副本", "creates a copy of the signal"),
    ("将时域信号转换为频域信号", "transforms time-domain signal to frequency domain"),
    ("重采样信号", "resamples signal"),
    ("获取或设置指定索引处的采样值", "gets or sets sample at index"),
    ("以索引获取信号段的子段", "gets sub-segment by index range"),
    ("以时间量获取信号的子段", "gets sub-segment by time range"),
    ("将信号段转换为频域信号", "transforms segment to frequency domain"),
    ("生成指定波形，并根据行为对信号进行处理", "generates waveform and applies behaviour to signal"),
    ("在修改采样值后调用此方法，通知信号对象样本已被修改",
     "call after modifying samples to notify the signal"),
    ("使用指定的 bin 数量计算 Shannon 熵", "computes Shannon entropy with given bin count"),
    ("用于划分数据的 bin 数量，默认为 32", "bin count for histogram; default 32"),
    ("计算得到的熵值", "computed entropy"),
    ("信号周期", "signal period"),
    ("信号幅度（最大值与最小值之差）", "signal amplitude (max − min)"),
    ("总功率（样本平方和）", "total power (sum of squared samples)"),
    ("平均功率（样本平方和 / 样本数）", "average power (sum of squares / count)"),
    ("总能量（等同于总功率）", "total energy (same as total power)"),
    ("平均能量（样本平方均值）", "average energy (mean squared sample)"),
    ("归一化 Shannon 熵，值范围 [0, 1]", "normalized Shannon entropy in [0, 1]"),
    ("信号持续时间", "signal duration"),
    ("信号长度", "signal length"),
    ("频域信号的幅度数组", "magnitude array"),
    ("频谱质心", "spectral centroid"),
    ("频域信号的频率", "frequency"),
    ("频域信号的相位数组", "phase array"),
    ("根据相位计算的角速度数组", "angular velocity from phase"),
    ("频域变换在时域信号中的偏移量", "offset of FFT in time-domain signal"),
    ("变换长度（2 的幂次）", "transform length (power of 2)"),
    ("实际有效数据长度", "actual valid data length"),
    ("频率分辨率", "frequency resolution"),
    ("所使用的窗函数类型", "window type applied"),
    ("变换结果的复数数组", "complex FFT result array"),
    ("关联的原始时域信号", "associated time-domain signal"),
    ("频域信号在原始数据中的起始量", "start offset in original data"),
    ("未补0的长度，也是实际有效数据的长度", "unpadded length; actual valid data length"),
    ("该值为2的N次方，通常会比 `ActualLength` 大", "power of 2; usually larger than `ActualLength`"),
    ("所关联的实信号", "associated real signal"),
    ("使用复信号的一半来计算幅度，这样会丢弃镜像部分",
     "magnitude from half of complex spectrum (discards mirror)"),
    ("使用复信号的一半来计算质心，这样会丢弃镜像部分",
     "centroid from half of complex spectrum (discards mirror)"),
    ("使用完整的复信号来计算频率", "frequency from full complex spectrum"),
    ("使用复信号的一半来计算相位，这样会丢弃镜像部分",
     "phase from half of complex spectrum (discards mirror)"),
    ("根据复信号的相位计算角速度", "angular velocity from complex phase"),
    ("将索引转换为频率", "converts index to frequency"),
    ("将索引转换为频率（实例方法）", "converts index to frequency (instance)"),
    ("将频率转换为索引", "converts frequency to index"),
    ("对FFT结果进行逆变换，并将结果写回信号的采样数据中",
     "inverse FFT; writes result back to signal samples"),
    ("对 FFT 结果执行逆变换，将结果写回原始时域信号的采样数据中",
     "inverse FFT; writes back to original time-domain samples"),
    ("用于分析信号在时域中的统计和结构特征",
     "for time-domain statistical and structural features"),
    ("单线程场景下的采样数据访问", "sample access for single-threaded use"),
    ("支持运行时修改采样数据，包括追加、插入、删除和重采样操作",
     "runtime sample mutation: append, insert, delete, resample"),
    ("锁保护的采样数据视图", "lock-protected sample view"),
    ("定义了频域分析的常用属性", "common frequency-domain analysis properties"),
    ("定义了完整的频域信号接口", "full frequency-domain signal contract"),
    ("封装了傅里叶变换的结果，包括原始信号、窗函数信息和计算得到的频域特征",
     "wraps FFT result with signal, window, and computed frequency features"),
    ("用于表示信号的只读连续子段", "readonly contiguous sub-segment of a signal"),
    ("时域特征属性采用延迟计算", "time-domain features are lazily computed"),
    ("是一个用于扩展信号类功能的静态类，提供了生成各种波形的方法",
     "static extensions to generate waveforms"),
    ("提供多种曲线拟合方法的统一入口", "unified entry for curve-fitting methods"),
    ("类型参数", "type parameter"),
    ("约束为", "constrained to"),
    ("枚举定义了所有支持的曲线拟合方法", "enum of supported curve-fitting methods"),
    ("表示曲线拟合的结果", "curve fit result"),
    ("表示一行多列的数据，用于多变量输入的拟合方法",
     "one row of multi-column data for multivariate fitting"),
    ("枚举定义了拟合算法的优化模式", "enum of fitting optimization modes"),
    ("标准托管代码", "standard managed code"),
    ("两种模式", "two modes"),
]

# Manual exact-line overrides (highest priority)
EXACT = {
    "    当前位置 : [首页](HOME_zh.md)/[机器学习模块](Module_MachineLearning_zh.md)":
        "Location: [Home](HOME.md) / [Machine learning](Module_MachineLearning.md)",
    "当前位置 : [首页](HOME_zh.md)/[线性代数模块](Module_LinearAlgebra_zh.md)":
        "Location: [Home](HOME.md) / [Linear algebra](Module_LinearAlgebra.md)",
    "当前位置 : [首页](HOME_zh.md)/[实验性模块](Module_Experimental_zh.md)":
        "Location: [Home](HOME.md) / [Experimental](Module_Experimental.md)",
    "# 机器学习模块 - Machine Learning Module": "# Machine learning module",
    "# 线性代数模块 - Linear Algebra Module": "# Linear algebra module",
    "# 实验性模块 - Experimental Module": "# Experimental module",
    ":ledger:目录  ": ":ledger: Contents",
    "### 快速示例": "### Quick examples",
    "### 0.9 分类器完整示例": "### 0.9 classifier walkthrough",
    "### 属性": "### Properties",
    "### 构造器": "### Constructors",
    "### 方法": "### Methods",
    "### 方法清单及说明": "### Methods",
    "### 索引器": "### Indexers",
    "### 隐式转换": "### Implicit conversions",
    "### 运算符": "### Operators",
    "### 代码示例": "### Code example",
    "### 描述": "### Description",
    "### 重写方法": "### Overrides",
    "# 距离度量类": "# Distance metric classes",
    "| 子系统 | 命名空间 / 类型 | 说明 |": "| Subsystem | Namespace / types | Description |",
    "| 属性 | 类型 | 说明 |": "| Property | Type | Description |",
    "| 方法 | 说明 |": "| Method | Description |",
    "| 方法 | 说明 | 模型 |": "| Method | Description | Model |",
    "| 值 | 说明 |": "| Value | Description |",
    "// 分类": "// Classification",
    "无公开属性。": "No public properties.",
    "以下示例使用二维数值特征与整数类别标签（`0`、`1`、`2`），与 `Examples/MachineLearning_example` 及 `TESTS/ML_module_test` 中的用法一致。所有分类器均实现 `IClassifier<T>`，统一调用 `Fit(x, y)` / `Predict(sample)`。":
        "The examples below use 2D numeric features and integer class labels (`0`, `1`, `2`), consistent with `Examples/MachineLearning_example` and `TESTS/ML_module_test`. All classifiers implement `IClassifier<T>` with `Fit(x, y)` / `Predict(sample)`.",
    "**Softmax / Logistic mini-batch 训练**": "**Softmax / Logistic mini-batch training**",
    "- :bookmark: [模块概览与新增 API](#模块概览与新增-api)（见上文表格）":
        "- :bookmark: [Module overview and new APIs](#module-overview-and-new-apis) (see table above)",
    "var projected = lda.Transform([2.1, 1.8]); // 降维后的特征":
        "var projected = lda.Transform([2.1, 1.8]); // reduced features",
    "> 以下所有类型都位于命名空间 : Vorcyc.Mathematics.MachineLearning":
        "> All types below are in the namespace `Vorcyc.Mathematics.MachineLearning`",
    "> 以下所有类型都位于命名空间 ：Vorcyc.Mathematics.LinearAlgebra":
        "> All types below are in the namespace `Vorcyc.Mathematics.LinearAlgebra`",
    "> 以下类型都位于 Vorcyc.Mathematics.Experimental.KalmanFilters 命名空间":
        "> All types below are in the namespace `Vorcyc.Mathematics.Experimental.KalmanFilters`",
    "> 以下类型都位于 Vorcyc.Mathematics.Experimental.Signals 命名空间":
        "> All types below are in the namespace `Vorcyc.Mathematics.Experimental.Signals`",
    "> 以下类型都位于 Vorcyc.Mathematics.Experimental.CurveFitting 命名空间":
        "> All types below are in the namespace `Vorcyc.Mathematics.Experimental.CurveFitting`",
}


def translate_text(text: str) -> str:
    if text in EXACT:
        return EXACT[text]
    out = text
    # structural header suffixes
    out = re.sub(r"^(## .+?) 类$", r"\1 class", out)
    out = re.sub(r"^(## .+?) 结构$", r"\1 struct", out)
    out = re.sub(r"^(## .+?) 接口$", r"\1 interface", out)
    out = re.sub(r"^(## .+?) 枚举$", r"\1 enum", out)
    out = re.sub(r"^(#### \d+\. )(.+?)运算符$", lambda m: f"{m.group(1)}{m.group(2)}operator", out)
    out = re.sub(r"^(#### \d+\. )隐式转换为(.+)$", lambda m: f"{m.group(1)}Implicit conversion to {m.group(2)}", out)
    out = re.sub(r"^(#### \d+\. )矩阵(.+)$", lambda m: f"{m.group(1)}Matrix{m.group(2)}", out)
    # TOC anchors
    out = out.replace("_zh.md", ".md").replace("HOME_zh.md", "HOME.md")
    out = re.sub(r"\(#([^)]+)-类\)", r"(#\1-class)", out)
    out = re.sub(r"\(#([^)]+)-结构\)", r"(#\1-struct)", out)
    out = re.sub(r"\(#([^)]+)-接口\)", r"(#\1-interface)", out)
    out = re.sub(r"\(#([^)]+)-枚举\)", r"(#\1-enum)", out)
    out = out.replace("#模块概览与新增-api", "#module-overview-and-new-apis")
    out = out.replace("#09-分类器完整示例", "#09-classifier-walkthrough")
    out = out.replace("#距离度量类", "#distance-metric-classes")
    # breadcrumb regex
    m = re.match(r"\s*当前位置\s*:\s*\[首页\]\(HOME[^)]*\)/\[(.+?)\]\(Module_[^)]+\)", out)
    if m:
        label = m.group(1)
        for zh, (en, _) in BREADCRUMB.items():
            if zh in label:
                module = BREADCRUMB[zh][1]
                return f"Location: [Home](HOME.md) / [{en}]({module})"
    # line-start doc verbs
    for prefix, en_prefix in [
        ("  - 获取", "  - Gets "),
        ("  - 返回", "  - Returns "),
        ("  - 计算", "  - Computes "),
        ("  - 使用", "  - Uses "),
        ("  - 判断", "  - Determines whether "),
        ("  - 初始化", "  - Initializes "),
        ("  - 执行", "  - Performs "),
        ("  - 训练", "  - Trains "),
        ("  - 预测", "  - Predicts "),
        ("  - 创建", "  - Creates "),
        ("  - 扩展", "  - Expands "),
        ("  - 更新", "  - Updates "),
        ("  - 估计", "  - Estimates "),
        ("  - 添加", "  - Adds "),
        ("  - 保存", "  - Saves "),
        ("  - 加载", "  - Loads "),
        ("  - 将", "  - "),
        ("  - 对", "  - "),
        ("  - 根据", "  - "),
        ("  - 从", "  - Loads from "),
        ("  - 在", "  - "),
        ("  - 沿", "  - Along "),
        ("  - 用", "  - Fills "),
        ("  - 水平", "  - Horizontally "),
        ("  - 垂直", "  - Vertically "),
        ("  - 查找", "  - Finds "),
        ("  - 找到", "  - Finds "),
        ("  - 搜索", "  - Searches "),
        ("  - 生成", "  - Generates "),
        ("  - 学习", "  - Learns "),
        ("  - 固定为", "  - Fixed to "),
        ("  - 可选", "  - Optional "),
        ("  - 归一化", "  - Normalizes "),
        ("  - 克隆", "  - Clones "),
        ("  - 写入", "  - Writes "),
        ("  - 读取", "  - Reads "),
        ("  - 相等", "  - Equality "),
        ("  - 不相等", "  - Inequality "),
        ("  - 加法", "  - Addition "),
        ("  - 减法", "  - Subtraction "),
        ("  - 乘法", "  - Multiplication "),
        ("  - 除法", "  - Division "),
        ("  - 矩阵", "  - Matrix "),
        ("  - 线性", "  - Linear "),
        ("  - 多项式", "  - Polynomial "),
        ("  - 高斯", "  - Gaussian "),
        ("  - Sigmoid ", "  - Sigmoid "),
        ("  - 返回", "  - Returns "),
        ("    - `ArgumentException`: 当", "    - `ArgumentException`: Thrown when "),
        ("  - 参数:", "  - Parameters:"),
        ("  - 返回值:", "  - Returns:"),
        ("  - 异常:", "  - Exceptions:"),
    ]:
        if out.startswith(prefix):
            out = en_prefix + out[len(prefix):].lstrip()
            break
    # fragment replacement (longest first)
    frags = sorted(FRAGMENTS, key=lambda x: len(x[0]), reverse=True)
    for zh, en in frags:
        out = out.replace(zh, en)
    # cleanup common leftovers
    out = out.replace("。.", ".")
    out = out.replace("。。", "。")
    if out.endswith("。"):
        out = out[:-1] + "."
    out = out.replace("抛出。", "thrown.")
    out = out.replace("抛出", "thrown")
    out = out.replace("class的新实例", " class instance")
    out = out.replace("class的新实例。", " class instance.")
    out = out.replace(" 类的新实例", " class instance")
    out = out.replace("struct体", "struct")
    out = out.replace("class型", "Type")
    return out


def translate_file_content(text: str) -> str:
    lines = text.splitlines()
    out_lines = []
    for line in lines:
        if re.search(r"[\u4e00-\u9fff]", line):
            out_lines.append(translate_text(line))
        else:
            out_lines.append(line)
    return "\n".join(out_lines) + ("\n" if text.endswith("\n") else "")


def build_translation_map():
    lines = json.loads(LINES_JSON.read_text(encoding="utf-8"))
    mapping = {line: translate_text(line) for line in lines}
    TRANS_JSON.write_text(json.dumps(mapping, ensure_ascii=False, indent=2), encoding="utf-8")
    remaining = sum(1 for k, v in mapping.items() if re.search(r"[\u4e00-\u9fff]", v))
    print(f"translation map: {len(mapping)} lines, {remaining} still contain Chinese")


def main():
    build_translation_map()
    for src_name, dst_name in MAPPINGS:
        src = HANS / src_name
        dst = EN / dst_name
        text = translate_file_content(src.read_text(encoding="utf-8"))
        dst.write_text(text, encoding="utf-8", newline="\n")
        chinese_left = len(re.findall(r"[\u4e00-\u9fff]", text))
        print(f"{dst_name}: {len(text.splitlines())} lines, {chinese_left} Chinese chars remaining")
    return 0


if __name__ == "__main__":
    sys.exit(main())
