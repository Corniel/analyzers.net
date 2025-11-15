using System.Collections.Immutable;

namespace AnalyzersNet.NuGetCollector;

/// <summary>Collects <see cref="AnalyzerInfo"/>.</summary>
public static class Collector
{
    /// <summary>Collects <see cref="AnalyzerInfo"/>.</summary>
    /// <param name="packages">
    /// The packages to collect for.
    /// </param>
    [Pure]
    public static async Task<Package[]> Collect(IReadOnlyCollection<NuGetPackage> packages)
    {
        ArgumentNullException.ThrowIfNull(packages);

        var collection = new List<Package>();

        foreach (var package in packages)
        {
            var analyzers = await NuGetRepository.FetchDiagnosticsAsync(package.Id, package.IncludePreRelease);

            if (!analyzers.Any())
            {
                continue;
            }

            collection.Add(new()
            {
                PackageId = package.Id,
                Version = await NuGetRepository.GetLatestVersionAsync(package.Id, package.IncludePreRelease),
                Analyzers = [.. analyzers],
            });
        }

        return [.. collection];
    }
}
