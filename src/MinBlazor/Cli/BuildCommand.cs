using MinBlazor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class BuildCommand(IOutput output)
{
    public int Execute(BuildOptions options)
    {
        output.Info($"building {Path.GetFileName(options.RazorFile)}\n");

        var prepared = new Pipeline(output).Prepare(options.RazorFile, options.Clean);
        if (!prepared.IsSuccess)
        {
            output.Error(prepared.Error!);
            return 1;
        }

        var builder = new Builder();
        builder.Output += output.Info;

        var built = builder.Build(prepared.Value!);
        if (!built.IsSuccess)
        {
            output.Error(built.Error!);
            return 1;
        }

        output.Info($"\nBuild succeeded.\nProject: {prepared.Value!}");
        return 0;
    }
}
