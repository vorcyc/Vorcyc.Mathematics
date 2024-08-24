
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.Statistics;

var r = Vorcyc.Mathematics.NumberMapper.Map<float>(5, 0, 10, 0, 100);
Console.WriteLine(r);



var array = new float[10];
for (int i = 0; i < 10; i++)
    array[i] = i;

var max = Vorcyc.Mathematics.Statistics.SBasic.Max(array);

Console.WriteLine(max);

