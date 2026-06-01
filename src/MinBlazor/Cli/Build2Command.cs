using MinBlazor.Index;
using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class Build2Command(IOutput output)
{
    public int Execute(Build2Options options)
    {
        var razorFile = options.RazorFile;
        var sourceDir = Path.GetDirectoryName(razorFile)!;
        var scaffoldDir = Pipeline.CacheDirectory(razorFile);
        var binDir = Path.Combine(scaffoldDir, "bin", "Debug", "net10.0");

        var compiled = new Pipeline(output).Compile(razorFile, scaffoldDir);
        if (!compiled.IsSuccess)
        {
            output.Error(compiled.Error!);
            return 1;
        }

        var result = compiled.Value!;
        var table = new ComponentTable();

        output.Info("--- source components ---");
        foreach (var (name, path) in new FolderSourceProvider(sourceDir).GetComponents())
        {
            var added = table.Add(
                new IndexedComponent(name, ComponentKind.Source, Namespace: null)
            );
            output.Info($"  {(added.IsSuccess ? "+" : "!")} {name}  ({path})");
        }

        output.Info("\n--- virtual components (Build.cs) ---");
        if (result.Script is not null)
            foreach (var component in result.Script.Outputs.Components)
            {
                var added = table.Add(
                    new IndexedComponent(component.Name, ComponentKind.Virtual, Namespace: null)
                );
                output.Info($"  {(added.IsSuccess ? "+" : "!")} {component.Name}");
            }

        output.Info("\n--- package components ---");
        if (Directory.Exists(binDir))
        {
            var packageNames = result.Compilation.Packages.Select(p => p.Name).ToList();
            var scanner = new AssemblyScanner(new BinDirectoryAssemblyProvider(binDir));
            foreach (var component in scanner.Scan(packageNames))
            {
                var added = table.Add(component);
                output.Info(
                    $"  {(added.IsSuccess ? "+" : "!")} {component.Name}  (ns: {component.Namespace})"
                );
            }
        }
        else
        {
            output.Info("  (no bin — run `minblazor build` first to index package components)");
        }

        output.Info(
            $"\ntotal: {table.Count}  source={table.OfKind(ComponentKind.Source).Count}  virtual={table.OfKind(ComponentKind.Virtual).Count}  package={table.OfKind(ComponentKind.Package).Count}"
        );
        return 0;
    }
}
