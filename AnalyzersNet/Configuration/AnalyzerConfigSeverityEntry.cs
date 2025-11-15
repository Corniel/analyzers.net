using System.Text;

namespace AnalyzersNet.Configuration;

/// <summary>Represents an entry that sets the dotnet_diagnostic severity.</summary>
public sealed record AnalyzerConfigSeverityEntry() : IComparable<AnalyzerConfigSeverityEntry>
{
    /// <summary>Initializes a new instance of the <see cref="AnalyzerConfigSeverityEntry"/> class.</summary>
    public AnalyzerConfigSeverityEntry(string id, AnalyzerConfigSeverity severity, string? justification = null)
        : this(DiagnosticId.Parse(id), severity, justification) { }

    /// <summary>Initializes a new instance of the <see cref="AnalyzerConfigSeverityEntry"/> class.</summary>
    public AnalyzerConfigSeverityEntry(DiagnosticId id, AnalyzerConfigSeverity severity, string? justification = null) : this()
    {
        Id = id;
        Severity = severity;
        Justification = justification;
    }

    /// <inheritdoc cref="DiagnosticDescriptor.Id"/>
    public DiagnosticId Id { get; init; }

    /// <summary>The severity of the diagnostic.</summary>
    public AnalyzerConfigSeverity Severity { get; init; }

    /// <inheritdoc cref="DiagnosticDescriptor.Title"/>
    public LocalizableString? Title { get; init; }

    /// <summary>The (optional) justification for the override.</summary>
    public string? Justification { get; init; }

    /// <summary>Indicates if this the override of a default.</summary>
    public bool IsOverride { get; init; }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(AnalyzerConfigSeverityEntry? other)
    {
        if (other is null)
        {
            return +1;
        }
        else if (IsOverride != other.IsOverride)
        {
            return other.IsOverride.CompareTo(IsOverride);
        }
        else
        {
            var compare = Id.CompareTo(other.Id);

            return compare is 0
                ? Severity.CompareTo(other.Severity)
                : compare;
        }
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        var sb = new StringBuilder();

        var param = $"dotnet_diagnostic.{Id}.severity";

        sb.Append($"{param,-35} = {Severity.ToString().ToLowerInvariant(),-10}");

        if (Title is { })
        {
            sb.Append($" # {Title}");
        }
        if (Justification is { Length: > 0 })
        {
            if (Title is null)
            {
                sb.Append(" #");
            }
            sb.Append($" [Justification: {Justification}]");
        }
        return sb.ToString();
    }

    /// <summary>Creates a new entry based on the <see cref="AnalyzerInfo"/>.</summary>
    [Pure]
    public static AnalyzerConfigSeverityEntry From(AnalyzerInfo info) => new()
    {
        Id = info.Id,
        Severity = info.AnalyzerConfigSeverity,
        Title = info.Title,
    };
}
