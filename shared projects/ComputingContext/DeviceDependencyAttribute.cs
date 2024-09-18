namespace Vorcyc.Mathematics;


/// <summary>
/// Specifies the device dependency for a method.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DeviceDependencyAttribute"/> class.
/// </remarks>
/// <param name="device">The device on which the method depends.</param>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class DeviceDependencyAttribute(DeviceDependency device) : Attribute
{

    /// <summary>
    /// Gets the device on which the method depends.
    /// </summary>
    public DeviceDependency Device { get; } = device;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => Device.ToString();
}

/// <summary>
/// Defines the types of devices that a method can depend on.
/// </summary>
internal enum DeviceDependency
{
    /// <summary>
    /// Central Processing Unit.
    /// </summary>
    CPU,

    /// <summary>
    /// Graphics Processing Unit.
    /// </summary>
    GPU,

    /// <summary>
    /// Field-Programmable Gate Array.
    /// </summary>
    FPGA,

    /// <summary>
    /// Tensor Processing Unit.
    /// </summary>
    TPU
}