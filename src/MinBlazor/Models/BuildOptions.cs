namespace MinBlazor.Models;

public sealed record BuildOptions
{
    public required string RazorFile { get; init; }
    public bool Clean { get; init; }
    public bool NoShadow { get; init; }
}

