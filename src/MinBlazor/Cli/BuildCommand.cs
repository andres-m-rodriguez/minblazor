using MinBlazor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class BuildCommand
{
    private readonly IOutput _output;

    public BuildCommand(IOutput output) => _output = output;

    public int Execute(BuildOptions options)
    {
        _output.Info($"building {Path.GetFileName(options.RazorFile)}\n");

        var prepared = new Pipeline(_output).Prepare(options.RazorFile, options.Clean);
        if (!prepared.IsSuccess)
        {
            _output.Error(prepared.Error!);
            return 1;
        }

        var builder = new Builder();
        builder.Output += _output.Info;

        var built = builder.Build(prepared.Value!);
        if (!built.IsSuccess)
        {
            _output.Error(built.Error!);
            return 1;
        }

        _output.Info($"\nBuild succeeded.\nProject: {prepared.Value!}");
        return 0;
    }
}
