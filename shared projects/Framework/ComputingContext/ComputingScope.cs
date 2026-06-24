namespace Vorcyc.Mathematics;

/// <summary>
/// Installs an ambient <see cref="ComputingContext"/> for a lexical scope.
/// </summary>
public static class ComputingScope
{
    private static readonly AsyncLocal<ComputingContext?> Ambient = new();

    internal static ComputingContext? Current => Ambient.Value;

    /// <summary>
    /// Sets <see cref="ComputingContext.Current"/> until the returned scope is disposed.
    /// </summary>
    public static IDisposable Enter(ComputingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new Scope(context);
    }

    private sealed class Scope : IDisposable
    {
        private readonly ComputingContext? _previous;
        private bool _disposed;

        public Scope(ComputingContext context)
        {
            _previous = Ambient.Value;
            Ambient.Value = context;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Ambient.Value = _previous;
            _disposed = true;
        }
    }
}
