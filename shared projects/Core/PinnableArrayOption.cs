namespace Vorcyc.Mathematics;

/// <summary>
/// The options for <see cref="PinnableArray{T}"/>.
/// </summary>
public class PinnableArrayOption
{
    internal PinnableArrayOption() { }

    /// <summary>
    /// Gets or sets if using leasing mode.
    /// </summary>
    public bool UseLeasingMode { get; set; } = false;

}

