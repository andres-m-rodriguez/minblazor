using MinBlazor.Parser;

namespace MinBlazor.Build.Models;

public sealed record CompilationInfo(
    string EntryName,
    IReadOnlyList<string> Components,
    IReadOnlyList<PackageReference> Packages
)
{
    public bool Uses(string componentName) => Components.Contains(componentName);
}
