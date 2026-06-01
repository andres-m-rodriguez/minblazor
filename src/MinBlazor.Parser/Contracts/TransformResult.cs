namespace MinBlazor.Parser;

public sealed record TransformResult(
    Document Document,
    IReadOnlyList<RazorNode> HostTags,
    IReadOnlyList<PackageReference> Packages
);

