
using basic_benchmark;
using BenchmarkDotNet.Running;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using Vorcyc.Mathematics;




//var array = new float[1000000];
////for (int i = 0; i < array.Length; i++)
////    array[i] = i;
//array.FillWithRandomNumber();

//var r = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Normal<float>(array);
//Console.WriteLine(r);


//var r2 = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector128(array);
//Console.WriteLine(r2);


//var r3 = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector256(array);
//Console.WriteLine(r3);


//var rx1 = another_extremeValueFinder.FindExtremeValue_Vector128(array);
//Console.WriteLine(rx1);

//var rx2 = another_extremeValueFinder.FindExtremeValue_Vector256(array);
//Console.WriteLine(rx2);

//var r4 = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector512(array);
//Console.WriteLine(r4);

BenchmarkRunner.Run<ExtremeValueFinder_benchmark>();

