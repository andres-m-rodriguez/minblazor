using MinBlazor.Razor.Models;

namespace MinBlazor.Cli;

public interface IPipelineStep
{
    PipelineStep Step { get; }
    Result Execute(PipelineContext context);
}
