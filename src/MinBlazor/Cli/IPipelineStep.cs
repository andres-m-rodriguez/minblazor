using MinBlazor.Core;
using MinBlazor.Parser;

namespace MinBlazor.Cli;

public interface IPipelineStep
{
    PipelineStep Step { get; }
    Result Execute(PipelineContext context);
}

