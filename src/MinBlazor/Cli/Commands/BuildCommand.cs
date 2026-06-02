using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class BuildCommand(IOutput output)
{
    public int Execute(BuildOptions options)
    {
        output.Info($"building {Path.GetFileName(options.RazorFile)}\n");

        var context = new PipelineContext(options.RazorFile, clean: options.Clean);
        var result = new PipelineRunner(output).RunUpTo(PipelineStep.Build, context);

        if (!result.IsSuccess)
        {
            output.Error(result.Error!);
            return 1;
        }

        output.Info($"\nBuild succeeded.\nProject: {context.ScaffoldDir}");
        return 0;
    }
}
