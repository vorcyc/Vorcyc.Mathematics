using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// Provides extension methods for resampling time-domain signals and signal segments to a specified sampling rate.
/// </summary>
/// <remarks>The extension methods in this class support resampling operations for various signal types, including
/// applying optional FIR filters to prevent aliasing when downsampling. These methods return new signal instances with
/// the desired sampling rate, preserving the original signal data when the sampling rate is unchanged. Use these
/// methods to convert signals to different sampling rates for processing or analysis.</remarks>
internal static class SignalResamplingExtension
{

    extension(Signal signal)
    {

        /// <summary>
        /// Resamples the signal to the specified destination sampling rate using sinc interpolation and optional
        /// filtering.
        /// </summary>
        /// <remarks>If the destination sampling rate is equal to the original, the method returns a clone
        /// of the signal. When downsampling and no filter is provided, a default low-pass FIR filter is applied to
        /// prevent aliasing. The method uses a Hann-windowed sinc interpolation for resampling.</remarks>
        /// <param name="destnationSamplingRate">The target sampling rate, in hertz, to which the signal will be resampled. Must be positive.</param>
        /// <param name="filter">An optional FIR filter to apply before resampling, typically used for anti-aliasing when downsampling. If
        /// null and downsampling, a default low-pass filter is applied.</param>
        /// <param name="order">The interpolation order, which determines the number of samples used in the sinc interpolation. Higher
        /// values may improve accuracy but increase computation time.</param>
        /// <returns>A new Signal instance containing the resampled data at the specified sampling rate.</returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public Signal Resample(
            float destnationSamplingRate,
            FirFilter? filter = null,
            int order = 15)
        {
            if (signal.SamplingRate == destnationSamplingRate)
            {
                //var sameResult = new Signal(signal.Length, signal.SamplingRate);
                //signal.Samples.CopyTo(sameResult.Samples);
                //return sameResult;
                return signal.Clone();
            }

            var g = destnationSamplingRate / signal.SamplingRate;

            var input = signal.Samples;
            var output = new float[(int)(input.Length * g)];

            if (g < 1 && filter is null)
            {
                filter = new FirFilter(DesignFilter.FirWinLp(101, g / 2));

                input = filter.ProcessAllSamples(signal.Samples);  // filter.ApplyTo(signal).Samples;
            }

            var step = 1 / g;

            for (var n = 0; n < output.Length; n++)
            {
                var x = n * step;

                for (var i = -order; i < order; i++)
                {
                    var j = (int)Math.Floor(x) - i;

                    if (j < 0 || j >= input.Length)
                    {
                        continue;
                    }

                    var t = x - j;
                    float w = 0.5f * (1.0f + MathF.Cos(t / order * ConstantsFp32.PI));    // Hann window
                    float sinc = TrigonometryHelper.Sinc(t);                             // Sinc function
                    output[n] += w * sinc * input[j];
                }
            }

            var result = new Signal(output.Length, destnationSamplingRate);
            output.CopyTo(result.Samples);
            return result;
        }


    }


    extension(ModifiableTimeDomainSignal signal)
    {
        /// <summary>
        /// Resamples the current signal to a specified sampling rate using sinc interpolation and an optional FIR filter.
        /// </summary>
        /// <remarks>If the destination sampling rate matches the current signal's sampling rate, a clone of the original
        /// signal is returned. When downsampling, a low-pass FIR filter is applied to prevent aliasing if no filter is
        /// provided. The method uses a Hann-windowed sinc interpolation for resampling. The returned signal length is adjusted
        /// according to the new sampling rate.</remarks>
        /// <param name="destnationSamplingRate">The target sampling rate, in hertz, to which the signal will be resampled. Must be positive.</param>
        /// <param name="filter">An optional FIR filter to apply during downsampling. If null and downsampling is required, a default low-pass filter
        /// is used.</param>
        /// <param name="order">The interpolation order, which determines the number of samples used in the sinc interpolation. Higher values may
        /// improve accuracy but increase computation.</param>
        /// <returns>A new ModifiableTimeDomainSignal instance containing the resampled signal at the specified sampling rate.</returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public ModifiableTimeDomainSignal Resample(
            float destnationSamplingRate,
            FirFilter? filter = null,
            int order = 15)
        {
            if (signal.SamplingRate == destnationSamplingRate)
            {
                //var sameResult = new ModifiableTimeDomainSignal(signal.Length, signal.SamplingRate);
                //signal.Samples.CopyTo(sameResult.Samples);
                //return sameResult;
                return signal.Clone();
            }

            var g = destnationSamplingRate / signal.SamplingRate;

            var input = signal.Samples;
            var output = new float[(int)(signal.Length * g)];

            if (g < 1 && filter is null)
            {
                filter = new FirFilter(DesignFilter.FirWinLp(101, g / 2));

                var temp = filter.ProcessAllSamples(input.Span);  // filter.ApplyTo(signal).Samples;
                temp.CopyTo(input.Span);
            }

            var step = 1 / g;

            for (var n = 0; n < output.Length; n++)
            {
                var x = n * step;

                for (var i = -order; i < order; i++)
                {
                    var j = (int)Math.Floor(x) - i;

                    if (j < 0 || j >= signal.Length)
                    {
                        continue;
                    }

                    var t = x - j;
                    float w = 0.5f * (1.0f + MathF.Cos(t / order * ConstantsFp32.PI));    // Hann window
                    float sinc = TrigonometryHelper.Sinc(t);                             // Sinc function
                    output[n] += w * sinc * input.Span[j];
                }
            }

            var result = new ModifiableTimeDomainSignal(output.Length, destnationSamplingRate);
            output.CopyTo(result.Samples.Span);
            return result;
        }

    }



    extension(SignalSegment segment)
    {

        /// <summary>
        /// Resamples the signal to the specified destination sampling rate using sinc interpolation and optional
        /// filtering.
        /// </summary>
        /// <remarks>If the destination sampling rate matches the original, the method returns a decoupled
        /// copy of the signal. When downsampling and no filter is provided, a default low-pass FIR filter is applied to
        /// reduce aliasing. The interpolation uses a Hann-windowed sinc function for each output sample.</remarks>
        /// <param name="destnationSamplingRate">The target sampling rate, in hertz, to which the signal will be resampled.</param>
        /// <param name="filter">An optional FIR filter to apply before resampling. If null and downsampling, a default low-pass filter is
        /// used to prevent aliasing.</param>
        /// <param name="order">The interpolation order, which determines the number of samples considered for each output value. Higher
        /// values may improve accuracy but increase computation time.</param>
        /// <returns>A new Signal instance containing the resampled data at the specified sampling rate.</returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public Signal Resample(
            float destnationSamplingRate,
            FirFilter? filter = null,
            int order = 15)
        {
            if (segment.Signal.SamplingRate == destnationSamplingRate)
            {
                return segment.Decouple();
            }

            var g = destnationSamplingRate / segment.Signal.SamplingRate;

            var input = segment.Samples;
            var output = new float[(int)(input.Length * g)];

            if (g < 1 && filter is null)
            {
                filter = new FirFilter(DesignFilter.FirWinLp(101, g / 2));

                input = filter.ProcessAllSamples(segment.Samples);  // filter.ApplyTo(signal).Samples;
            }

            var step = 1 / g;

            for (var n = 0; n < output.Length; n++)
            {
                var x = n * step;

                for (var i = -order; i < order; i++)
                {
                    var j = (int)Math.Floor(x) - i;

                    if (j < 0 || j >= input.Length)
                    {
                        continue;
                    }

                    var t = x - j;
                    float w = 0.5f * (1.0f + MathF.Cos(t / order * ConstantsFp32.PI));    // Hann window
                    float sinc = TrigonometryHelper.Sinc(t);                             // Sinc function
                    output[n] += w * sinc * input[j];
                }
            }

            var result = new Signal(output.Length, destnationSamplingRate);
            output.CopyTo(result.Samples);
            return result;
        }
    }


}
