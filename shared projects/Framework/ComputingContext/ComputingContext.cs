namespace Vorcyc.Mathematics;

/// <summary>
/// Lightweight execution policy for numerical operations (Phase 1: CPU only).
/// </summary>
/// <remarks>
/// Resolution order when an API accepts <c>ComputingContext?</c>:
/// explicit parameter, then <see cref="Current"/>, then <see cref="Default"/>.
/// </remarks>
public sealed class ComputingContext
{
    /// <summary>Process-wide default policy.</summary>
    public static ComputingContext Default { get; private set; } = Create(CpuExecutionMode.Auto);

    /// <summary>Ambient context for the current async flow, if any.</summary>
    public static ComputingContext? Current => ComputingScope.Current;

    /// <summary>Scalar CPU execution.</summary>
    public static ComputingContext Normal { get; } = Create(CpuExecutionMode.Normal);

    /// <summary>SIMD CPU execution.</summary>
    public static ComputingContext Simd { get; } = Create(CpuExecutionMode.Simd);

    /// <summary>Parallel CPU execution.</summary>
    public static ComputingContext Parallel { get; } = Create(CpuExecutionMode.Parallel);

    /// <summary>Heuristic CPU execution.</summary>
    public static ComputingContext Auto { get; } = Create(CpuExecutionMode.Auto);

    private ComputingContext(CpuExecutionMode cpuMode, int? maxParallelism)
    {
        CpuMode = cpuMode;
        MaxParallelism = maxParallelism;
    }

    /// <summary>Gets the CPU execution mode.</summary>
    public CpuExecutionMode CpuMode { get; }

    /// <summary>Gets an optional parallel worker cap.</summary>
    public int? MaxParallelism { get; }

    /// <summary>Creates a CPU execution policy.</summary>
    public static ComputingContext Create(
        CpuExecutionMode cpuMode = CpuExecutionMode.Auto,
        int? maxParallelism = null)
    {
        if (maxParallelism is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxParallelism), "Must be positive when specified.");
        }

        return new ComputingContext(cpuMode, maxParallelism);
    }

    /// <summary>Replaces the process-wide default policy.</summary>
    public static void ConfigureDefault(ComputingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Default = context;
    }

    /// <summary>
    /// Resolves an effective CPU mode, expanding <see cref="CpuExecutionMode.Auto"/> when <paramref name="problemSize"/> is known.
    /// </summary>
    public CpuExecutionMode ResolveCpuMode(int? problemSize = null)
    {
        if (CpuMode != CpuExecutionMode.Auto)
        {
            return CpuMode;
        }

        if (problemSize is null or < 0)
        {
            return CpuExecutionMode.Normal;
        }

        if (problemSize >= 16_384)
        {
            return CpuExecutionMode.Parallel;
        }

        if (problemSize >= 1_024)
        {
            return CpuExecutionMode.Simd;
        }

        return CpuExecutionMode.Normal;
    }

    /// <summary>
    /// Resolves the context used by an API call.
    /// </summary>
    public static ComputingContext Resolve(ComputingContext? context)
        => context ?? Current ?? Default;
}
