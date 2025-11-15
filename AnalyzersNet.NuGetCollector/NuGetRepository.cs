using AnalyzersNet.NuGetCollector.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace AnalyzersNet.NuGetCollector;

/// <summary>NuGet Repository.</summary>
/// <remarks>
/// Subset of NuGet functionality to download analyzer files.
/// </remarks>
internal static class NuGetRepository
{
    private static readonly Uri NuGetV3 = new("https://api.nuget.org/v3/index.json");

    /// <summary>Fetches the diagnostics of the analyzers.</summary>
    [Pure]
    public static async Task<IReadOnlySet<AnalyzerInfo>> FetchDiagnosticsAsync(string packageId, bool includePrerelease = false)
    {
        var folders = await ResolveFoldersAsync(packageId, includePrerelease);
        var infos = new HashSet<AnalyzerInfo>();
        var assemblies = new List<Assembly>();

        foreach (var dll in folders
            .SelectMany(f => f.Files)
            .Where(f => f.Extension == ".dll"))
        {
            using var stream = new MemoryStream();
            using var reader = dll.OpenRead();

            await reader.CopyToAsync(stream);

            try
            {
                assemblies.Add(Assembly.Load(stream.ToArray()));
            }
            catch (BadImageFormatException)
            {
                // Not a .NET dll.
            }
        }

     
        using (new AssemblyResolver(assemblies))
        {
            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                var analyzers = types
                    .Where(IsDiagnosticAnalyzer)
                    .Select(Activator.CreateInstance)
                .OfType<DiagnosticAnalyzer>();

                foreach (var analyzer in analyzers)
                {
                    var languages = analyzer.GetType().GetCustomAttribute<DiagnosticAnalyzerAttribute>()!.Languages;

                    var obsolete = analyzer
                        .GetType()
                        .GetCustomAttribute<ObsoleteAttribute>()?.Message;

                    foreach (var desc in analyzer.SupportedDiagnostics)
                    {
                        infos.AddRange(languages.Select(lang => new AnalyzerInfo()
                        {
                            Id = DiagnosticId.Parse(desc.Id),
                            Language = lang,
                            Title = desc.Title.ToString(),
                            Description = desc.Description.ToString(),
                            CustomTags = [.. desc.CustomTags],
                            DefaultSeverity = desc.DefaultSeverity,
                            IsEnabledByDefault = desc.IsEnabledByDefault,
                            HelpLinkUri = desc.HelpLinkUri,
                            Obsolete = obsolete,
                        }));
                    }
                }
            }
        }

        return infos;

        static bool IsDiagnosticAnalyzer(Type type)
            => !type.IsAbstract
            && type.IsAssignableTo(typeof(DiagnosticAnalyzer))
            && type.GetCustomAttribute<DiagnosticAnalyzerAttribute>() is { };
    }

    /// <summary>Resolves the folders of a package.</summary>
    [Pure]
    public static async Task<ImmutableArray<NuGetPackageFolder>> ResolveFoldersAsync(string packageId, bool includePrerelease = false)
    {
        var version = await GetLatestVersionAsync(packageId, includePrerelease);

        var location = await DownloadAsync(packageId, version);

        return
        [.. location
            .GetFiles("*.*", SearchOption.AllDirectories)
            .GroupBy(file => file.Directory?.Name.Split('+')[0])
            .Select(group => new NuGetPackageFolder(Path.GetFileName(group.Key)!, [.. group]))
        ];
    }

    /// <summary>Gets the latest version of a NuGet package.</summary>
    [Pure]
    public static async Task<NuGetVersion> GetLatestVersionAsync(string packageId, bool includePrerelease = false)
    {
        var cache = await GetLatestVersionsCacheAsync();
        if (cache.TryGetValue(packageId, out var cached)
            && cached.Version is { Length: > 0 }
            && cached.Checked.AddDays(2) >= Clock.UtcNow()
            && new NuGetVersion(cached.Version) is { } version
            && (!version.IsPrerelease || includePrerelease))
        {
            return version;
        }
        else
        {
            var repo = await Repo();
            using var context = new SourceCacheContext();
            var all = await repo.GetAllVersionsAsync(packageId, context, NullLogger.Instance, default);
            var latest = all.OrderByDescending(v => v.Version).FirstOrDefault(version => includePrerelease || !version.IsPrerelease);
            if (latest is { })
            {
                cache[packageId] = new NuGetLatestVersionCheck(latest.OriginalVersion, Clock.UtcNow());

                if (LatestVersionsFile.Directory is { Exists: false } directory)
                {
                    directory.Create();
                }
                await cache.SaveAsync(LatestVersionsFile);
                return latest;
            }
            else
            {
                throw new InvalidOperationException($"Version could not be resolved for '{packageId}'.");
            }
        }
    }

    /// <summary>The local (storage) directory.</summary>
    public static DirectoryInfo LocalDirectory
        => new(Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? "../../../../packages");

    /// <summary>The latest version latest versions cache file.</summary>
    private static FileInfo LatestVersionsFile => new(Path.Combine(LocalDirectory.FullName, "latest-versions.json"));

    /// <summary>Gets the location of the cached assemblies.</summary>
    [Pure]
    private static DirectoryInfo CacheDirectory(string id, NuGetVersion version) => new(Path.Combine(LocalDirectory.FullName, id, $"cached.{version}"));

    [Impure]
    private static async Task<DirectoryInfo> DownloadAsync(string packageId, NuGetVersion version)
    {
        var resource = await Repo();
        using var stream = new MemoryStream();
        using var context = new SourceCacheContext();

        await resource.CopyNupkgToStreamAsync(packageId, version, stream, context, NullLogger.Instance, default);
        using var packageReader = new PackageArchiveReader(stream);

        var cacheDir = CacheDirectory(packageId, version);
        if (!cacheDir.Exists)
        {
            cacheDir.Create();
        }

        foreach (var file in packageReader.GetFiles())
        {
            packageReader.ExtractFile(file, Path.Combine(cacheDir.FullName, file), NullLogger.Instance);
        }
        return cacheDir;
    }

    [Pure]
    private static Task<FindPackageByIdResource> Repo()
        => Repository.Factory.GetCoreV3(NuGetV3.AbsoluteUri).GetResourceAsync<FindPackageByIdResource>();

    [Pure]
    private static async Task<NuGetLatestVersions> GetLatestVersionsCacheAsync()
    {
        latestVersions ??= await NuGetLatestVersions.LoadAsync(LatestVersionsFile);
        return latestVersions;
    }

    private static NuGetLatestVersions? latestVersions;
}
