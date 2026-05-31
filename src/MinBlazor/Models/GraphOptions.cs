namespace MinBlazor.Models;

public sealed record GraphOptions
{
    public required string RazorFile { get; init; }
    public bool Json { get; init; }
}
