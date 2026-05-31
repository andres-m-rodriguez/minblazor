namespace MinBlazor.Razor.Models;

public sealed record Compilation(
    CompiledComponent Entry,
    IReadOnlyList<CompiledComponent> Components,
    IReadOnlyList<PackageReference> Packages
);
