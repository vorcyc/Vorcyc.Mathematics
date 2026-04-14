using Audio_example.Io;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Audio_example.Pipeline;

internal sealed class LabeledWavDataset
{
    public required IReadOnlyList<Signal> Signals { get; init; }
    public required IReadOnlyList<int> Labels { get; init; }
    public required IReadOnlyList<string> FilePaths { get; init; }

    public static LabeledWavDataset LoadFromFolders(string dataRoot)
    {
        var signals = new List<Signal>();
        var labels = new List<int>();
        var paths = new List<string>();

        for (int classIndex = 0; classIndex < PitchClassifierConfig.ClassNames.Length; classIndex++)
        {
            string folderName = classIndex == 0
                ? PitchClassifierConfig.ClassLowDir
                : PitchClassifierConfig.ClassHighDir;
            string classDir = Path.Combine(dataRoot, folderName);

            if (!Directory.Exists(classDir))
            {
                throw new DirectoryNotFoundException($"Missing class folder: {classDir}");
            }

            foreach (string wav in Directory.EnumerateFiles(classDir, "*.wav").OrderBy(Path.GetFileName))
            {
                signals.Add(WavBridge.ReadAsSignal(wav, PitchClassifierConfig.TargetSampleRate));
                labels.Add(classIndex);
                paths.Add(wav);
            }
        }

        if (signals.Count < 2)
        {
            throw new InvalidOperationException("Need at least two WAV files (one per class). Run `prepare` first.");
        }

        return new LabeledWavDataset
        {
            Signals = signals,
            Labels = labels,
            FilePaths = paths,
        };
    }
}
