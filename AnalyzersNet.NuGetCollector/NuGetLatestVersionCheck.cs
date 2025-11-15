namespace AnalyzersNet.NuGetCollector;

/// <summary>The latest version of a NuGet package and the time it was checked.</summary>
internal sealed record NuGetLatestVersionCheck(string? Version, DateTime Checked);
