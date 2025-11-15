using AnalyzersNet;

namespace Specs.DiagnosticId_specs;

public class Compares
{
    [TestCase("ABC", "BCD", -1)]
    [TestCase("ABC", "ABCD", -1)]
    [TestCase("S101", "S101", 0)]
    [TestCase("S101", "S201", -1)]
    [TestCase("S3101", "S34", +1)]
    [TestCase("S101", "S0101", 0)]
    [TestCase("S101", "S108", -1)]
    [TestCase("S101", "S1008", -1)]
    public void IDs(DiagnosticId l, DiagnosticId r, int compare)
        => l.CompareTo(r).Should().Be(compare);
}

public class Does
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("S101")]
    [TestCase("Proj0042")]
    [TestCase("ClassWithoutModifierAnalyzer")]
    public void Parse(string? str) => DiagnosticId.TryParse(str).Should().NotBeNull();
}

public class Does_not
{
    [TestCase("500")]
    [TestCase("AB500X")]
    [TestCase("500X")]
    [TestCase("$A500")]
    public void Parse(string str) => DiagnosticId.TryParse(str).Should().BeNull();
}
