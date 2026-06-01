using MinBlazor.Core;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class BuildStep(IOutput output) : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Build;

    public Result Execute(PipelineContext context)
    {
        var builder = new Builder();
        builder.Output += output.Info;

        var built = builder.Build(context.ScaffoldDir!);
        if (!built.IsSuccess)
            return Result.Fail(built.Error!);

        context.ManifestPath = built.Value;
        return Result.Ok();
    }
}
