﻿namespace Vorcyc.Mathematics;

public static partial class IComparableExtension
{





    private static Task[] AllocateTasks_Min<TValue>(int workerCount, TValue[] values, int start, int length, out TValue[] minValues)
        where TValue : IComparable, IComparable<TValue>
    {
        var tasks = new Task[workerCount];

        var minValuesForTasks = new TValue[workerCount];

        var mod = length % workerCount;

        var segmentLength = (length - mod) / workerCount;

        var startIndex = 0;

        //分配实例，分配任务
        for (int workerIndex = 0; workerIndex < workerCount - 1; workerIndex++)
        {
            startIndex = segmentLength * workerIndex + start;

            tasks[workerIndex] = new Task((state) =>
            {
                var localState = (Tuple<int, int>)state;
                var resultWorkerIndex = localState.Item1;//单个任务执行后存储在 maxValues 里的索引
                var localStartIndex = localState.Item2;//单个任务的起始索引

                minValuesForTasks[resultWorkerIndex] = CompareMin(values, localStartIndex, segmentLength);

            }, state: Tuple.Create(workerIndex, startIndex));

            startIndex += segmentLength;
        }

        //分配最后一个
        tasks[tasks.Length - 1] = new Task((localStartIndex2) =>
        {
            var lastSegmentLength = start + length - (int)localStartIndex2;
            minValuesForTasks[tasks.Length - 1] = CompareMin(values, (int)localStartIndex2, lastSegmentLength);
        }, state: startIndex);

        minValues = minValuesForTasks;
        return tasks;
    }

    /// <summary>
    /// Returns the minimum value in a parallel sequence of values.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="values"></param>
    /// <param name="numberOfWorkers"></param>
    /// <returns></returns>
    public static Task<TValue> CompareMinAsync<TValue>(this TValue[] values, int? numberOfWorkers = null, bool useTPL = false)
        where TValue : IComparable, IComparable<TValue>
    {
        if (useTPL)
        {
            return Task.Run(() =>
            {
                var result = values[0];
                Parallel.ForEach(values, (ele) =>
                {
                    if (ele.CompareTo(result) == -1)
                        result = ele;
                });
                return result;
            });
        }
        else
        {
            return Task.Run(() =>
            {

                int workerCount = 0;
                if (!numberOfWorkers.HasValue)
                    workerCount = Environment.ProcessorCount;
                else
                    workerCount = numberOfWorkers.Value > Environment.ProcessorCount ? Environment.ProcessorCount : numberOfWorkers.Value;

                var tasks = AllocateTasks_Min(workerCount, values, 0, values.Length, out TValue[] maxValues);

                //gogoogo
                foreach (var t in tasks)
                    t.Start();

                Task.WaitAll(tasks);

                return CompareMin(maxValues);

            });

        }
    }

    /// <summary>
    /// Parallel for huge data amount.
    /// </summary>
    /// <typeparam name="TValue">unmanaged , IComparable , IComparable&lt;T&gt;</typeparam>
    /// <param name="values">The input array.</param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="numberOfWorkers">If null use TPL, otherwise this specify the number of cores to compute, 
    /// this value is less than or equal to <strong>Environment.ProcessorCount</strong>.</param>
    /// <returns></returns>
    public static Task<TValue> CompareMinAsync<TValue>(this TValue[] values, int start, int length, int? numberOfWorkers = null, bool useTPL = false)
        where TValue : IComparable, IComparable<TValue>
    {
        if (useTPL)
        {
            return Task.Run(() =>
            {
                var result = values[start];

                Parallel.For(start, start + length, (idx) =>
                {
                    if (values[idx].CompareTo(result) == -1)
                        result = values[idx];
                });

                return result;

                //return values.AsParallel().Skip(start).Take(length).Max(); //low performance
            });

        }
        else
        {
            return Task.Run(() =>
            {
                int workerCount = 0;
                if (!numberOfWorkers.HasValue)
                    workerCount = Environment.ProcessorCount;
                else
                    workerCount = numberOfWorkers.Value > Environment.ProcessorCount ? Environment.ProcessorCount : numberOfWorkers.Value;

                var tasks = AllocateTasks_Min(workerCount, values, start, length, out TValue[] maxValues);

                //gogoogo
                foreach (var t in tasks)
                {
                    t.Start();
                }

                Task.WaitAll(tasks);

                return CompareMin(maxValues);

            });
        }
    }



    private static Task[] AllocateTasks_LocateMin<TValue>(
        int workerCount,
        TValue[] values, int start, int length,
        out TValue[] minValues, out int[] minValueIndics)
        where TValue : IComparable, IComparable<TValue>
    {
        var minValuesForTasks = new TValue[workerCount];//结果集 每个任务的最大值

        var minIndicsForTasks = new int[workerCount];//结果集 每个任务的最大值的索引

        var tasks = new Task[workerCount];

        var mod = length % workerCount;

        var segmentLength = (length - mod) / workerCount;

        var startIndex = 0;

        //分配实例，分配任务
        for (int workerIndex = 0; workerIndex < workerCount - 1; workerIndex++)
        {
            startIndex = segmentLength * workerIndex + start;

            tasks[workerIndex] = new Task((state) =>
            {
                var localState = (Tuple<int, int>)state;
                var resultWorkerIndex = localState.Item1;//单个任务执行后存储在 maxValues 里的索引
                var localStartIndex = localState.Item2;//单个任务的起始索引

                (minIndicsForTasks[resultWorkerIndex], minValuesForTasks[resultWorkerIndex]) =
                LocateMin(values, localStartIndex, segmentLength);

            }, state: Tuple.Create(workerIndex, startIndex));

            startIndex += segmentLength;
        }

        //分配最后一个

        tasks[tasks.Length - 1] = new Task((localStartIndex2) =>
        {
            var lastSegmentLength = start + length - (int)localStartIndex2;
            //(maxIndics[^1], maxValues[^1]) = LocateMax(values, (int)localStartIndex2, lastSegmentLength); 这语法有问题，.NET 的 BUG？
            (minIndicsForTasks[tasks.Length - 1], minValuesForTasks[tasks.Length - 1]) = LocateMin(values, (int)localStartIndex2, lastSegmentLength);
        }, state: startIndex);

        minValues = minValuesForTasks;
        minValueIndics = minIndicsForTasks;
        return tasks;
    }



    public static Task<(int, TValue)> LocateMinAsync<TValue>(this TValue[] values, int? numberOfWorkers = null, bool useTPL = false)
        where TValue : IComparable, IComparable<TValue>
    {
        if (useTPL)
        {
            return Task.Run(() =>
            {
                var result = values[0];
                int resultIndex = 0;

                Parallel.For(0, values.Length, (index) =>
                {
                    if (values[index].CompareTo(result) == -1)
                    {
                        result = values[index];
                        resultIndex = index;
                    }
                });

                return (resultIndex, result);

            });
        }
        else
        {
            return Task.Run(() =>
            {
                int workerCount = 0;
                if (!numberOfWorkers.HasValue)
                    workerCount = Environment.ProcessorCount;
                else
                    workerCount = numberOfWorkers.Value > Environment.ProcessorCount ? Environment.ProcessorCount : numberOfWorkers.Value;

                var tasks = AllocateTasks_LocateMin(workerCount, values, 0, values.Length, out TValue[] maxValues, out int[] maxIndics);

                //gogoogo
                foreach (var t in tasks)
                {
                    t.Start();
                }

                Task.WaitAll(tasks);

                //结果集中的索引和最大值
                var (maxIndex_in_MaxValues, max_in_maxValues) = LocateMin(maxValues);
                return (maxIndics[maxIndex_in_MaxValues], max_in_maxValues);
            });
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="values"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="numberOfWorkers"></param>
    /// <param name="useTPL">If true, ingore paramater <strong>numberOfWorkers</strong>.</param>
    /// <returns></returns>
    public static Task<(int, TValue)> LocateMinAsync<TValue>(this TValue[] values, int start, int length,
        int? numberOfWorkers = null, bool useTPL = false)
        where TValue : IComparable, IComparable<TValue>
    {
        if (useTPL)
        {
            return Task.Run(() =>
            {
                var result = values[0];
                int resultIndex = 0;

                Parallel.For(start, start + length, (index) =>
                {
                    if (values[index].CompareTo(result) == -1)
                    {
                        result = values[index];
                        resultIndex = index;
                    }
                });

                return (resultIndex, result);

            });
        }
        else
        {
            return Task.Run(() =>
            {
                int workerCount = 0;
                if (!numberOfWorkers.HasValue)
                    workerCount = Environment.ProcessorCount;
                else
                    workerCount = numberOfWorkers.Value > Environment.ProcessorCount ? Environment.ProcessorCount : numberOfWorkers.Value;

                var tasks = AllocateTasks_LocateMin(workerCount, values, start, length, out TValue[] maxValues, out int[] maxIndics);

                //gogoogo
                foreach (var t in tasks)
                {
                    t.Start();
                }

                Task.WaitAll(tasks);

                //结果集中的索引和最大值
                var (maxIndex_in_MaxValues, max_in_maxValues) = LocateMin(maxValues);
                return (maxIndics[maxIndex_in_MaxValues], max_in_maxValues);

            });

        }
    }









}
