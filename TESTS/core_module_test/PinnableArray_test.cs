using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Buffers;
namespace core_module_test;
internal class PinnableArray_test
{
    public static void go()
    {
        {
            PinnableArray<float>.Options.UseLeasingMode = true;
            PinnableArray<float> a = new(1000);
            a.Span.FillWithRandomNumber();
            Console.WriteLine(a.ToString());
            Console.WriteLine(a.Values.Length);
            Console.WriteLine(a.Length);
        }

    }
}
