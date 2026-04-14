namespace Audio_example;

internal static class PitchClassifierConfig
{
    public const int TargetSampleRate = 8000;
    public const int FeatureCount = 13;
    public const int HiddenUnits = 16;
    public const int NumClasses = 2;

    public const string ClassLowDir = "class_low";
    public const string ClassHighDir = "class_high";
    public static readonly string[] ClassNames = ["low", "high"];

    public static string DefaultModelPath =>
        Path.Combine(AppContext.BaseDirectory, "models", "pitch_mfcc.vmdl");

    public static string DataRoot =>
        Path.Combine(AppContext.BaseDirectory, "data");
}
