using System.IO;

namespace AnalyzersNet.Configuration;

/// <summary>Represents (global) analyzer configuration.</summary>
[DebuggerDisplay("IsGlobal = {IsGlobal}, Count = {Count}")]
[DebuggerTypeProxy(typeof(Diagnostics.CollectionDebugView))]
public sealed class AnalyzerConfiguration : IReadOnlyCollection<AnalyzerConfigSeverityEntry>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Dictionary<DiagnosticId, AnalyzerConfigSeverityEntry> Entries;

    /// <summary>Initializes a new instance of the <see cref="AnalyzerConfiguration"/> class.</summary>
    private AnalyzerConfiguration(Dictionary<DiagnosticId, AnalyzerConfigSeverityEntry> lookup) => Entries = lookup;

    /// <inheritdoc />
    public int Count => Entries.Count;

    /// <summary>Indicates if this should be the top level entry (default is true).</summary>
    public bool IsGlobal { get; init; } = true;

    /// <summary>Creates a new analyzer configuration applying the specified overrides.</summary>
    [Pure]
    public AnalyzerConfiguration Override(params AnalyzerConfigSeverityEntry[] overrides)
    {
        var updated = new AnalyzerConfiguration(new(Entries)) { IsGlobal = IsGlobal };

        foreach (var o in overrides)
        {
            if (Entries.TryGetValue(o.Id, out var existing))
            {
                if (o.Severity != existing.Severity || o.Justification is { Length: > 0 })
                {
                    updated.Entries[o.Id] = existing with { Severity = o.Severity, Justification = o.Justification, IsOverride = true };
                }
            }
            else
            {
                updated.Entries[o.Id] = o with { IsOverride = true };
            }
        }
        return updated;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<AnalyzerConfigSeverityEntry> GetEnumerator() => Entries.Values
        .OrderBy(x => x)
        .GetEnumerator();

    /// <inheritdoc />
    [Pure]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="Save(TextWriter, bool)" />
    public void Save(FileInfo file, bool includeDefaults = false)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var writer = new StreamWriter(file.FullName, new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Create });
        Save(writer, includeDefaults);
    }

    /// <inheritdoc cref="Save(TextWriter, bool)" />
    public void Save(Stream stream, bool includeDefaults = false)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var writer = new StreamWriter(stream);
        Save(writer, includeDefaults);
    }

    /// <summary>Save the analyzer configuration.</summary>
    public void Save(TextWriter writer, bool includeDefaults = false)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (IsGlobal)
        {
            writer.WriteLine("# Top level entry required to mark this as a global AnalyzerConfig file");
            writer.WriteLine("is_global = true");
        }
        else
        {
            writer.WriteLine("is_global = false");
        }
        writer.WriteLine();

        writer.WriteLine("# .NET diagnostics overrides");
        AnalyzerConfigSeverityEntry? prev = null;

        foreach (var d in this.TakeWhile(d => d.IsOverride || includeDefaults))
        {
            if (prev is { })
            {
                if (prev.IsOverride != d.IsOverride)
                {
                    writer.WriteLine();
                    writer.WriteLine("# .NET diagnostics defaults");
                }
                else if (prev.Id.Prefix != d.Id.Prefix || prev.Severity != d.Severity)
                {
                    writer.WriteLine();
                }
            }
            writer.WriteLine(d);
            prev = d;
        }
    }

    /// <summary>Creates new analyzer configuration based on <see cref="DiagnosticAnalyzerInfo"/>.</summary>
    [Pure]
    public static AnalyzerConfiguration New(bool isGlobal, IEnumerable<DiagnosticAnalyzerInfo> diagnostics)
    {
        var ecd = new AnalyzerConfiguration([]) { IsGlobal = isGlobal };

        foreach (var diagnostic in diagnostics)
        {
            ecd.Entries[diagnostic.Id] = AnalyzerConfigSeverityEntry.From(diagnostic);
        }

        return ecd;
    }
}
