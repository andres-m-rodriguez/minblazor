using MinBlazor.Build;
using MinBlazor.Build.Models;
using MinBlazor.Compiler;
using MinBlazor.Parser;
using MinBlazor.Scaffold;

namespace MinBlazor.Services;

public sealed class Scaffold
{
    private const string DependenciesFile = "Dependencies.cs";

    private static readonly IReadOnlyDictionary<string, string> EmptyProperties = new Dictionary<string, string>();

    public void Write(string targetDir, string sourceDir, Compilation compilation, int port, BuildOutputs? build, IReadOnlyCollection<string> usings)
    {
        Directory.CreateDirectory(targetDir);
        Directory.CreateDirectory(Path.Combine(targetDir, "Properties"));
        Directory.CreateDirectory(Path.Combine(targetDir, "wwwroot"));

        var emitter = new Emitter();
        var hostTags = new List<string>();
        var produced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        WriteComponent(targetDir, emitter, compilation.Entry, hostTags, produced);

        foreach (var component in compilation.Components)
            WriteComponent(targetDir, emitter, component, hostTags, produced);

        foreach (var cs in Directory.EnumerateFiles(sourceDir, "*.cs"))
            if (!string.Equals(Path.GetFileName(cs), BuildScript.FileName, StringComparison.OrdinalIgnoreCase))
                Produce(produced, Path.Combine(targetDir, Path.GetFileName(cs)), File.ReadAllText(cs));

        if (build is not null)
            WriteBuildOutputs(targetDir, produced, hostTags, build);

        var hasDependencies = File.Exists(Path.Combine(sourceDir, DependenciesFile));
        var packages = compilation.Packages;
        var properties = build?.Properties ?? EmptyProperties;

        Produce(produced, Path.Combine(targetDir, ".gitignore"), "*\n");
        Produce(produced, Path.Combine(targetDir, "App.csproj"), ScaffoldTemplates.Csproj(AppInfo.BlazorPackageVersion, packages, properties));
        Produce(produced, Path.Combine(targetDir, "Program.cs"), ScaffoldTemplates.Program(compilation.Entry.Name, hasDependencies));
        Produce(produced, Path.Combine(targetDir, "_Imports.razor"), ScaffoldTemplates.Imports(usings));
        Produce(produced, Path.Combine(targetDir, "Properties", "launchSettings.json"), ScaffoldTemplates.LaunchSettings(port));

        var head = string.Join('\n', hostTags.OrderBy(tag => tag, StringComparer.Ordinal));
        Produce(produced, Path.Combine(targetDir, "wwwroot", "index.html"), ScaffoldTemplates.IndexHtml.Replace(ScaffoldTemplates.HeadPlaceholder, head));

        DeleteOrphans(targetDir, produced);
    }

    private static void WriteBuildOutputs(string targetDir, HashSet<string> produced, List<string> hostTags, BuildOutputs build)
    {
        hostTags.AddRange(build.HeadTags);

        foreach (var source in build.Sources)
            Produce(produced, Path.Combine(targetDir, source.FileName), source.Code);

        if (build.Options.Count > 0)
            Produce(produced, Path.Combine(targetDir, "BuildOptions.cs"), ScaffoldTemplates.BuildOptions(build.Options));

        foreach (var asset in build.Assets)
            WriteAsset(targetDir, asset);
    }

    private static void WriteAsset(string targetDir, BuildAsset asset)
    {
        var path = Path.Combine(targetDir, "wwwroot", asset.Path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (!File.Exists(path) || !File.ReadAllBytes(path).AsSpan().SequenceEqual(asset.Contents))
            File.WriteAllBytes(path, asset.Contents);
    }


    private static void WriteComponent(string targetDir, Emitter emitter, CompiledComponent component, List<string> hostTags, HashSet<string> produced)
    {
        Produce(produced, Path.Combine(targetDir, $"{component.Name}.razor"), emitter.Emit(component.Document));
        hostTags.AddRange(emitter.EmitHostTags(component.HostTags));
    }

    private static void Produce(HashSet<string> produced, string path, string content)
    {
        produced.Add(Path.GetFileName(path));

        if (!File.Exists(path) || File.ReadAllText(path) != content)
            File.WriteAllText(path, content);
    }

    private static void DeleteOrphans(string targetDir, HashSet<string> produced)
    {
        var written = Directory
            .EnumerateFiles(targetDir, "*.razor")
            .Concat(Directory.EnumerateFiles(targetDir, "*.cs"));

        foreach (var file in written)
            if (!produced.Contains(Path.GetFileName(file)))
                File.Delete(file);
    }
}


