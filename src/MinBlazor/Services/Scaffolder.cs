namespace MinBlazor.Services;

public sealed record GeneratedFile(string RelativePath, string Contents);

public sealed class Scaffolder
{
    private const string ImportsFile = "_Imports.razor";

    private readonly string _packageVersion;

    public Scaffolder(string packageVersion) => _packageVersion = packageVersion;

    public IReadOnlyList<GeneratedFile> Plan(string entryComponent, int port) =>
    [
        new(".gitignore", "*\n"),
        new("App.csproj", ScaffoldTemplates.Csproj(_packageVersion)),
        new("Program.cs", ScaffoldTemplates.Program(entryComponent)),
        new(ImportsFile, ScaffoldTemplates.Imports),
        new(Path.Combine("Properties", "launchSettings.json"), ScaffoldTemplates.LaunchSettings(port)),
        new(Path.Combine("wwwroot", "index.html"), ScaffoldTemplates.IndexHtml),
    ];

    public void Create(string targetDir, string sourceDir, string entryComponent, int port)
    {
        Directory.CreateDirectory(targetDir);
        RemoveStaleComponents(targetDir);
        CopyComponents(sourceDir, targetDir);

        foreach (var file in Plan(entryComponent, port))
        {
            var fullPath = Path.Combine(targetDir, file.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, file.Contents);
        }
    }

    private static void CopyComponents(string sourceDir, string targetDir)
    {
        foreach (var razor in Directory.EnumerateFiles(sourceDir, "*.razor"))
        {
            if (IsImports(razor))
                continue;

            var component = ComponentName.From(Path.GetFileNameWithoutExtension(razor));
            File.Copy(razor, Path.Combine(targetDir, $"{component}.razor"), overwrite: true);
        }
    }

    private static void RemoveStaleComponents(string targetDir)
    {
        foreach (var razor in Directory.EnumerateFiles(targetDir, "*.razor"))
        {
            if (!IsImports(razor))
                File.Delete(razor);
        }
    }

    private static bool IsImports(string path) =>
        string.Equals(Path.GetFileName(path), ImportsFile, StringComparison.OrdinalIgnoreCase);
}
