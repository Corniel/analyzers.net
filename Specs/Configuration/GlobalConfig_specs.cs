using AnalyzersNet;
using System.Collections.Immutable;
using System.IO;

namespace Specs.Configuration.GlobalConfig_specs;

[Explicit]
public class Generates
{
    [Test]
    public void project_config()
    {
        var packages = Packages.Load("../../../../data/packages.json");

        using var stream = new FileStream("../../../../.globalconfig.generated", FileMode.Create, FileAccess.Write);

        var selection = packages.Where(p => Included.Contains(p.PackageId))
            .SelectMany(a =>a.Analyzers.Where(a => a.Language == LanguageNames.CSharp))
            .ToArray();

        selection.Should().NotBeEmpty();

        var config = AnalyzersNet.Configuration.AnalyzerConfiguration.New(true, selection);

        config.Save(stream, true);
    }

    private static readonly ImmutableArray<string> Included =
    [
        "AsyncFixer",
        "DotNetProjectFile.Analyzers",
        "IDisposableAnalyzers",
        "Marten.Analyzers",
        "Microsoft.CodeAnalysis.NetAnalyzers",
        "Moq.Analyzers",
        "NUnit.Analyzers",
        "Qowaiv.Analyzers.CSharp",
        "SonarAnalyzer.CSharp",
        "StyleCop.Analyzers.Unstable",
    ];
}
