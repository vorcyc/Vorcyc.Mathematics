using Vorcyc.Mathematics;
using Vorcyc.Mathematics.LanguageExtension;

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
            var values = new PinnableArray<double>(size, false);
            values.FillWithRandomNumber();


            $"length : {values.Values.Length}".PrintLine(ConsoleColor.Green);
            var average = values.AsSpan().Variance<double>();
            average.PrintLine();

            $"length : {values.AsSpan().Length}".PrintLine(ConsoleColor.Green);
            average = values.AsSpan().Variance();
            average.PrintLine();

            "----------".PrintLine(ConsoleColor.Red);
        }


    }
}
