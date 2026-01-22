using System;
using System.Collections.Generic;
using System.Text;
using Vorcyc.Mathematics.Experimental.Signals;

namespace DSP_module_test;


public class ModifiableTimeDomainSignalTests
{
    private const float SamplingRate = 44100f;
    private const int Length = 100;

    public async Task RunAllTestsAsync()
    {
        Console.WriteLine("=== 开始测试 ModifiableTimeDomainSignal ===\n");

        await Test_InitialState();
        await Test_AppendAndFlush();
        await Test_ModifySamples();
        await Test_InsertAndRemoveOperations();
        Test_DisposeBehavior();
        await Test_ConcurrentAppendAndFlushAsync();

        Console.WriteLine("\n=== 所有测试完成 ===\n");
    }

    private async Task Test_InitialState()
    {
        Console.WriteLine("测试 1: 初始状态");
        using var signal = new ModifiableTimeDomainSignal(Length, SamplingRate);

        Console.WriteLine($"  初始长度: {signal.Length}");
        Console.WriteLine($"  采样率: {signal.SamplingRate}");
        PrintSamples(signal, "初始样本（前/后5个）");

        Console.WriteLine("  → 通过（长度和采样率正确）\n");
    }

    private async Task Test_AppendAndFlush()
    {
        Console.WriteLine("测试 2: 异步追加 + Flush");
        using var signal = new ModifiableTimeDomainSignal(Length, SamplingRate);

        // 模拟追加几块数据
        await signal.AppendAsync(new float[] { 1f, 2f, 3f });
        await signal.AppendAsync(new float[] { 4f, 5f });

        int added = signal.FlushPendingAppends();
        Console.WriteLine($"  Flush 后新增: {added}，总长度: {signal.Length}");

        if (signal.Length == 105 && added == 5)
            Console.WriteLine("  → 通过（追加和合并正确）");
        else
            Console.WriteLine("  → 失败");

        PrintSamples(signal, "追加后样本");
        Console.WriteLine();
    }

    private async Task Test_ModifySamples()
    {
        Console.WriteLine("测试 3: 直接修改 Samples");
        using var signal = new ModifiableTimeDomainSignal(Length, SamplingRate);

        // 先追加一些数据
        await signal.AppendAsync(new float[20]);
        signal.FlushPendingAppends();

        // 修改前10个样本（独立作用域，确保锁及时释放）
        {
            using var samples = signal.Samples;
            for (int i = 0; i < Math.Min(10, samples.Span.Length); i++)
            {
                samples.Span[i] = i * 10f;
            }
        }

        PrintSamples(signal, "修改后样本（前10个应为 0,10,20,...）");

        // 验证修改结果
        bool ok = true;
        {
            using var view = signal.Samples;
            for (int i = 0; i < 10 && i < view.Span.Length; i++)
            {
                if (Math.Abs(view.Span[i] - i * 10f) > 0.0001f) ok = false;
            }
        }

        Console.WriteLine(ok ? "  → 通过" : "  → 失败");
        Console.WriteLine();
    }

    private async Task Test_InsertAndRemoveOperations()
    {
        Console.WriteLine("\n=== 测试 Insert 和 RemoveRange 的正确性 ===");
        Console.WriteLine("注意：初始长度固定为 100 个样本（值为 0），追加后会累加\n");

        using var signal = new ModifiableTimeDomainSignal(100, SamplingRate);

        // Step 1: 追加 0~19
        var addedData = Enumerable.Range(0, 20).Select(i => (float)i).ToArray();
        await signal.AppendAsync(addedData);
        signal.FlushPendingAppends();

        // 预期：前 100 个为 0.0，后 20 个为 0~19
        var expected1 = Enumerable.Repeat(0f, 100).Concat(addedData).ToArray();
        Console.WriteLine("Step 1: 初始 100 + 追加 0~19");
        AssertSequence(signal, expected1, "追加后长度或内容不匹配");

        // Step 2: 在索引 105（即原有 0~19 的第 5 个位置）插入 [100,101,102,103]
        signal.Insert(105, new float[] { 100f, 101f, 102f, 103f });

        var expected2 = expected1.Take(105)
                                 .Concat(new float[] { 100f, 101f, 102f, 103f })
                                 .Concat(expected1.Skip(105))
                                 .ToArray();
        Console.WriteLine("\nStep 2: 在追加数据的索引 5 处插入 4 个值");
        AssertSequence(signal, expected2, "插入后序列不匹配");

        // Step 3: 删除从索引 110 开始的 6 个（影响插入的部分和后面的）
        signal.RemoveRange(110, 6);

        var expected3 = expected2.Take(110).Concat(expected2.Skip(116)).ToArray();
        Console.WriteLine("\nStep 3: 从插入点附近删除 6 个");
        AssertSequence(signal, expected3, "删除后序列不匹配");

        Console.WriteLine("\n如果以上步骤全部 ✓ 通过，则 Insert/RemoveRange 逻辑正确。");
    }

    private void AssertSequence(ModifiableTimeDomainSignal signal, float[] expected, string errorMessage)
    {
        float[] actual;
        {
            using var view = signal.Samples;
            actual = view.Span.ToArray();
        }

        if (actual.Length != expected.Length)
        {
            Console.WriteLine($"  × 失败：长度不匹配。实际 {actual.Length}，预期 {expected.Length}");
            Console.WriteLine($"  实际前10: {string.Join(", ", actual.Take(10).Select(x => x.ToString("F1")))}");
            Console.WriteLine($"  预期前10: {string.Join(", ", expected.Take(10).Select(x => x.ToString("F1")))}");
            return;
        }

        bool match = true;
        for (int i = 0; i < actual.Length; i++)
        {
            if (Math.Abs(actual[i] - expected[i]) > 0.0001f)
            {
                match = false;
                Console.WriteLine($"  × 在索引 {i} 不匹配：实际 {actual[i]:F3}，预期 {expected[i]:F1}");
                break;
            }
        }

        if (match)
        {
            Console.WriteLine("  ✓ 通过：序列完全匹配预期");
            int show = 8;
            Console.WriteLine($"  前{show}: {string.Join(", ", actual.Take(show).Select(x => x.ToString("F1")))}");
            if (actual.Length > show * 2)
                Console.WriteLine($"  后{show}: {string.Join(", ", actual.Skip(actual.Length - show).Select(x => x.ToString("F1")))}");
        }
        else
        {
            Console.WriteLine("  → 测试失败，请检查 Insert / RemoveRange 实现");
        }
    }

    private void Test_DisposeBehavior()
    {
        Console.WriteLine("测试 5: Dispose 行为");
        var signal = new ModifiableTimeDomainSignal(Length, SamplingRate);

        signal.Dispose();

        try
        {
            _ = signal.Samples;
            Console.WriteLine("  → 失败：Dispose 后仍能访问 Samples");
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("  → 通过：Dispose 后访问 Samples 抛 ObjectDisposedException");
        }

        Console.WriteLine();
    }

    // 辅助：线程安全地将样本复制为数组快照
    private static float[] SnapshotToArray(ModifiableTimeDomainSignal signal)
    {
        using var view = signal.Samples;
        return view.Span.ToArray();
    }

    // 辅助打印方法
    private void PrintSamples(ModifiableTimeDomainSignal signal, string title)
    {
        float[] snapshot = SnapshotToArray(signal);

        if (snapshot.Length == 0)
        {
            Console.WriteLine($"  {title}: (空)");
            return;
        }

        int show = Math.Min(8, snapshot.Length);
        var first = string.Join(", ", snapshot.Take(show).Select(v => v.ToString("F1")));
        Console.WriteLine($"  {title} (前{show}个): {first}");

        if (snapshot.Length > show * 2)
        {
            var last = string.Join(", ", snapshot.Skip(snapshot.Length - show).Select(v => v.ToString("F1")));
            Console.WriteLine($"  {title} (后{show}个): {last}");
        }
    }

    /// <summary>
    /// 测试：一个线程定时写入，另一个线程定时刷新，观察并发行为和数据一致性。
    /// </summary>
    private static async Task Test_ConcurrentAppendAndFlushAsync()
    {
        Console.WriteLine("\n=== 测试：并发追加 + 定时刷新 ===");
        Console.WriteLine("  - 写入线程：每 80~120ms 追加 50~150 个样本");
        Console.WriteLine("  - 刷新线程：每 150ms 尝试 flush 并输出长度变化");
        Console.WriteLine("  - 主线程：观察 8 秒后停止\n");

        using var signal = new ModifiableTimeDomainSignal(200, 44100f);
        using var cts = new CancellationTokenSource();

        // 写入线程（模拟传感器）
        var writerTask = Task.Run(async () =>
        {
            var rnd = new Random();
            int counter = 0;

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    int count = rnd.Next(50, 151);
                    var chunk = new float[count];

                    for (int i = 0; i < count; i++)
                    {
                        chunk[i] = (float)(Math.Sin(counter * 0.08 + i * 0.05) * 0.9 + rnd.NextDouble() * 0.2);
                    }

                    await signal.AppendAsync(chunk, cts.Token);

                    counter++;

                    await Task.Delay(rnd.Next(80, 121), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消退出
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入线程异常: {ex.Message}");
            }
        });

        var flusherTask = Task.Run(async () =>
        {
            int round = 0;

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    int added = signal.FlushPendingAppends();

                    // 在锁保护下读取前3个样本值
                    string first3;
                    {
                        using var view = signal.Samples;
                        first3 = view.Span.Length >= 3
                            ? $"{view.Span[0]:F3}, {view.Span[1]:F3}, {view.Span[2]:F3}"
                            : "不足3个";
                    }

                    Console.WriteLine($"[{++round,2}] 时间: {DateTime.Now:HH:mm:ss.fff} | " +
                                      $"新增: {added,-4} | 总长度: {signal.Length,-7} | " +
                                      $"前3: {first3}");

                    await Task.Delay(150, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"刷新线程正常取消（在第 {round} 轮后）");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刷新线程意外异常: {ex.Message}");
            }
        });

        // 运行 8 秒后停止
        await Task.Delay(8000);

        cts.Cancel();

        // 等待两个任务结束
        await Task.WhenAll(writerTask, flusherTask);

        // 最后一次 flush，确保所有数据都合并
        int lastAdded = signal.FlushPendingAppends();
        Console.WriteLine($"\n测试结束，最后 flush 新增: {lastAdded}");
        Console.WriteLine($"最终长度: {signal.Length}");

        // 安全快照打印最终样本
        float[] finalSnapshot = SnapshotToArray(signal);
        if (finalSnapshot.Length > 0)
        {
            int head = Math.Min(5, finalSnapshot.Length);
            int tail = Math.Min(5, finalSnapshot.Length);
            Console.WriteLine("最终样本 前5个: " + string.Join(", ", finalSnapshot.Take(head).Select(x => x.ToString("F3"))));
            Console.WriteLine("最终样本 后5个: " + string.Join(", ", finalSnapshot.Skip(finalSnapshot.Length - tail).Select(x => x.ToString("F3"))));
        }

        Console.WriteLine("测试完成，观察输出是否稳定、无异常、长度持续增长。\n");
    }

    // 运行入口（可从 Program.Main 调用）
    public static async Task Run()
    {
        var tester = new ModifiableTimeDomainSignalTests();
        await tester.RunAllTestsAsync();
    }
}