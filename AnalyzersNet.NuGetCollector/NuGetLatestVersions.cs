using System.IO;
using System.Text.Json;

namespace AnalyzersNet.NuGetCollector;

/// <summary>Contains NuGet packages and their latest versions.</summary>
internal sealed class NuGetLatestVersions : Dictionary<string, NuGetLatestVersionCheck>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Initializes a new instance of the <see cref="NuGetLatestVersions"/> class.</summary>
    public NuGetLatestVersions() { }

    /// <summary>Saves the latests versions to a file.</summary>
    public async Task SaveAsync(FileInfo file)
    {
        using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, this, JsonOptions);
    }

    /// <summary>Saves the latests versions to a stream.</summary>
    public Task SaveAsync(Stream stream)
        => JsonSerializer.SerializeAsync(stream, this);

    /// <summary>Loads the latests versions from a file.</summary>
    [Pure]
    public static async Task<NuGetLatestVersions> LoadAsync(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Exists)
        {
            using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            return await LoadAsync(stream);
        }
        else
        {
            return [];
        }
    }

    /// <summary>Loads the latests versions from a stream.</summary>
    [Pure]
    public static Task<NuGetLatestVersions> LoadAsync(Stream stream)
        => JsonSerializer.DeserializeAsync<NuGetLatestVersions>(stream).AsTask()!;
}
