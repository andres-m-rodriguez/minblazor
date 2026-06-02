using MinBlazor.Core;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class BuildStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Build;
    public string? StartMessage => "Building...";

    public Result Execute(PipelineContext context)
    {
        var buildProperties = context.Script?.Outputs.Properties
            ?? new Dictionary<string, string>();

        var built = new InProcessBuilder().Build(
            context.ScaffoldDir!,
            context.Compilation!,
            buildProperties,
            AppInfo.BlazorPackageVersion,
            context.Diagnostics);

        if (!built.IsSuccess)
            return Result.Fail(built.Error!);

        context.ManifestPath = built.Value;
        return Result.Ok();
    }
}
