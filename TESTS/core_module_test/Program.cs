using System.Drawing;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Numerics;
using Vorcyc.Mathematics.Statistics;

var x = new PinnableArray<float>(100, false);
var y = new PinnableArray<float>(100, false);
x.FillWithRandomNumber();
y.FillWithRandomNumber();


var r = Vorcyc.Mathematics.Statistics.SimpleLinearRegression.ComputeParameters(x, y);

//Console.WriteLine(  x.ToString());
//Console.WriteLine(  y.ToString());

Console.WriteLine(r);
Console.WriteLine(SimpleLinearRegression.GetX(50, r));
Console.WriteLine("-------------");


var r2 = Vorcyc.Mathematics.Statistics.another_SimpleLinearRegression.ComputeParameters(x, y);
Console.WriteLine(  r2);




public class LinearRegression
{
    public double Slope { get; private set; }
    public double Intercept { get; private set; }

    public LinearRegression(PointF[] data)
    {
        if (data == null || data.Length < 2)
        {
            throw new ArgumentException("Data cannot be null or the number of data points must be at least 2.");
        }

        double sumX = 0;
        double sumY = 0;
        double sumXY = 0;
        double sumX2 = 0;
        int n = data.Length;

        foreach (var point in data)
        {
            sumX += point.X;
            sumY += point.Y;
            sumXY += point.X * point.Y;
            sumX2 += point.X * point.X;
        }

        Slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        Intercept = (sumY - Slope * sumX) / n;
    }

    public double GetY(double x)
    {
        return Slope * x + Intercept;
    }

    public double GetX(double y)
    {
        return (y - Intercept) / Slope;
    }
}

