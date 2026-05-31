namespace MinBlazor.Razor.Models;

public sealed record TransformResult(
    Document Document,
    IReadOnlyList<HostTag> HostTags,
    IReadOnlyList<PackageReference> Packages
);
