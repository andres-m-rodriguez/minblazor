using MinBlazor.Compiler;
using MinBlazor.Core;
using MinBlazor.Index;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class PipelineContext(string razorPath, bool clean = false)
{
    public string RazorPath { get; } = razorPath;
    public bool Clean { get; } = clean;

    // Set by PrebuildStep
    public ComponentTable? ComponentTable { get; set; }
    public Dictionary<string, string> SourcePaths { get; } = new(StringComparer.Ordinal);
    public BuildScript? Script { get; set; }

    // Set by CompileStep
    public Compilation? Compilation { get; set; }
    public List<Diagnostic> Diagnostics { get; } = [];

    // Set by ScaffoldStep
    public string? ScaffoldDir { get; set; }
}
