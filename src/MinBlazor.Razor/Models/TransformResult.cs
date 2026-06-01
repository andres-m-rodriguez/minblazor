namespace MinBlazor.Razor.Models;

public sealed record TransformResult(
    Document Document,
    IReadOnlyList<RazorNode> HostTags,
    IReadOnlyList<PackageReference> Packages
);

