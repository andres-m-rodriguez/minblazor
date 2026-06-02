using MinBlazor.Core;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class RestoreStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Restore;
    public string? StartMessage => "Restoring packages...";

    public Result Execute(PipelineContext context)
    {
        var buildProperties = context.Script?.Outputs.Properties
            ?? new Dictionary<string, string>();

        return new InProcessBuilder().WriteAndRestore(
            context.ScaffoldDir!,
            context.Compilation!,
            buildProperties,
            AppInfo.BlazorPackageVersion,
            context.Diagnostics);
    }
}
