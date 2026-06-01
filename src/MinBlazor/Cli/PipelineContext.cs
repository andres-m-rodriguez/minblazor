using MinBlazor.Compiler;
using MinBlazor.Core;
using MinBlazor.Index;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class PipelineContext(string razorPath)
{
    public string RazorPath { get; } = razorPath;

    // Set by PrebuildStep
    public ComponentTable? ComponentTable { get; set; }
    public Dictionary<string, string> SourcePaths { get; } = new(StringComparer.Ordinal);
    public BuildScript? Script { get; set; }

    // Set by CompileStep
    public Compilation? Compilation { get; set; }
    public List<Diagnostic> Diagnostics { get; } = [];
}
