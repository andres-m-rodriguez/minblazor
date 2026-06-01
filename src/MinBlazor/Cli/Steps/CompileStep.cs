using MinBlazor.Compiler;
using MinBlazor.Parser;
using MinBlazor.Services;
using RazorCompiler = MinBlazor.Compiler.RazorCompiler;

namespace MinBlazor.Cli.Steps;

public sealed class CompileStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Compile;

    public Result Execute(PipelineContext context)
    {
        var virtuals =
            context.Script?.Outputs.Components.ToDictionary(
                c => c.Name,
                c => c.Source,
                StringComparer.Ordinal
            ) ?? [];

        var resolver = new IndexedComponentResolver(
            context.ComponentTable!,
            context.SourcePaths,
            virtuals
        );

        var entryName = ComponentNameParser.From(
            Path.GetFileNameWithoutExtension(context.RazorPath)
        );
        var entrySource = File.ReadAllText(context.RazorPath);

        var diagnostics = new Diagnostics();
        var compilation = new RazorCompiler(resolver, diagnostics).Compile(entryName, entrySource);

        foreach (var diagnostic in diagnostics.Items)
            context.Diagnostics.Add(diagnostic);

        context.Compilation = compilation;
        return Result.Ok();
    }
}
