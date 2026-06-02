using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class RestoreCommand(IOutput output)
{
    public int Execute(RestoreOptions options)
    {
        output.Info($"restoring {Path.GetFileName(options.RazorFile)}");
        var context = new PipelineContext(options.RazorFile, noShadow: options.NoShadow);
        var result = new PipelineRunner(output).RunUpTo(PipelineStep.Restore, context);
        if (!result.IsSuccess) { output.Error(result.Error!); return 1; }
        output.Info("Restore complete.");
        return 0;
    }
}
