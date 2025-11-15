using AnalyzersNet.Configuration;
using NuGet.Versioning;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace AnalyzersNet;

/// <summary>
/// Represents <see cref="DiagnosticDescriptor"/> data combined with package,
/// version and language data.
/// </summary>
[DebuggerDisplay("{Id}: {Title} ({Language})")]
public sealed record DiagnosticAnalyzerInfo :
    IEquatable<DiagnosticAnalyzerInfo>,
    IComparable<DiagnosticAnalyzerInfo>
{
    /// <summary>The ID of the package.</summary>
    public required string PackageId { get; init; }

    /// <summary>The (latest) version of the diagnostic.</summary>
    [JsonConverter(typeof(Json.NuGetVersionConverter))]
    public required NuGetVersion Version { get; init; }

    /// <summary>The language of the diagnostic.</summary>
    public required string Language { get; init; }

    /// <inheritdoc cref="DiagnosticDescriptor.Id" />
    public required DiagnosticId Id { get; init; }

    /// <inheritdoc cref="DiagnosticDescriptor.Title" />
    public required string Title { get; init; }

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
    public int CompareTo(DiagnosticAnalyzerInfo? other)
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
    public bool Equals(DiagnosticAnalyzerInfo? other)
        => other is { }
        && Id == other.Id
        && PackageId == other.PackageId
        && Language == other.Language;

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode() => HashCode.Combine(Id, PackageId, Language);
}
