using AnalyzersNet;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LookupType = System.Collections.Generic.SortedDictionary<AnalyzersNet.DiagnosticId, System.Collections.Generic.SortedDictionary<string, AnalyzersNet.DiagnosticAnalyzerInfo>>;

namespace AnalyzersNet;

/// <summary>A read-only collection of <see cref="DiagnosticAnalyzerInfo" />.</summary>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Diagnostics.CollectionDebugView))]
[JsonConverter(typeof(Json.DiagnosticAnalyzerInfosConverter))]
public sealed class DiagnosticAnalyzerInfos(LookupType analyzers) : IReadOnlyCollection<DiagnosticAnalyzerInfo>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly LookupType Lookup = analyzers;

    /// <inheritdoc />
    public int Count => Lookup.Count;

    /// <summary>Find the info bsaed on the ID and language.</summary>
    [Pure]
    public DiagnosticAnalyzerInfo? Find(DiagnosticId id, string language)
        => Lookup.TryGetValue(id, out var infos)
        && infos.TryGetValue(language, out var info)
            ? info
            : null;

    /// <inheritdoc />
    [Pure]
    public IEnumerator<DiagnosticAnalyzerInfo> GetEnumerator() => Lookup.Values
        .SelectMany(x => x.Values)
        .OrderBy(x => x.Id)
        .GetEnumerator();

    /// <inheritdoc />
    [Pure]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Loads all <see cref="DiagnosticInfo"/>s from a path.</summary>
    [Pure]
    public static DiagnosticAnalyzerInfos Load(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Load(stream);
    }

    /// <summary>Loads all <see cref="DiagnosticInfo"/>s from a stream.</summary>
    [Pure]
    public static DiagnosticAnalyzerInfos Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return JsonSerializer.Deserialize<DiagnosticAnalyzerInfos>(stream)
            ?? new([]);
    }

    [Pure]
    internal LookupType ToJson() => Lookup;
}
