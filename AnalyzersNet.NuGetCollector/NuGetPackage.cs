namespace AnalyzersNet.NuGetCollector;

/// <summary>Represents NuGet package info.</summary>
public sealed record NuGetPackage
{
    /// <summary>Initializes a new instance of the <see cref="NuGetPackage"/> class.</summary>
    public NuGetPackage(string id, bool includePreRelease = false)
    {
        Id = id;
        IncludePreRelease = includePreRelease;
    }

    /// <summary>The Id of the package.</summary>
    public string Id { get; }

    /// <summary>Indicates if pre-releases should be included.</summary>
    public bool IncludePreRelease { get; }
}
