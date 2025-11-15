using LookupType = System.Collections.Generic.SortedDictionary<AnalyzersNet.DiagnosticId, System.Collections.Generic.SortedDictionary<string, AnalyzersNet.DiagnosticAnalyzerInfo>>;

namespace AnalyzersNet.NuGetCollector;

/// <summary>Collects <see cref="DiagnosticAnalyzerInfo"/>.</summary>
public static class Collector
{
    /// <summary>Collects <see cref="DiagnosticAnalyzerInfo"/>.</summary>
    /// <param name="packages">
    /// The packages to collect for.
    /// </param>
    [Pure]
    public static async Task<LookupType> Collect(IReadOnlyCollection<NuGetPackage> packages)
    {
        ArgumentNullException.ThrowIfNull(packages);

        var lookup = new LookupType();

        foreach (var package in packages)
        {
            var infos = await NuGetRepository.FetchDiagnosticsAsync(package.Id, package.IncludePreRelease);
            foreach (var info in infos)
            {
                lookup.TryAdd(info.Id, []);
                lookup[info.Id][info.Language] = info;
            }
        }

        return lookup;
    }
}
