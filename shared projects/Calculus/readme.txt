1.	Differentiation (导数)

•	Derivative: 单/多变量数值导数与自动微分；Gradient / GradientAD。

•	Jacobian: 向量值函数的雅可比矩阵。

•	Hessian: 标量多元 Hessian；HyperDualNumber 二阶 AD。

•	DualNumber: 前向模式自动微分对偶数。

2.	Integration (积分)

•	Integration: 梯形、辛普森、Romberg、Gauss-Legendre、自适应 Simpson；绑定函数不定积分；反常与二重积分。

3.	Limits (极限)

•	Limits: 左/右/双侧极限；Aitken Δ² 加速。

4.	Series (级数)

•	TaylorSeries: 泰勒展开（Horner + 系数数组缓存）。

•	FourierSeries: 傅里叶级数（预采样 + 复合 Simpson）。

•	FourierSeriesFromSamples: FFT 估计系数。

•	PadeApproximant / ChebyshevSeries: Padé 与 Chebyshev（workspace 复用）。

5.	Numerical Methods (数值方法)

•	NewtonRaphson / Bisection / Brent: 标量求根。

•	RungeKutta / RungeKuttaSystem: RK4；SolveInPlace / SolveTrajectory。

•	AdaptiveRungeKutta / AdaptiveRungeKuttaSystem: 自适应步长；OdeStepControl。

•	NewtonRaphsonSystem: 多元牛顿法。

•	ImplicitEuler / ShootingBvpSolver: 刚性 ODE 与打靶 BVP。

6.	Optimization (优化)

•	BfgsOptimizer / LBfgsOptimizer / LevenbergMarquardt / LineSearch。



性能调优（详见 wiki_hans/Module_Calculus_zh.md §性能调优指南）

•	循环外构造求解器/积分器/导数器，循环内复用。

•	Gradient(point, destination)、Jacobian/Hessian.Calculate(..., matrix?)、ArmijoBacktracking(..., trial)。

•	RungeKuttaSystem.SolveInPlace、ShootingBvpSolver、PadeWorkspace、ChebyshevFitWorkspace。

•	Integration(func,h) 绑定路径 + GetIndefiniteIntegral 累积表。

•	double/float 向量运算经 CalculusVectorOps SIMD。

•	基准: benchmarks/basic_benchmark/Calculus_benchmark.cs


