using MinBlazor.Index;
using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class Build2Command(IOutput output)
{
    public int Execute(Build2Options options)
    {
        var table = new Pipeline(output).Prebuild(options.RazorFile);
        if (!table.IsSuccess)
        {
            output.Error(table.Error!);
            return 1;
        }

        var t = table.Value!;
        output.Info($"source:  {t.OfKind(ComponentKind.Source).Count}");
        output.Info($"virtual: {t.OfKind(ComponentKind.Virtual).Count}");
        output.Info($"package: {t.OfKind(ComponentKind.Package).Count}");
        output.Info($"total:   {t.Count}");
        return 0;
    }
}
