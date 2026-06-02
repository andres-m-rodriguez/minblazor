using MinBlazor.Compiler;
using MinBlazor.Core;
using MinBlazor.Parser;
using RazorCompiler = MinBlazor.Compiler.RazorCompiler;

namespace MinBlazor.Cli.Steps;

public sealed class CompileStep : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Compile;

    public Result Execute(PipelineContext context)
    {
        var virtuals = context.Script?.Outputs.Components
            .ToDictionary(c => c.Name, c => c.Source, StringComparer.Ordinal)
            ?? [];

        var resolver = new IndexedComponentResolver(
            context.ComponentTable!,
            context.SourcePaths,
            virtuals);

        var entryName = ComponentNameParser.From(Path.GetFileNameWithoutExtension(context.RazorPath));
        var entrySource = File.ReadAllText(context.RazorPath);

        var compilation = new RazorCompiler(resolver, context.Diagnostics).Compile(entryName, entrySource);

        context.Compilation = compilation;
        return Result.Ok();
    }
}
