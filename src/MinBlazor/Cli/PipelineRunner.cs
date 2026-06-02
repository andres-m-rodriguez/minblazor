using MinBlazor.Core;
using MinBlazor.Cli.Steps;
using MinBlazor.Parser;

namespace MinBlazor.Cli;

public sealed class PipelineRunner(IOutput output)
{
    private readonly IReadOnlyList<IPipelineStep> _steps =
    [
        new PrebuildStep(output),
        new ShadowStep(),
        new CompileStep(),
        new ScaffoldStep(),
        new BuildStep(),
        new ServeStep(output),
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

