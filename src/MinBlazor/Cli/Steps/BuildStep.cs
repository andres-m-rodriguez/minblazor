using MinBlazor.Core;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class BuildStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Build;
    public string? StartMessage => "Building...";

    public Result Execute(PipelineContext context)
    {
        var buildDiagnostics = new Diagnostics();
        var built = new Builder().Build(context.ScaffoldDir!, buildDiagnostics);

        if (!built.IsSuccess)
        {
            foreach (var d in buildDiagnostics.Items)
                context.Diagnostics.Warning(d.Message);

            return Result.Fail(built.Error!);
        }

        context.ManifestPath = built.Value;
        return Result.Ok();
    }
}

