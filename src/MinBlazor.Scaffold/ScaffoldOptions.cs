using MinBlazor.Parser;

namespace MinBlazor.Scaffold;

public sealed record ScaffoldOptions(
    string BlazorPackageVersion,
    int Port,
    bool HasDependencies,
    IReadOnlyList<string> PackageUsings
);
