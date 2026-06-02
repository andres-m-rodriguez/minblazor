using MinBlazor.Core;
using MinBlazor.Index;
using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class Build2Command(IOutput output)
{
    public int Execute(Build2Options options)
    {
        var context = new PipelineContext(options.RazorFile);
        var result = new PipelineRunner(output).RunUpTo(PipelineStep.Compile, context);
        if (!result.IsSuccess)
        {
            output.Error(result.Error!);
            return 1;
        }

        var t = context.ComponentTable!;
        output.Info($"--- index ---");
        output.Info($"source:  {t.OfKind(ComponentKind.Source).Count}");
        output.Info($"virtual: {t.OfKind(ComponentKind.Virtual).Count}");
        output.Info($"package: {t.OfKind(ComponentKind.Package).Count}");
        output.Info($"total:   {t.Count}");

        var c = context.Compilation!;
        output.Info($"\n--- compilation ---");
        output.Info($"entry:      {c.Entry.Name}");
        output.Info($"components: {string.Join(", ", c.Components.Select(x => x.Name))}");
        output.Info($"packages:   {string.Join(", ", c.Packages.Select(x => x.Name))}");

        foreach (var d in context.Diagnostics.Items)
            output.Info($"{d.Severity}: {d.Message}");

        return 0;
    }
}


