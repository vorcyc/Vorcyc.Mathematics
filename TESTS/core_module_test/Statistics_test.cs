using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Buffers;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.Statistics;

namespace core_module_test;
internal class Statistics_test
{
    public static void go()
    {
        //Span<float> values = stackalloc float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        for (int i = 0; i < 10; i++)
        {
            //var values = new float[Random.Shared.Next(50,5000000)];
            var size = Random.Shared.Next(50, 5000000);
            var values = new PinnableArray<float>(size, false);
            values.Span.FillWithRandomNumber();
            $"length : {values.Values.Length}".PrintLine(ConsoleColor.Green);
            var average = values.Span.Variance<float>();
            average.PrintLine();
            $"length : {values.Span.Length}".PrintLine(ConsoleColor.Green);
            average = values.Span.Variance();
            average.PrintLine();
            "----------".PrintLine(ConsoleColor.Red);
        }
    }
    public static void go2()
    {
        for (int i = 0; i < 20; i++)
        {
            var a = new float[1000];
            a.FillWithRandomNumber();
            Vorcyc.Mathematics.Statistics.Basic.CalculateAllStatistics<float>(a).PrintLine();
            //Statistics.CalculateAllStatistics_SIMD<float>(a).PrintLine();
            new string('-', 50).PrintLine(ConsoleColor.Red);
        }
    }
}
