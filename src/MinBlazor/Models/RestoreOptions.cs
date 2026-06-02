namespace MinBlazor.Models;

public sealed record RestoreOptions
{
    public required string RazorFile { get; init; }
    public bool NoShadow { get; init; }
}
