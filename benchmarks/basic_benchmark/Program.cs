
using basic_benchmark;
using BenchmarkDotNet.Running;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using Vorcyc.Mathematics;


//BenchmarkRunner.Run<ExtremeValueFinder_benchmark>();

BenchmarkRunner.Run<SimpleLinearRegression_benchmark>();

