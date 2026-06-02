namespace MinBlazor.Models;

public sealed record CleanOptions
{
    public string? RazorFile { get; init; } // null = clean all
}
