namespace Vorcyc.Mathematics.SignalProcessing.Identification;

/// <summary>
/// Single-input, single-output identification session: output, optional input, and sample time.
/// </summary>
public sealed class IdData
{
    public IdData(float[] output, float[]? input, float sampleTime)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (output.Length == 0)
            throw new ArgumentException("The output record is empty.", nameof(output));
        if (sampleTime <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleTime));
        if (input != null && input.Length != output.Length)
            throw new ArgumentException("Input and output must have the same length.");

        Output = (float[])output.Clone();
        Input = input == null ? null : (float[])input.Clone();
        SampleTime = sampleTime;
    }

    public float[] Output { get; }

    public float[]? Input { get; }

    public float SampleTime { get; }

    public int Samples => Output.Length;

    public static IdData TimeSeries(float[] output, float sampleTime = 1f)
        => new(output, input: null, sampleTime);

    public static IdData FromInputOutput(float[] input, float[] output, float sampleTime = 1f)
        => new(output, input, sampleTime);
}
