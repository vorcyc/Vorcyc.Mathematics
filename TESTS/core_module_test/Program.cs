

using System.Numerics;
using System.Security.AccessControl;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Helpers;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.Statistics;


PinnableArray<float> a = new(100, false);
a.Fill(0, 1f);


Console.WriteLine(a.ToString());

var max= a.AsSpan(50,10).MaxMin();
Console.WriteLine(  max);



var x = new float[100];
for (int i = 0; i < x.Length; i++)
    x[i] = i;

var seg=new ArraySegment<float>(x,10,10);

Console.WriteLine(seg.MaxMin());