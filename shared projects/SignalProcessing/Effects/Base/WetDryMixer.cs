namespace Vorcyc.Mathematics.SignalProcessing.Effects.Base
{
    /// <summary>
    /// Base class implementing wet/dry mixing logic.
    /// </summary>
    public class WetDryMixer : IMixable
    {
        /// <summary>
        /// Gets or sets wet gain (by default, 1).
        /// </summary>
        public float Wet { get; set; } = 1f;

        /// <summary>
        /// Gets or sets dry gain (by default, 0).
        /// </summary>
        public float Dry { get; set; } = 0f;

        /// <summary>
        /// Sets wet/dry mix (in range [0..1]).
        /// </summary>
        /// <param name="mix">Wet/dry mix</param>
        /// <param name="mixingRule">Mixing rule</param>
        public void WetDryMix(float mix, MixingRule mixingRule = MixingRule.Linear)
        {
            if (mix < 0f)
            {
                mix = 0;
            }

            if (mix > 1f)
            {
                mix = 1;
            }

            switch (mixingRule)
            {
                case MixingRule.Balanced:
                    {
                        Dry = 2 * Math.Min(0.5f, 1 - mix);
                        Wet = 2 * Math.Min(0.5f, mix);
                        break;
                    }

                case MixingRule.Sin3Db:
                    {
                        Dry = MathF.Cos(0.5f * ConstantsFp32.PI * mix);
                        Wet = MathF.Sin(0.5f * ConstantsFp32.PI * mix);
                        break;
                    }

                case MixingRule.Sin4_5Db:
                    {
                        Dry = MathF.Pow(MathF.Cos(0.5f * ConstantsFp32.PI * mix), 1.5f);
                        Wet = MathF.Pow(MathF.Sin(0.5f * ConstantsFp32.PI * mix), 1.5f);
                        break;
                    }

                case MixingRule.Sin6Db:
                    {
                        Dry = MathF.Pow(MathF.Cos(0.5f * ConstantsFp32.PI * mix), 2.0f);
                        Wet = MathF.Pow(MathF.Sin(0.5f * ConstantsFp32.PI * mix), 2.0f);
                        break;
                    }

                case MixingRule.SqRoot3Db:
                    {
                        Dry = MathF.Sqrt(1 - mix);
                        Wet = MathF.Sqrt(mix);
                        break;
                    }

                case MixingRule.SqRoot4_5Db:
                    {
                        Dry = MathF.Pow(1 - mix, 1.5f);
                        Wet = MathF.Pow(mix, 1.5f);
                        break;
                    }

                case MixingRule.Linear:
                default:
                    {
                        Dry = 1 - mix;
                        Wet = mix;
                        break;
                    }
            }
        }

        /// <summary>
        /// Sets wet/dry gains in decibels and applies linear mix rule.
        /// </summary>
        /// <param name="wetDb">Wet gain in decibels</param>
        /// <param name="dryDb">Dry gain in decibels</param>
        public void WetDryDb(float wetDb, float dryDb)
        {
            var w = MathF.Pow(10, wetDb / 20);
            var d = MathF.Pow(10, dryDb / 20);

            var mix = w / (w + d);

            WetDryMix(mix, MixingRule.Linear);
        }
    }
}
