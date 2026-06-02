using MinBlazor.Core;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class BuildStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Build;

    public Result Execute(PipelineContext context)
    {
        var built = new Builder().Build(context.ScaffoldDir!, context.Diagnostics);
        if (!built.IsSuccess)
            return Result.Fail(built.Error!);

        context.ManifestPath = built.Value;
        return Result.Ok();
    }
}
