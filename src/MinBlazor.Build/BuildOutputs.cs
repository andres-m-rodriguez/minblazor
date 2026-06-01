using MinBlazor.Build.Models;
using MinBlazor.Razor.Models;

namespace MinBlazor.Build;

public sealed class BuildOutputs
{
    public List<PackageReference> Packages { get; } = [];
    public Dictionary<string, string> Properties { get; } = new(StringComparer.Ordinal);
    public List<string> HeadTags { get; } = [];
    public List<BuildAsset> Assets { get; } = [];
    public List<BuildSource> Sources { get; } = [];
    public List<string> SourceDirectories { get; } = [];
    public Dictionary<string, string> Options { get; } = new(StringComparer.Ordinal);
    public List<VirtualComponent> Components { get; } = [];
}
