using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Base;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Options;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi;

/// <summary>
/// Represents extractor of time-domain features (energy, rms, ZCR, entropy).
/// </summary>
public class TimeDomainFeaturesExtractor : FeatureExtractor
{
    /// <summary>
    /// Full set of features.
    /// </summary>
    public const string FeatureSet = "energy, rms, zcr, entropy";

    /// <summary>
    /// Gets string annotations (or simply names) of features.
    /// </summary>
    public override List<string> FeatureDescriptions { get; }

    /// <summary>
    /// Per-frame extractors operating on <see cref="SignalSegment"/> views.
    /// </summary>
    protected List<Func<SignalSegment, float>> _segmentExtractors;

    /// <summary>
    /// Parameters.
    /// </summary>
    protected readonly Dictionary<string, object> _parameters;

    /// <summary>
    /// Constructs extractor from configuration <paramref name="options"/>.
    /// </summary>
    public TimeDomainFeaturesExtractor(MultiFeatureOptions options) : base(options)
    {
        var featureList = options.FeatureList;

        if (featureList == "all" || featureList == "full")
        {
            featureList = FeatureSet;
        }

        var features = featureList.Split(',', '+', '-', ';', ':')
                                  .Select(f => f.Trim().ToLower())
                                  .ToList();

        _parameters = options.Parameters;
        _segmentExtractors = features.Select<string, Func<SignalSegment, float>>(feature =>
        {
            return feature switch
            {
                "e" or "en" or "energy" => frame => frame.AverageEnergy,
                "rms" => frame => frame.Rms,
                "zcr" or "zero-crossing-rate" => frame => frame.ZeroCrossingRate,
                "entropy" => frame => frame.Entropy,
                _ => _ => 0
            };
        }).ToList();

        FeatureCount = features.Count;
        FeatureDescriptions = features;
    }

    /// <summary>
    /// Adds a user-defined feature based on a <see cref="SignalSegment"/> frame view.
    /// </summary>
    public void AddFeature(string name, Func<SignalSegment, float> algorithm)
    {
        FeatureCount++;
        FeatureDescriptions.Add(name);
        _segmentExtractors.Add(algorithm);
    }

    /// <summary>
    /// Adds a user-defined feature based on a sample span (applied to each frame segment).
    /// </summary>
    public void AddFeature(string name, Func<ReadOnlySpan<float>, int, int, float> algorithm)
    {
        AddFeature(name, frame => algorithm(frame.Samples, 0, frame.Length));
    }

    /// <inheritdoc />
    public override int ComputeFrom(Signal signal, int startSample, int endSample, IList<float[]> vectors)
    {
        var fv = 0;

        for (var sample = startSample; sample + FrameSize < endSample; sample += HopSize, fv++)
        {
            var frame = signal[sample, FrameSize, throwException: true]!.Value;
            var featureVector = vectors[fv];

            for (var j = 0; j < featureVector.Length; j++)
            {
                featureVector[j] = _segmentExtractors[j](frame);
            }
        }

        return fv;
    }

    /// <inheritdoc />
    public override int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)
    {
        var signal = Signal.FromCopy(samples, SamplingRate);
        return ComputeFrom(signal, startSample, endSample, vectors);
    }

    /// <summary>
    /// <para>Processes one frame in block of data at each step.</para>
    /// <para><see cref="TimeDomainFeaturesExtractor"/> does not provide this function.</para>
    /// </summary>
    public override void ProcessFrame(float[] block, float[] features)
    {
        throw new NotImplementedException("TimeDomainFeaturesExtractor does not provide this function. Please call ComputeFrom() method");
    }

    /// <summary>
    /// Returns true, since <see cref="TimeDomainFeaturesExtractor"/> always supports parallelization.
    /// </summary>
    public override bool IsParallelizable() => true;

    /// <summary>
    /// Creates thread-safe copy of the extractor for parallel computations.
    /// </summary>
    public override FeatureExtractor ParallelCopy()
    {
        var options = new MultiFeatureOptions
        {
            SamplingRate = SamplingRate,
            FrameDuration = FrameDuration,
            HopDuration = HopDuration,
            FeatureList = string.Join(",", FeatureDescriptions),
            Parameters = _parameters
        };

        return new TimeDomainFeaturesExtractor(options)
        {
            _segmentExtractors = _segmentExtractors
        };
    }
}
