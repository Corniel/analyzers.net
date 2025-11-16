using Specs.Json;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AnalyzersNet;

/// <summary><see cref="Package"/> helper.</summary>
public static class Packages
{
    /// <summary>Saves the <see cref="Package" />s to the specified path.</summary>
    public static void Save(this IEnumerable<Package> packages, string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        packages.Save(stream);
    }

    /// <summary>Saves the <see cref="Package" />s to the specified stream.</summary>
    public static void Save(this IEnumerable<Package> packages, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        JsonSerializer.Serialize(stream, packages.ToArray(), Options);
    }

    /// <summary>Find the info bsaed on the ID and language.</summary>
    [Pure]
    public static AnalyzerInfo? Find(this IEnumerable<Package> packages, DiagnosticId id, string language)
        => packages.SelectMany(p => p.Analyzers)
        .FirstOrDefault(a => a.Id == id && a.Language == language);

    /// <summary>Loads all <see cref="Package"/>s from a path.</summary>
    [Pure]
    public static ImmutableArray<Package> Load(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Load(stream);
    }

    /// <summary>Loads all <see cref="Package"/>s from a stream.</summary>
    [Pure]
    public static ImmutableArray<Package> Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return [.. JsonSerializer.Deserialize<Package[]>(stream) ?? []];
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver!
           .WithAddedModifier(CollectionMembersResolver.IgnoreEmpty),
    };
}
