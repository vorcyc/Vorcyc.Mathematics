using System.Drawing;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.Statistics;

var x = new PinnableArray<float>(100, false);
var y = new PinnableArray<float>(100, false);
x.FillWithRandomNumber();
y.FillWithRandomNumber();


var r = Vorcyc.Mathematics.Statistics.SimpleLinearRegression.ComputeParameters<float>(x, y);

Console.WriteLine(r);
Console.WriteLine("-------------");


var r2 = Vorcyc.Mathematics.Statistics.another_SimpleLinearRegression.ComputeParameters(x, y);
Console.WriteLine(  r2);

