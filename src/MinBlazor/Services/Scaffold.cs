using MinBlazor.Razor;
using MinBlazor.Razor.Models;

namespace MinBlazor.Services;

public sealed class Scaffold
{
    private const string DependenciesFile = "Dependencies.cs";

    public void Write(string targetDir, string sourceDir, Compilation compilation, int port)
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
            Produce(produced, Path.Combine(targetDir, Path.GetFileName(cs)), File.ReadAllText(cs));

        var hasDependencies = File.Exists(Path.Combine(sourceDir, DependenciesFile));

        Produce(produced, Path.Combine(targetDir, ".gitignore"), "*\n");
        Produce(produced, Path.Combine(targetDir, "App.csproj"), ScaffoldTemplates.Csproj(AppInfo.BlazorPackageVersion, compilation.Packages));
        Produce(produced, Path.Combine(targetDir, "Program.cs"), ScaffoldTemplates.Program(compilation.Entry.Name, hasDependencies));
        Produce(produced, Path.Combine(targetDir, "_Imports.razor"), ScaffoldTemplates.Imports([]));
        Produce(produced, Path.Combine(targetDir, "Properties", "launchSettings.json"), ScaffoldTemplates.LaunchSettings(port));
        var head = string.Join('\n', hostTags.OrderBy(tag => tag, StringComparer.Ordinal));
        Produce(produced, Path.Combine(targetDir, "wwwroot", "index.html"), ScaffoldTemplates.IndexHtml.Replace(ScaffoldTemplates.HeadPlaceholder, head));

        DeleteOrphans(targetDir, produced);
    }

    private static void WriteComponent(string targetDir, Emitter emitter, CompiledComponent component, List<string> hostTags, HashSet<string> produced)
    {
        Produce(produced, Path.Combine(targetDir, $"{component.Name}.razor"), emitter.Emit(component.Document));
        hostTags.AddRange(emitter.EmitHostTags(component.Document, component.HostTags));
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
