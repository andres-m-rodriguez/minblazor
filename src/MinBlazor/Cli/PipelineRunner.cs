using MinBlazor.Cli.Steps;
using MinBlazor.Razor.Models;

namespace MinBlazor.Cli;

public sealed class PipelineRunner(IOutput output)
{
    private readonly IReadOnlyList<IPipelineStep> _steps =
    [
        new PrebuildStep(output),
        new CompileStep(),
    ];

    public Result RunUpTo(PipelineStep target, PipelineContext context)
    {
        foreach (var step in _steps.Where(s => s.Step <= target))
        {
            var result = step.Execute(context);
            if (!result.IsSuccess)
                return result;
        }

        return Result.Ok();
    }
}
