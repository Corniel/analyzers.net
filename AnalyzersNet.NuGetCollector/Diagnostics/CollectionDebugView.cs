#pragma warning disable S2365 // Properties should not make collection or array copies
// but here, it is the only purpose of this debug view.

namespace AnalyzersNet.NuGetCollector.Diagnostics;

/// <summary>Allows the debugger to display collections.</summary>
[Mutable]
[ExcludeFromCodeCoverage(Justification = "Debug experience only, challenging to test.")]
public sealed class CollectionDebugView(IEnumerable enumeration)
{
    /// <summary>The array that is shown by the debugger.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object[] Items => Enumeration.Cast<object>().ToArray();

    /// <summary>A reference to the enumeration to display.</summary>
    private readonly IEnumerable Enumeration = enumeration;
}
