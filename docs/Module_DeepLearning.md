当前位置 : [根目录](README.md)/[深度学习模块](Module_DeepLearning.md)

# 深度学习模块 - Deep Learning Module

提供深度学习相关的函数和运算。

> 以下方法均位于类 ：Vorcyc.Mathematics.DeepLearning

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.DeepLearning.Layers.Layers 类](#vorcycmathematicsdeeplearninglayerslayers-类)  
  - :bookmark: [BatchNorm 方法](#1-batchnorm-方法)  
  - :bookmark: [Conv2D 方法](#2-conv2d-方法)  
  - :bookmark: [Dense 方法](#3-dense-方法)  
  - :bookmark: [Fusion 方法](#4-fusion-方法)  
  - :bookmark: [JoinLayer 方法](#5-joinlayer-方法)  
  - :bookmark: [LinearLayer 方法](#6-linearlayer-方法)  
  - :bookmark: [MaxPool2D 方法](#7-maxpool2d-方法)  
  - :bookmark: [ReLU 方法](#8-relu-方法)  
  - :bookmark: [Sigmoid 方法](#9-sigmoid-方法)  
  - :bookmark: [Upsample2D 方法](#10-upsample2d-方法)  

---

## Vorcyc.Mathematics.DeepLearning.Layers.Layers 类

Vorcyc.Mathematics.DeepLearning.Layers.Layers 是一个实现神经网络层的静态类。

### 方法

#### 1. BatchNorm
- `public static Tensor<T> BatchNorm<T>(Tensor<T> input, Tensor<T> mean, Tensor<T> variance, Tensor<T> shift, Tensor<T> scale) where T : IBinaryFloatingPointIeee754<T>`
  - 对输入张量进行批量归一化。
  - 参数:
    - `input`: 输入的张量。
    - `mean`: 均值张量。
    - `variance`: 方差张量。
    - `shift`: 偏移量张量。
    - `scale`: 缩放因子张量。
  - 返回值: 归一化后的张量。

- `public static Tensor BatchNorm(Tensor input, Tensor mean, Tensor variance, Tensor shift, Tensor scale)`
  - 对输入张量进行批量归一化。
  - 参数:
    - `input`: 输入的张量。
    - `mean`: 均值张量。
    - `variance`: 方差张量。
    - `shift`: 偏移量张量。
    - `scale`: 缩放因子张量。
  - 返回值: 归一化后的张量。

#### 2. Conv2D
- `public static Tensor<T> Conv2D<T>(Tensor<T> input, Tensor<T>[] filters, Tensor<T> biases, int stride = 1, int dilation = 1) where T : IBinaryFloatingPointIeee754<T>`
  - 对 2D 张量执行卷积操作。
  - 参数:
    - `input`: 输入的 2D 张量。
    - `filters`: 卷积核的数组。
    - `biases`: 偏置张量。
    - `stride`: 卷积步长，默认为 1。
    - `dilation`: 卷积扩张率，默认为 1。
  - 返回值: 卷积结果的张量。

- `public static Tensor Conv2D(Tensor input, Tensor[] filters, Tensor biases, int stride = 1, int dilation = 1)`
  - 对 2D 张量执行卷积操作。
  - 参数:
    - `input`: 输入的 2D 张量。
    - `filters`: 卷积核的数组。
    - `biases`: 偏置张量。
    - `stride`: 卷积步长，默认为 1。
    - `dilation`: 卷积扩张率，默认为 1。
  - 返回值: 卷积结果的张量。

#### 3. Dense
- `public static Tensor<T> Dense<T>(Tensor<T> input, Tensor<T>[] weights, Tensor<T> biases) where T : IBinaryFloatingPointIeee754<T>`
  - 对输入张量执行密集（全连接）层操作。
  - 参数:
    - `input`: 输入的张量。
    - `weights`: 权重张量的数组。
    - `biases`: 偏置张量。
  - 返回值: 执行密集操作后的张量。

- `public static Tensor Dense(Tensor input, Tensor[] weights, Tensor biases)`
  - 对输入张量执行密集（全连接）层操作。
  - 参数:
    - `input`: 输入的张量。
    - `weights`: 权重张量的数组。
    - `biases`: 偏置张量。
  - 返回值: 执行密集操作后的张量。

#### 4. Fusion
- `public static Tensor<T> Fusion<T>(Tensor<T> input, Tensor<T> joint) where T : IBinaryFloatingPointIeee754<T>`
  - 合并两个张量。
  - 参数:
    - `input`: 输入的张量。
    - `joint`: 要合并的张量。
  - 返回值: 合并后的张量。

- `public static Tensor Fusion(Tensor input, Tensor joint)`
  - 合并两个张量。
  - 参数:
    - `input`: 输入的张量。
    - `joint`: 要合并的张量。
  - 返回值: 合并后的张量。

#### 5. JoinLayer
- `public static Tensor<T> JoinLayer<T>(Tensor<T> input, Tensor<T> joint) where T : IBinaryFloatingPointIeee754<T>`
  - 合并两个张量层。
  - 参数:
    - `input`: 输入的张量。
    - `joint`: 要合并的张量。
  - 返回值: 合并后的张量。

- `public static Tensor JoinLayer(Tensor input, Tensor joint)`
  - 合并两个张量层。
  - 参数:
    - `input`: 输入的张量。
    - `joint`: 要合并的张量。
  - 返回值: 合并后的张量。

#### 6. LinearLayer
- `public static Tensor<T> LinearLayer<T>(Tensor<T> input, Tensor<T>[] weights, Tensor<T> biases) where T : IBinaryFloatingPointIeee754<T>`
  - 对输入张量执行线性（全连接）层操作。
  - 参数:
    - `input`: 输入的张量。
    - `weights`: 权重张量的数组。
    - `biases`: 偏置张量。
  - 返回值: 执行线性操作后的张量。

- `public static Tensor LinearLayer(Tensor input, Tensor[] weights, Tensor biases)`
  - 对输入张量执行线性（全连接）层操作。
  - 参数:
    - `input`: 输入的张量。
    - `weights`: 权重张量的数组。
    - `biases`: 偏置张量。
  - 返回值: 执行线性操作后的张量。

#### 7. MaxPool2D
- `public static Tensor<T> MaxPool2D<T>(Tensor<T> input) where T : IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 对输入张量执行二维最大池化操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 执行最大池化操作后的张量。

- `public static Tensor MaxPool2D(Tensor input)`
  - 对输入张量执行二维最大池化操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 执行最大池化操作后的张量。

#### 8. ReLU
- `public static Tensor<T> ReLU<T>(Tensor<T> input) where T : IBinaryFloatingPointIeee754<T>`
  - 对输入张量执行 ReLU 激活函数操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 执行 ReLU 操作后的张量。

- `public static Tensor ReLU(Tensor input)`
  - 对输入张量执行 ReLU 激活函数操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 执行 ReLU 操作后的张量。

#### 9. Sigmoid
- `public static Tensor<T> Sigmoid<T>(Tensor<T> input) where T : IBinaryFloatingPointIeee754<T>`
  - 对输入张量执行 Sigmoid 激活函数操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 执行 Sigmoid 操作后的张量。

- `public static Tensor Sigmoid(Tensor input)`
  - 对输入张量执行 Sigmoid 激活函数操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 执行 Sigmoid 操作后的张量。

#### 10. Upsample2D
- `public static Tensor<T> Upsample2D<T>(Tensor<T> input) where T : IBinaryFloatingPointIeee754<T>`
  - 对输入张量执行二维上采样操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 上采样后的张量。

- `public static Tensor Upsample2D(Tensor input)`
  - 对输入张量执行二维上采样操作。
  - 参数:
    - `input`: 输入的张量。
  - 返回值: 上采样后的张量。

### 代码示例
以下是一个使用 Layers 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.DeepLearning.Layers;
using Vorcyc.Mathematics.LinearAlgebra;

public class LayersExample
{
    public static void Main()
    {
        // 定义输入张量
        var input = new Tensor<float>(new float[,,] 
        { 
            { 
                { 1.0f, 2.0f }, 
                { 3.0f, 4.0f } 
            }, 
            {
                { 5.0f, 6.0f }, 
                { 7.0f, 8.0f }
            } 
        });
        // 定义均值、方差、偏移量和缩放因子张量
        var mean = new Tensor<float>(new float[] { 1.0f, 2.0f });
        var variance = new Tensor<float>(new float[] { 1.0f, 1.0f });
        var shift = new Tensor<float>(new float[] { 0.0f, 0.0f });
        var scale = new Tensor<float>(new float[] { 1.0f, 1.0f });

        // 执行批量归一化
        var normalized = Layers.BatchNorm(input, mean, variance, shift, scale);
        Console.WriteLine("BatchNorm Result:");
        PrintTensor(normalized);

        // 定义卷积核和偏置张量
        var filters = new Tensor<float>[]
        {
        new Tensor<float>(new float[,,] { { { 1.0f, 0.0f }, { 0.0f, 1.0f } } }),
        new Tensor<float>(new float[,,] { { { 0.0f, 1.0f }, { 1.0f, 0.0f } } })
        };
        var biases = new Tensor<float>(new float[] { 0.0f, 0.0f });

        // 执行卷积操作
        var convResult = Layers.Conv2D(input, filters, biases);
        Console.WriteLine("Conv2D Result:");
        PrintTensor(convResult);

        // 定义权重和偏置张量
        var weights = new Tensor<float>[]
        {
        new Tensor<float>(new float[,,] { { { 1.0f, 0.0f }, { 0.0f, 1.0f } } }),
        new Tensor<float>(new float[,,] { { { 0.0f, 1.0f }, { 1.0f, 0.0f } } })
        };
        var denseBiases = new Tensor<float>(new float[] { 0.0f, 0.0f });

        // 执行密集层操作
        var denseResult = Layers.Dense(input, weights, denseBiases);
        Console.WriteLine("Dense Result:");
        PrintTensor(denseResult);

        // 执行 ReLU 激活函数操作
        var reluResult = Layers.ReLU(input);
        Console.WriteLine("ReLU Result:");
        PrintTensor(reluResult);

        // 执行 Sigmoid 激活函数操作
        var sigmoidResult = Layers.Sigmoid(input);
        Console.WriteLine("Sigmoid Result:");
        PrintTensor(sigmoidResult);
    }

    private static void PrintTensor(Tensor<float> tensor)
    {
        for (int d = 0; d < tensor.Depth; d++)
        {
            for (int y = 0; y < tensor.Height; y++)
            {
                for (int x = 0; x < tensor.Width; x++)
                {
                    Console.Write($"{tensor[x, y, d]} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
```
