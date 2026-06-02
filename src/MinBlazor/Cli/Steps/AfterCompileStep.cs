using MinBlazor.Build.Models;
using MinBlazor.Core;

namespace MinBlazor.Cli.Steps;

public sealed class AfterCompileStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.AfterCompile;
    public string? StartMessage => null;

    public Result Execute(PipelineContext context)
    {
        if (context.Script is null)
            return Result.Ok();

        var compilation = context.Compilation!;
        var components = new List<string> { compilation.Entry.Name };
        components.AddRange(compilation.Components.Select(c => c.Name));

        var info = new CompilationInfo(compilation.Entry.Name, components, compilation.Packages);

        var result = context.Script.RunAfterCompile(info);
        if (!result.IsSuccess)
            return Result.Fail(result.Error!);

        var duplicate = context.Script.Outputs.Sources
            .GroupBy(s => s.FileName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
            return Result.Fail($"Two build source files are named '{duplicate.Key}'. Source file names must be unique.");

        return Result.Ok();
    }
}
