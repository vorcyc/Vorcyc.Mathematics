using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Online filter that processes one sample of type <typeparamref name="T"/>.
/// The non-generic <see cref="IOnlineFilter"/> remains the float contract used by
/// <see cref="LtiFilter"/> and the existing FDA / effect stack.
/// </summary>
public interface IOnlineFilter<T>
    where T : unmanaged, IFloatingPointIeee754<T>
{
    /// <summary>Processes one sample.</summary>
    T Process(T sample);

    /// <summary>Clears delay state.</summary>
    void Reset();
}
