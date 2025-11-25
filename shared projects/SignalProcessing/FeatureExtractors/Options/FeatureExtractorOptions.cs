using System.Runtime.Serialization;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options
{
    /// <summary>
    /// Defines basic properties for configuring feature extractors. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>FrameDuration = 0.025</item>
    ///     <item>HopDuration = 0.01</item>
    ///     <item>Window = WindowType.Rectangular</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class FeatureExtractorOptions
    {
        /// <summary>
        /// Gets or sets number of features to extract (feature vector size).
        /// </summary>
        [DataMember]
        public int FeatureCount { get; set; }

        /// <summary>
        /// Gets or sets expected sampling rate of signals for analysis.
        /// </summary>
        [DataMember]
        public int SamplingRate { get; set; }

        /// <summary>
        /// Gets or sets length of analysis frame (duration in seconds). By default, 0.025 (25ms).
        /// </summary>
        [DataMember]
        public float FrameDuration { get; set; } = 0.025f;/*seconds*/

        /// <summary>
        /// Gets or sets hop length (duration in seconds). By default, 0.01 (10ms).
        /// </summary>
        [DataMember]
        public float HopDuration { get; set; } = 0.01f;/*seconds*/

        /// <summary>
        /// Gets or sets size of analysis frame (number of samples). Has priority over <see cref="FrameDuration"/>. 
        /// If it is not specified explicitly, then it is auto-computed from <see cref="FrameDuration"/>.
        /// </summary>
        [DataMember]
        public int FrameSize { get; set; } = 0;

        /// <summary>
        /// Gets or sets hop size (number of samples). Has priority over <see cref="HopDuration"/>. 
        /// If it is not specified explicitly, then it is auto-computed from <see cref="HopDuration"/>.
        /// </summary>
        [DataMember]
        public int HopSize { get; set; } = 0;

        /// <summary>
        /// Gets or sets pre-emphasis filter coefficient.
        /// </summary>
        [DataMember]
        public float PreEmphasis { get; set; } = 0;

        /// <summary>
        /// Gets or sets window function (by default, rectangular window, i.e. no windowing). 
        /// In <see cref="FilterbankOptions"/> and its subclasses (MFCC, PLP, etc.) default window is Hamming.
        /// </summary>
        [DataMember]
        public WindowType Window { get; set; } = WindowType.Rectangular;

        /// <summary>
        /// Returns the list of error messages describing particular configuration validation problem.
        /// </summary>
        public virtual List<string> Errors
        {
            get
            {
                var errors = new List<string>();

                if (SamplingRate <= 0)
                {
                    errors.Add("Positive sampling rate must be specified");
                }

                if (FrameDuration <= 0 && FrameSize <= 0)
                {
                    errors.Add("Positive frame duration (in seconds) or frame size (in samples) must be specified");
                }

                if (HopDuration <= 0 && HopSize <= 0)
                {
                    errors.Add("Positive hop duration (in seconds) or hop size (in samples) must be specified");
                }

                return errors;
            }
        }
    }
}
