using ML_module_test;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Framework.Utilities;

// 单独跑大 N SIMD：dotnet run --project TESTS/ML_module_test -- simd-large-n
if (args is ["simd-large-n"])
{
    CurveFitting_SimdLargeN_test.Go();
    return;
}

if (args is ["cancel"])
{
    CurveFitting_Cancel_test.Go();
    return;
}

//SVM_test.text_go();

//GMM_test.go();
Regression_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
CurveFitting_Sinusoidal_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
CurveFitting_Cancel_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
CurveFitting_SimdLargeN_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
KNN_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
LogisticRegression_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
ExtendedML_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
ClassificationSuite_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
InfrastructureSuite_test.Go();
new string('-', 30).PrintLine(ConsoleColor.Green);
GprAndIsolationForest_test.Go();
