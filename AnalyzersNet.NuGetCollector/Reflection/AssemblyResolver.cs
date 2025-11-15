using System.Collections.Immutable;
using System.Reflection;

namespace AnalyzersNet.NuGetCollector.Reflection;

internal sealed class AssemblyResolver : IDisposable
{
    private readonly ImmutableArray<Assembly> Assemblies;

    public AssemblyResolver(IReadOnlyCollection<Assembly> assemblies)
    {
        Assemblies = [..assemblies];
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
    }

    [Pure]
    private Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
        => Assemblies.FirstOrDefault(x => x.FullName == args.Name);

    public void Dispose()
        => AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
}
