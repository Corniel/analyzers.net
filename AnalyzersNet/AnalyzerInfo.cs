using AnalyzersNet.Configuration;
using Qowaiv.Hashing;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace AnalyzersNet;

/// <summary>
/// Represents <see cref="DiagnosticDescriptor"/> data combined with package,
/// version and language data.
/// </summary>
[DebuggerDisplay("{Id}: {Title} ({Language})")]
public sealed record AnalyzerInfo :
    IEquatable<AnalyzerInfo>,
    IComparable<AnalyzerInfo>
{
    /// <inheritdoc cref="DiagnosticDescriptor.Id" />
    public required DiagnosticId Id { get; init; }

    /// <summary>The language of the diagnostic.</summary>
    public required string Language { get; init; }

    /// <inheritdoc cref="DiagnosticDescriptor.Title" />
    public string Title { get; init; } = string.Empty;

    /// <inheritdoc cref="DiagnosticDescriptor.Description" />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc cref="DiagnosticDescriptor.HelpLinkUri" />
    public string HelpLinkUri { get; init; } = string.Empty;

    /// <inheritdoc cref="DiagnosticDescriptor.CustomTags" />
    public ImmutableArray<string> CustomTags { get; init; } = [];

    /// <summary>Collection of similar rules.</summary>
    public ImmutableHashSet<DiagnosticId> Similar { get; init; } = [];

    /// <inheritdoc cref="DiagnosticDescriptor.DefaultSeverity" />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DiagnosticSeverity DefaultSeverity { get; init; } = DiagnosticSeverity.Warning;

    /// <inheritdoc cref="DiagnosticDescriptor.IsEnabledByDefault" />
    public bool IsEnabledByDefault { get; init; } = true;

    /// <summary>The .globalconfig severity.</summary>
    [JsonIgnore]
    public AnalyzerConfigSeverity AnalyzerConfigSeverity => DefaultSeverity switch
    {
        _ when !IsEnabledByDefault /*.*/ => AnalyzerConfigSeverity.None,
        DiagnosticSeverity.Hidden /*..*/ => AnalyzerConfigSeverity.Silent,
        DiagnosticSeverity.Info /*....*/ => AnalyzerConfigSeverity.Info,
        DiagnosticSeverity.Warning /*.*/ => AnalyzerConfigSeverity.Warning,
        DiagnosticSeverity.Error /*...*/ => AnalyzerConfigSeverity.Error,
        _ => throw new InvalidCastException($"{DefaultSeverity} can not be mapped to {typeof(AnalyzerConfigSeverity)}."),
    };

    /// <summary>Obsolete indication.</summary>
    public string? Obsolete { get; init; }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(AnalyzerInfo? other)
    {
        if (other is null) return +1;

        var self = Split(Id);
        var othr = Split(other.Id);

        if (self.Prefix == othr.Prefix)
        {
            if (IsEnabledByDefault != other.IsEnabledByDefault)
            {
                // None first.
                return IsEnabledByDefault.CompareTo(other.IsEnabledByDefault);
            }
            else if (DefaultSeverity != other.DefaultSeverity)
            {
                return DefaultSeverity.CompareTo(other.DefaultSeverity);
            }
            else
            {
                return (self.Numeric ?? 0).CompareTo(othr.Numeric ?? 0);
            }
        }
        else
        {
            return Id.CompareTo(other.Id);
        }

        static (string? Prefix, int? Numeric) Split(DiagnosticId id) => (id.Prefix, id.Numeric);
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(AnalyzerInfo? other)
        => other is { }
        && Id == other.Id
        && Language == other.Language;

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode() => Hash.Code(Id).And(Language);
}
