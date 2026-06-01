using MinBlazor.Parser;

namespace MinBlazor.Compiler;

public sealed record Compilation(
    CompiledComponent Entry,
    IReadOnlyList<CompiledComponent> Components,
    IReadOnlyList<PackageReference> Packages
);
