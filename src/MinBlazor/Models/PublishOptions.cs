namespace MinBlazor.Models;

public sealed record PublishOptions
{
    public required string RazorFile { get; init; }
    public string? OutputDir { get; init; }
    public bool Clean { get; init; }
    public bool NoShadow { get; init; }
}
