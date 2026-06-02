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
            openBrowser: options.OpenBrowser,
            noShadow: options.NoShadow);

        var result = new PipelineRunner(output).RunUpTo(PipelineStep.Serve, context);
        if (!result.IsSuccess)
        {
            output.Error(result.Error!);
            return 1;
        }

        return 0;
    }
}

