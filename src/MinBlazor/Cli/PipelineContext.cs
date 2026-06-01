using MinBlazor.Index;

namespace MinBlazor.Cli;

public sealed class PipelineContext
{
    public required string RazorPath { get; init; }

    // Set by PrebuildStep
    public ComponentTable? ComponentTable { get; set; }
}
