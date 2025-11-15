using AnalyzersNet;
using AnalyzersNet.NuGetCollector;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Specs.NuGetCollector.Collect_specs;

[Explicit]
public class Collects
{
    [Test]
    public async Task NuGet_packages()
    {
        var packages = await Collector.Collect(Packages);

        var similars = Similars.Select(s => s.Select(s => DiagnosticId.Parse(s)).ToImmutableArray()).ToArray();

        for (var p = 0; p < packages.Length; p++)
        {
            var package = packages[p];
            var analyzers = package.Analyzers.ToArray();

            for (var a = 0; a < analyzers.Length; a++)
            {
                var analyzer = analyzers[a];

                foreach (var similar in similars)
                {
                    if (similar.FirstOrDefault(id => analyzer.Id == id) is { HasValue: true } id)
                    {
                        analyzer = analyzer with { Similar = [.. analyzer.Similar, .. similar.Remove(id)] };
                    }

                }
                analyzers[a] = analyzer;
            }
            packages[p] = package with { Analyzers = [.. analyzers] };
        }

        packages.Should().NotBeEmpty();

        packages.Save("../../../../data/packages.json");
    }

    private static readonly ImmutableArray<ImmutableArray<string>> Similars =
    [
        // No to-do's.
        ["S1135", "AV2318", "FFS0042", "MA0026", "PH2151", "Proj3001"],
        // Abstract types should not have public constructors.
        ["CA1012", "S3442"],
        // Exceptions should be public.
        ["CA1064", "S3871"],
        // Mark members as static.
        ["CA1822", "S2325"],
        // Dispose objects before losing scope.
        ["CA2000", "IDISP001"],
        // Do not raise reserved exception types.
        ["CA2201", "S122"],
        // Define accessors for attribute arguments.
        ["CA1019", "S3993"],
        // Use a testable Time Provider.
        ["QW0001", "S6354"],
        // Sealed classes should not have protected members.
        ["CS0628", "S2156"],
    ];

    // ALSO see: https://github.com/cybermaxs/awesome-analyzers
    private static readonly ImmutableArray<NuGetPackage> Packages =
    [
        new("Agoda.Analyzers"),
        new("Akka.Analyzers"),
        new("Apex.Analyzers.Immutable"),
        new("Ardalis.ApiEndpoints.CodeAnalyzers"),
        new("AsyncFixer"),
        new("Bit.CodeAnalyzers"),
        new("BlowinCleanCode"),
        new("CodeCracker.CSharp"),
        new("CodeCracker.CSharp"),
        new("CodeCracker.VisualBasic"),
        new("CodeCracker.VisualBasic"),
        new("CSharpGuidelinesAnalyzer"),
        new("D2L.CodeStyle.Analyzers"),
        new("DotNetProjectFile.Analyzers"),
        new("ErrorProne.NET"),
        new("Faithlife.Analyzers"),
        new("FakeItEasy.Analyzer.CSharp"),
        new("FakeItEasy.Analyzer.VisualBasic"),
        new("FluentAssertions.Analyzers"),
        new("FSharp.Analyzers.SDK"),
        new("fsharp-analyzers"),
        new("FunFair.CodeAnalysis"),
        new("Gu.Analyzers"),
        new("Gu.Analyzers"),
        new("IDisposableAnalyzers"),
        new("Libplanet.Analyzers"),
        new("Marten.Analyzers"),
        new("MassTransit.Analyzers"),
        new("Menees.Analyzers"),
        new("Menees.Analyzers"),
        new("MessagePackAnalyzer"),
        new("MessagePipe.Analyzer"),
        new("Meziantou.Analyzer"),
        new("Microsoft.AspNetCore.Components.Analyzers", true),
        new("Microsoft.Azure.Functions.Analyzers", true),
        new("Microsoft.Azure.Functions.Worker.Sdk.Analyzers", true),
        new("Microsoft.CodeAnalysis.Analyzers", true),
        new("Microsoft.CodeAnalysis.CSharp", true),
        new("Microsoft.CodeAnalysis.CSharp.CodeStyle", true),
        new("Microsoft.CodeAnalysis.NetAnalyzers", true),
        new("Microsoft.CodeAnalysis.PublicApiAnalyzers", true),
        new("Microsoft.CodeAnalysis.VisualBasic", true),
        new("Microsoft.EntityFrameworkCore.Analyzers", true),
        new("Microsoft.ServiceHub.Analyzers", true),
        new("Microsoft.VisualStudio.SDK.Analyzers", true),
        new("Microsoft.VisualStudio.Threading.Analyzers", true),
        new("MongoDB.Analyzer"),
        new("NSubstitute.Analyzers.CSharp"),
        new("NSubstitute.Analyzers.VisualBasic"),
        new("NUnit.Analyzers"),
        new("Octopus.Nevermore.Analyzers"),
        new("Philips.CodeAnalysis.DuplicateCodeAnalyzer"),
        new("Philips.CodeAnalysis.MaintainabilityAnalyzers"),
        new("Philips.CodeAnalysis.MoqAnalyzers"),
        new("Philips.CodeAnalysis.MsTestAnalyzers"),
        new("Qowaiv.Analyzers.CSharp"),
        new("ReflectionAnalyzers"),
        new("Roslynator.Analyzers"),
        new("RuntimeContracts.Analyzer"),
        new("SerilogAnalyzer"),
        new("SharpSource"),
        new("SonarAnalyzer.CSharp"),
        new("SonarAnalyzer.VisualBasic"),
        new("StructuredLogging.Analyzers"),
        new("StyleCop.Analyzers.Unstable"),
        new("Text.Analyzers", true),
        new("Uno.MonoAnalyzers"),
        new("Wintellect.Analyzers"),
        new("xunit.analyzers"),
        new("ZeroFormatter.Analyzer"),
    ];
}
