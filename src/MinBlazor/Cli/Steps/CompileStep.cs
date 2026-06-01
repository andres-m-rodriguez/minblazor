using MinBlazor.Razor;
using MinBlazor.Razor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class CompileStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Compile;

    public Result Execute(PipelineContext context)
    {
        var razorPath = context.RazorPath;
        var sourceDir = Path.GetDirectoryName(razorPath)!;

        var registry = new ComponentRegistry();
        var indexed = FolderIndexer.Index(sourceDir, registry);
        if (!indexed.IsSuccess)
            return Result.Fail(indexed.Error!);

        if (context.Script is not null)
            foreach (var component in context.Script.Outputs.Components)
            {
                var added = registry.Add(component.Name, component.Source);
                if (!added.IsSuccess)
                    return Result.Fail(added.Error!);
            }

        var entryName = ComponentName.From(Path.GetFileNameWithoutExtension(razorPath));
        var entrySource = File.ReadAllText(razorPath);

        var diagnostics = new Diagnostics();
        var compilation = new Compiler(registry, diagnostics).Compile(entryName, entrySource);

        foreach (var diagnostic in diagnostics.Items)
            context.Diagnostics.Add(diagnostic);

        context.Compilation = compilation;
        return Result.Ok();
    }
}
