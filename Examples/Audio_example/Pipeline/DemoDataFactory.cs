using Audio_example.Io;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Audio_example.Pipeline;

internal static class DemoDataFactory
{
    static readonly (string Folder, float[] FrequenciesHz)[] Clips =
    [
        (PitchClassifierConfig.ClassLowDir, [180f, 220f, 260f]),
        (PitchClassifierConfig.ClassHighDir, [900f, 1200f, 1500f]),
    ];

    public static string Prepare(string dataRoot)
    {
        Directory.CreateDirectory(dataRoot);

        int written = 0;
        const float durationSec = 0.6f;
        int sampleCount = (int)(PitchClassifierConfig.TargetSampleRate * durationSec);

        foreach (var (folder, frequencies) in Clips)
        {
            string classDir = Path.Combine(dataRoot, folder);
            Directory.CreateDirectory(classDir);

            foreach (float hz in frequencies)
            {
                var signal = new Signal(sampleCount, PitchClassifierConfig.TargetSampleRate);
                signal.GenerateWave(WaveShape.Sine, hz, Behaviour.Replace);
                signal.NormalizeMax();

                string fileName = $"tone_{hz:0}hz.wav";
                string path = Path.Combine(classDir, fileName);
                WavBridge.WritePcm16(path, signal);
                written++;
            }
        }

        return dataRoot;
    }

    public static int CountPreparedFiles(string dataRoot)
    {
        if (!Directory.Exists(dataRoot))
        {
            return 0;
        }

        return Directory
            .EnumerateFiles(dataRoot, "*.wav", SearchOption.AllDirectories)
            .Count();
    }
}
