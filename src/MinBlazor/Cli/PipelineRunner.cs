using MinBlazor.Cli.Steps;
using MinBlazor.Core;

namespace MinBlazor.Cli;

public sealed class PipelineRunner(IOutput output)
{
    private readonly IReadOnlyList<IPipelineStep> _steps =
    [
        new PrebuildStep(output),
        new ShadowStep(),
        new CompileStep(),
        new AfterCompileStep(),
        new ScaffoldStep(),
        new RestoreStep(),
        new BuildStep(),
        new ServeStep(output),
    ];

    public Result RunUpTo(
        PipelineStep target,
        PipelineContext context,
        IReadOnlySet<PipelineStep>? skip = null
    )
    {
        var steps = _steps.Where(s => s.Step <= target && (skip is null || !skip.Contains(s.Step)));
        foreach (var step in steps)
        {
            if (step.StartMessage is not null)
                output.Info(step.StartMessage);

            var countBefore = context.Diagnostics.Items.Count;
            var result = step.Execute(context);

            foreach (
                var d in context
                    .Diagnostics.Items.Skip(countBefore)
                    .Where(d => d.Severity != DiagnosticSeverity.Info)
            )
                output.Info(d.Message);

            if (!result.IsSuccess)
                return result;
        }

        return Result.Ok();
    }
}
