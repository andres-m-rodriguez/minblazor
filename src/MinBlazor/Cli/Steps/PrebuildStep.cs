using MinBlazor.Index;
using MinBlazor.Razor;
using MinBlazor.Razor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class PrebuildStep(IOutput output) : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Prebuild;

    public Result Execute(PipelineContext context)
    {
        var razorPath = context.RazorPath;
        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var scaffoldDir = Pipeline.CacheDirectory(razorPath);

        var table = new ComponentTable();

        foreach (var (name, _) in new FolderSourceProvider(sourceDir).GetComponents())
            table.Add(new IndexedComponent(name, ComponentKind.Source, Namespace: null));

        var scriptResult = new BuildScriptLoader(sourceDir, scaffoldDir, output.Info).Load();
        if (!scriptResult.IsSuccess)
            return Result.Fail(scriptResult.Error!);

        var script = scriptResult.Value;

        if (script is not null)
        {
            var before = script.RunBeforeCompile();
            if (!before.IsSuccess)
                return Result.Fail(before.Error!);

            foreach (var dir in script.Outputs.SourceDirectories)
                foreach (var (name, _) in new FolderSourceProvider(dir, SearchOption.AllDirectories).GetComponents())
                    table.Add(new IndexedComponent(name, ComponentKind.Source, Namespace: null));

            foreach (var component in script.Outputs.Components)
                table.Add(new IndexedComponent(component.Name, ComponentKind.Virtual, Namespace: null));
        }

        var binDir = FindBuildOutput(sourceDir);
        if (binDir is not null)
        {
            var entryDoc = new Parser(new Lexer(File.ReadAllText(razorPath))).Parse();
            var packageNames = new Scanner().Packages(entryDoc).Select(p => p.Name).ToList();

            var scanner = new AssemblyScanner();
            scanner.Load(new BinDirectoryAssemblyProvider(binDir));
            foreach (var component in scanner.Scan(packageNames))
                table.Add(component);
        }

        context.ComponentTable = table;
        return Result.Ok();
    }

    private static string? FindBuildOutput(string sourceDir)
    {
        foreach (var file in Directory.EnumerateFiles(sourceDir, "*.razor", SearchOption.AllDirectories))
        {
            var binDir = Path.Combine(Pipeline.CacheDirectory(file), "bin", "Debug", "net10.0");
            if (Directory.Exists(binDir))
                return binDir;
        }

        return null;
    }
}
