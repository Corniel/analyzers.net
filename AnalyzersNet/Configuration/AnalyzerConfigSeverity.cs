namespace AnalyzersNet.Configuration;

/// <summary>Severities used in the analyzer configuration file.</summary>
public enum AnalyzerConfigSeverity
{
    /// <summary>None/not applicable.</summary>
    None = 0,

    /// <summary>Silent.</summary>
    Silent,

    /// <summary>Info.</summary>
    Info,

    /// <summary>Suggestion.</summary>
    Suggestion,

    /// <summary>Warning.</summary>
    Warning,

    /// <summary>Error (results in a build failure).</summary>
    Error,
}
