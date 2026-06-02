using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class RunCommand(IOutput output)
{
    public int Execute(RunOptions options)
    {
        var context = new PipelineContext(
            options.RazorFile,
            clean: options.Clean,
            port: options.Port,
            openBrowser: options.OpenBrowser);

        var result = new PipelineRunner(output).RunUpTo(PipelineStep.Serve, context);

        foreach (var d in context.Diagnostics.Items.Where(d =>
            d.Severity != MinBlazor.Core.DiagnosticSeverity.Info))
            output.Info($"{d.Severity}: {d.Message}");

        if (!result.IsSuccess)
        {
            output.Error(result.Error!);
            return 1;
        }

        return 0;
    }
}
