using MinBlazor.Parser;

namespace MinBlazor.Compiler;

public sealed record CompiledComponent(
    string Name,
    Document Document,
    IReadOnlyList<HostTag> HostTags,
    IReadOnlySet<string> References,
    IReadOnlyList<PackageReference> Packages
);
