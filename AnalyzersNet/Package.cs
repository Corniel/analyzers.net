using NuGet.Versioning;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace AnalyzersNet;

/// <summary>NuGet package.</summary>
public sealed record Package
{
    /// <summary>The ID of the package.</summary>
    public required string PackageId { get; init; }

    /// <summary>The (latest) version of the diagnostic.</summary>
    [JsonConverter(typeof(Json.NuGetVersionConverter))]
    public required NuGetVersion Version { get; init; }

    /// <summary>The analyzers of the package.</summary>
    public required ImmutableArray<AnalyzerInfo> Analyzers { get; init; } = [];
}
