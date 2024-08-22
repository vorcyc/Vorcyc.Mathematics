namespace Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

/// <summary>
/// Represents stereo panning audio effect.
/// </summary>
public class PanEffect : StereoEffect
{
    /// <summary>
    /// Pan value (must be in range [-1..1]).
    /// </summary>
    protected float _pan;

    /// <summary>
    /// Pan value for calculations (in range [0..1]).
    /// </summary>
    protected float _mappedPan;

    /// <summary>
    /// Pan value for calculations in constant-power mode (in range [0..1]).
    /// </summary>
    protected float _constantPowerPan;

    /// <summary>
    /// Gets or sets pan (in range [-1..1]).
    /// </summary>
    public float Pan
    {
        get => _pan;
        set
        {
            _pan = value;
            if (_pan > 1) _pan = 1;
            if (_pan < -1) _pan = -1;

            _mappedPan = (_pan + 1) / 2;
            _constantPowerPan = ConstantsFp32.PI * (_pan + 1) / 4;
        }
    }

    /// <summary>
    /// Gets or sets pan rule (pan law).
    /// </summary>
    public PanRule PanRule { get; set; }

    /// <summary>
    /// Constructs <see cref="PanEffect"/>.
    /// </summary>
    /// <param name="pan">Pan</param>
    /// <param name="panRule">Pan rule (pan law)</param>
    public PanEffect(float pan, PanRule panRule)
    {
        Pan = pan;
        PanRule = panRule;
    }

    /// <summary>
    /// Processes one sample in each of two channels : [ input left , input right ] -> [ output left , output right ].
    /// </summary>
    /// <param name="left">Input sample in left channel</param>
    /// <param name="right">Input sample in right channel</param>
    public override void Process(ref float left, ref float right)
    {
        var leftIn = left;
        var rightIn = right;

        switch (PanRule)
        {
            case PanRule.Balanced:
                {
                    left *= 2 * Math.Min(0.5f, 1 - _mappedPan);
                    right *= 2 * Math.Min(0.5f, _mappedPan);
                    break;
                }

            case PanRule.ConstantPower:
                {
                    left *= MathF.Cos(_constantPowerPan);
                    right *= MathF.Sin(_constantPowerPan);
                    break;
                }

            case PanRule.Sin3Db:
                {
                    var gain = MathF.Sqrt(2.0f);
                    left *= gain * MathF.Cos(0.5f * ConstantsFp32.PI * _mappedPan);
                    right *= gain * MathF.Sin(0.5f * ConstantsFp32.PI * _mappedPan);
                    break;
                }

            case PanRule.Sin4_5Db:
                {
                    var gain = MathF.Pow(2, 3f / 4.0f);
                    left *= gain * MathF.Pow(MathF.Cos(0.5f * ConstantsFp32.PI * _mappedPan), 1.5f);
                    right *= gain * MathF.Pow(MathF.Sin(0.5f * ConstantsFp32.PI * _mappedPan), 1.5f);
                    break;
                }

            case PanRule.Sin6Db:
                {
                    var gain = 2.0f;
                    left *= gain * MathF.Pow(MathF.Cos(0.5f * ConstantsFp32.PI * _mappedPan), 2.0f);
                    right *= gain * MathF.Pow(MathF.Sin(0.5f * ConstantsFp32.PI * _mappedPan), 2.0f);
                    break;
                }

            case PanRule.SqRoot3Db:
                {
                    var gain = MathF.Sqrt(2.0f);
                    left *= gain * MathF.Sqrt(1 - _mappedPan);
                    right *= gain * MathF.Sqrt(_mappedPan);
                    break;
                }

            case PanRule.SqRoot4_5Db:
                {
                    var gain = MathF.Pow(2, 3 / 4.0f);
                    left *= gain * MathF.Pow(1 - _mappedPan, 1.5f);
                    right *= gain * MathF.Pow(_mappedPan, 1.5f);
                    break;
                }

            case PanRule.Linear:
            default:
                {
                    left *= 2 * (1 - _mappedPan);
                    right *= 2 * _mappedPan;
                    break;
                }
        }

        left = leftIn * Dry + left * Wet;
        right = rightIn * Dry + right * Wet;
    }

    /// <summary>
    /// Resets effect.
    /// </summary>
    public override void Reset()
    {
    }
}
