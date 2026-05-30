using MinBlazor.Models;

namespace MinBlazor.Services;

public sealed record GeneratedFile(string RelativePath, string Contents);

public sealed class Scaffolder
{
    private const string ImportsFile = "_Imports.razor";

    private readonly string _packageVersion;
    private readonly ComponentAnalyzer _analyzer = new();

    public Scaffolder(string packageVersion) => _packageVersion = packageVersion;

    public ComponentGraph Create(string targetDir, string sourceDir, string entryPath, int port)
    {
        var graph = _analyzer.Analyze(entryPath, sourceDir);
        var entryComponent = ComponentName.From(Path.GetFileNameWithoutExtension(entryPath));

        Directory.CreateDirectory(targetDir);
        RemoveStaleComponents(targetDir);
        CopyComponents(graph, targetDir);

        foreach (var file in Plan(entryComponent, port, FolderNamespaces(graph)))
        {
            var fullPath = Path.Combine(targetDir, file.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, file.Contents);
        }

        return graph;
    }

    public IReadOnlyList<GeneratedFile> Plan(string entryComponent, int port, IReadOnlyList<string> folderNamespaces) =>
    [
        new(".gitignore", "*\n"),
        new("App.csproj", ScaffoldTemplates.Csproj(_packageVersion)),
        new("Program.cs", ScaffoldTemplates.Program(entryComponent)),
        new(ImportsFile, ScaffoldTemplates.Imports(folderNamespaces)),
        new(Path.Combine("Properties", "launchSettings.json"), ScaffoldTemplates.LaunchSettings(port)),
        new(Path.Combine("wwwroot", "index.html"), ScaffoldTemplates.IndexHtml),
    ];

    private static void CopyComponents(ComponentGraph graph, string targetDir)
    {
        foreach (var node in graph.Nodes)
        {
            var destination = Path.Combine(targetDir, node.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(node.FilePath, destination, overwrite: true);
        }
    }

    private static IReadOnlyList<string> FolderNamespaces(ComponentGraph graph)
    {
        var namespaces = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var node in graph.Nodes)
        {
            var folder = Path.GetDirectoryName(node.RelativePath);
            if (string.IsNullOrEmpty(folder))
                continue;

            var segments = folder
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Where(segment => segment.Length > 0)
                .Select(ComponentName.From);

            namespaces.Add($"{ScaffoldTemplates.RootNamespace}.{string.Join('.', segments)}");
        }

        return namespaces.ToList();
    }

    private static void RemoveStaleComponents(string targetDir)
    {
        foreach (var razor in Directory.EnumerateFiles(targetDir, "*.razor", SearchOption.AllDirectories))
        {
            if (!IsImports(razor))
                File.Delete(razor);
        }
    }

    private static bool IsImports(string path) =>
        string.Equals(Path.GetFileName(path), ImportsFile, StringComparison.OrdinalIgnoreCase);
}
