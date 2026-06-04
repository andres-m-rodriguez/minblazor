using MinBlazor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class PublishCommand(IOutput output)
{
    public int Execute(PublishOptions options)
    {
        output.Info($"publishing {Path.GetFileName(options.RazorFile)}\n");

        var context = new PipelineContext(
            options.RazorFile,
            clean: options.Clean,
            noShadow: options.NoShadow
        );
        var pipeline = new PipelineRunner(output).RunUpTo(PipelineStep.Restore, context);
        if (!pipeline.IsSuccess)
        {
            output.Error(pipeline.Error!);
            return 1;
        }

        var outputDir = options.OutputDir is not null
            ? Path.GetFullPath(options.OutputDir)
            : Path.Combine(Path.GetDirectoryName(options.RazorFile)!, "publish");

        output.Info("Publishing (Release)...");
        var result = new InProcessBuilder().Publish(
            context.ScaffoldDir!,
            outputDir,
            context.Diagnostics
        );
        if (!result.IsSuccess)
        {
            output.Error(result.Error!);
            return 1;
        }

        output.Info($"\nPublish succeeded.\nOutput: {result.Value}");
        return 0;
    }
}
