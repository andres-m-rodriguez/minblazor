using MinBlazor.Index;
using MinBlazor.Razor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class PipelineContext(string razorPath)
{
    public string RazorPath { get; } = razorPath;

    // Set by PrebuildStep
    public ComponentTable? ComponentTable { get; set; }
    public BuildScript? Script { get; set; }

    // Set by CompileStep
    public Compilation? Compilation { get; set; }
    public List<Diagnostic> Diagnostics { get; } = [];
}
