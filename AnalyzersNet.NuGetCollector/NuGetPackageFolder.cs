using System.Collections.Immutable;
using System.IO;

namespace AnalyzersNet.NuGetCollector;

/// <summary>Represents a folder in a NuGet Package.</summary>
[DebuggerDisplay("Name = {Name}, Count = {Files.Count}")]
[DebuggerTypeProxy(typeof(Diagnostics.CollectionDebugView))]
internal sealed record NuGetPackageFolder : IReadOnlyCollection<FileInfo>
{
    /// <summary>Initializes a new instance of the <see cref="NuGetPackageFolder"/> class.</summary>
    public NuGetPackageFolder(string name, ImmutableArray<FileInfo> files)
    {
        Name = name;
        Files = files;
    }

    /// <summary>The name of the package.</summary>
    public string Name { get; }

    /// <inheritdoc />
    public int Count => Files.Length;

    /// <summary>The files in the folder.</summary>
    public ImmutableArray<FileInfo> Files { get; }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<FileInfo> GetEnumerator() => Files.AsEnumerable().GetEnumerator();

    /// <inheritdoc />
    [Pure]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
