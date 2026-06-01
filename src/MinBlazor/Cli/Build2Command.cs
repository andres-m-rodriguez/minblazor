using MinBlazor.Index;
using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class Build2Command(IOutput output)
{
    public int Execute(Build2Options options)
    {
        var context = new PipelineContext(options.RazorFile);
        var result = new PipelineRunner(output).RunUpTo(PipelineStep.Prebuild, context);
        if (!result.IsSuccess)
        {
            output.Error(result.Error!);
            return 1;
        }

        var t = context.ComponentTable!;
        output.Info($"source:  {t.OfKind(ComponentKind.Source).Count}");
        output.Info($"virtual: {t.OfKind(ComponentKind.Virtual).Count}");
        output.Info($"package: {t.OfKind(ComponentKind.Package).Count}");
        output.Info($"total:   {t.Count}");
        return 0;
    }
}
