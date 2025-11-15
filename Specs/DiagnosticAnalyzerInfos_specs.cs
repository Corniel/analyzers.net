using AnalyzersNet;
using System.IO;
using System.Text.Json;

namespace Specs.DiagnosticAnalyzerInfos_specs;

public class JSON
{
    [Test]
    public void Deserializable()
    {
        using var stream = new FileStream("../../../../data/info.json", FileMode.Open, FileAccess.Read);
        var infos = JsonSerializer.Deserialize<DiagnosticAnalyzerInfos>(stream);
        infos.Should().NotBeEmpty();
    }
}
