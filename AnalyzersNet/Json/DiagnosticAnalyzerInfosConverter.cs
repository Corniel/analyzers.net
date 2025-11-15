using System.Text.Json;
using System.Text.Json.Serialization;
using LookupType = System.Collections.Generic.SortedDictionary<AnalyzersNet.DiagnosticId, System.Collections.Generic.SortedDictionary<string, AnalyzersNet.DiagnosticAnalyzerInfo>>;

namespace AnalyzersNet.Json;

internal sealed class DiagnosticAnalyzerInfosConverter : JsonConverter<DiagnosticAnalyzerInfos?>
{
    public override DiagnosticAnalyzerInfos? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<LookupType>(ref reader, options) is { } lookup
            ? new(lookup)
            : null;

    public override void Write(Utf8JsonWriter writer, DiagnosticAnalyzerInfos? value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value?.ToJson(), options);
}
