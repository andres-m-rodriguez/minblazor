namespace MinBlazor.Services;

public sealed record GeneratedFile(string RelativePath, string Contents);

public sealed class Scaffolder
{
    private readonly string _packageVersion;

    public Scaffolder(string packageVersion) => _packageVersion = packageVersion;

    public IReadOnlyList<GeneratedFile> Plan(string componentName, int port) =>
    [
        new(".gitignore", "*\n"),
        new("App.csproj", ScaffoldTemplates.Csproj(_packageVersion)),
        new("Program.cs", ScaffoldTemplates.Program(componentName)),
        new("_Imports.razor", ScaffoldTemplates.Imports),
        new(Path.Combine("Properties", "launchSettings.json"), ScaffoldTemplates.LaunchSettings(port)),
        new(Path.Combine("wwwroot", "index.html"), ScaffoldTemplates.IndexHtml),
    ];

    public void Create(string targetDir, string razorPath, string componentName, int port)
    {
        Directory.CreateDirectory(targetDir);

        File.Copy(razorPath, Path.Combine(targetDir, $"{componentName}.razor"), overwrite: true);

        foreach (var file in Plan(componentName, port))
        {
            var fullPath = Path.Combine(targetDir, file.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, file.Contents);
        }
    }
}
