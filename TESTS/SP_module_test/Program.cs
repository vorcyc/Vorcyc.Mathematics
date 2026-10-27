using SP_module_test;
using Vorcyc.Mathematics.Framework.Utilities;

if (args is ["spectral-011"])
{
    Spectral_011_test.Go();
    return;
}

if (args is ["sysid-011"])
{
    SysId_011_test.Go();
    return;
}

EMD_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
VMD_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
HHT_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
ExtendedModeDecomposition_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
ZLaplace_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
FFT_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Welch_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
IIR_018_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Filter_018_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Conv_019_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Tf_020_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Audit_023_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Audit_023_core_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Audit_024_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Spectral_011_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
SysId_011_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
Console.WriteLine("SP_module_test done.");
