using MinBlazor.Core;
using MinBlazor.Index;
using MinBlazor.Parser;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class PrebuildStep(IOutput output) : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Prebuild;
    public string? StartMessage => null;

    public Result Execute(PipelineContext context)
    {
        var razorPath = context.RazorPath;
        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var scaffoldDir = PipelineCache.DirectoryFor(razorPath);

        var table = new ComponentTable();

        foreach (var (name, path) in new FolderSourceProvider(sourceDir).GetComponents())
        {
            table.Add(new IndexedComponent(name, ComponentKind.Source, Namespace: null));
            context.SourcePaths[name] = path;
        }

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
            foreach (
                var (name, path) in new FolderSourceProvider(
                    dir,
                    SearchOption.AllDirectories
                ).GetComponents()
            )
            {
                table.Add(new IndexedComponent(name, ComponentKind.Source, Namespace: null));
                context.SourcePaths[name] = path;
            }

            foreach (var component in script.Outputs.Components)
                table.Add(
                    new IndexedComponent(component.Name, ComponentKind.Virtual, Namespace: null)
                );
        }

        var binDir = FindBuildOutput(sourceDir);
        if (binDir is not null)
        {
            var entryDoc = new RazorParser(File.ReadAllText(razorPath)).Parse();
            var packageNames = new Scanner().Packages(entryDoc).Select(p => p.Name).ToList();

            var scanner = new AssemblyScanner();
            scanner.Load(new BinDirectoryAssemblyProvider(binDir));
            foreach (var component in scanner.Scan(packageNames))
                table.Add(component);
        }

        context.ComponentTable = table;
        context.Script = script;

        WriteVirtualComponentCache(sourceDir, script);

        return Result.Ok();
    }

    private static void WriteVirtualComponentCache(string sourceDir, BuildScript? script)
    {
        var cacheDir = Path.Combine(sourceDir, "_minblazor");
        if (!Directory.Exists(cacheDir)) return;

        var names = script?.Outputs.Components.Select(c => c.Name).ToArray() ?? [];
        var json = System.Text.Json.JsonSerializer.Serialize(names);
        var path = Path.Combine(cacheDir, ".virtual-components.json");

        if (!File.Exists(path) || File.ReadAllText(path) != json)
            File.WriteAllText(path, json);
    }

    private static string? FindBuildOutput(string sourceDir)
    {
        foreach (
            var file in Directory.EnumerateFiles(sourceDir, "*.razor", SearchOption.AllDirectories)
        )
        {
            var binDir = Path.Combine(PipelineCache.DirectoryFor(file), "bin", "Debug", "net10.0");
            if (Directory.Exists(binDir))
                return binDir;
        }

        return null;
    }
}



