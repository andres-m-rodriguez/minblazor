using MinBlazor.Core;
using MinBlazor.Cli.Steps;

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
            if (step.StartMessage is not null)
                output.Info(step.StartMessage);

            var countBefore = context.Diagnostics.Items.Count;
            var result = step.Execute(context);

            // Print any diagnostics this step added, immediately after it runs
            foreach (var d in context.Diagnostics.Items.Skip(countBefore)
                .Where(d => d.Severity != DiagnosticSeverity.Info))
                output.Info(d.Message);

            if (!result.IsSuccess)
                return result;
        }

        return Result.Ok();
    }
}
