using MinBlazor.Core;

namespace MinBlazor.Cli;

public interface IPipelineStep
{
    PipelineStep Step { get; }
    string? StartMessage { get; }
    Result Execute(PipelineContext context);
}
