using MinBlazor.Core;
using MinBlazor.Parser;
using MinBlazor.Scaffold;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class ScaffoldStep : IPipelineStep
{
    private const string DependenciesFile = "Dependencies.cs";

    public PipelineStep Step => PipelineStep.Scaffold;

    public Result Execute(PipelineContext context)
    {
        var razorPath = context.RazorPath;
        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var scaffoldDir = Pipeline.CacheDirectory(razorPath);

        if (context.Clean && Directory.Exists(scaffoldDir))
        {
            Directory.Delete(scaffoldDir, recursive: true);
        }

        Directory.CreateDirectory(scaffoldDir);
        Directory.CreateDirectory(Path.Combine(scaffoldDir, "Properties"));
        Directory.CreateDirectory(Path.Combine(scaffoldDir, "wwwroot"));

        var packageUsings = GetPackageUsings(sourceDir, context.Compilation!);

        var options = new ScaffoldOptions(
            AppInfo.BlazorPackageVersion,
            AppInfo.DefaultPort,
            HasDependencies: File.Exists(Path.Combine(sourceDir, DependenciesFile)),
            PackageUsings: packageUsings);

        var content = new ScaffoldGenerator().Generate(
            context.Compilation!,
            context.Script?.Outputs,
            options);

        var produced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in content.Entries)
            WriteEntry(scaffoldDir, entry, produced);

        // copy user .cs files from source (exclude Build.cs)
        foreach (var cs in Directory.EnumerateFiles(sourceDir, "*.cs"))
        {
            var name = Path.GetFileName(cs);
            if (string.Equals(name, BuildScript.FileName, StringComparison.OrdinalIgnoreCase))
                continue;
            Produce(scaffoldDir, name, File.ReadAllText(cs), produced);
        }

        Produce(scaffoldDir, ".gitignore", "*\n", produced);

        DeleteOrphans(scaffoldDir, produced);

        context.ScaffoldDir = scaffoldDir;
        return Result.Ok();
    }

    private static void WriteEntry(string scaffoldDir, ScaffoldEntry entry, HashSet<string> produced)
    {
        switch (entry)
        {
            case ProjectFile f:
                Produce(scaffoldDir, "App.csproj", f.Content.ToString(), produced);
                break;

            case SourceFile f:
                Produce(scaffoldDir, f.Name, f.Content.ToString(), produced);
                break;

            case ComponentFile f:
                Produce(scaffoldDir, $"{f.Name}.razor", f.Source.ToString(), produced);
                break;

            case HostPage f:
                ProduceBinary(Path.Combine(scaffoldDir, "wwwroot", "index.html"),
                    System.Text.Encoding.UTF8.GetBytes(f.Content.ToString()), produced, "index.html");
                break;

            case StaticAsset f:
                var assetPath = Path.Combine(scaffoldDir, "wwwroot",
                    f.Name.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(assetPath)!);
                var assetBytes = f.Content.ToArray();
                if (!File.Exists(assetPath) || !File.ReadAllBytes(assetPath).AsSpan().SequenceEqual(assetBytes))
                    File.WriteAllBytes(assetPath, assetBytes);
                produced.Add(Path.GetFileName(assetPath));
                break;
        }
    }

    private static void Produce(string scaffoldDir, string name, string content, HashSet<string> produced)
    {
        produced.Add(name);
        var path = Path.Combine(scaffoldDir, name);
        if (!File.Exists(path) || File.ReadAllText(path) != content)
            File.WriteAllText(path, content);
    }

    private static void ProduceBinary(string path, byte[] content, HashSet<string> produced, string name)
    {
        produced.Add(name);
        if (!File.Exists(path) || !File.ReadAllBytes(path).AsSpan().SequenceEqual(content))
            File.WriteAllBytes(path, content);
    }

    private static void DeleteOrphans(string scaffoldDir, HashSet<string> produced)
    {
        var tracked = Directory.EnumerateFiles(scaffoldDir, "*.razor")
            .Concat(Directory.EnumerateFiles(scaffoldDir, "*.cs"));

        foreach (var file in tracked)
            if (!produced.Contains(Path.GetFileName(file)))
                File.Delete(file);
    }

    private static IReadOnlyList<string> GetPackageUsings(string sourceDir, MinBlazor.Compiler.Compilation compilation)
    {
        var binDir = FindBuildOutput(sourceDir);
        if (binDir is null)
            return [];

        var packageNames = compilation.Packages.Select(p => p.Name).ToList();
        return PackageComponents.Scan(binDir, packageNames).Namespaces;
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
