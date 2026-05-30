namespace MinBlazor.Models;

public sealed record RunOptions
{
    public required string RazorFile { get; init; }
    public int Port { get; init; } = AppInfo.DefaultPort;
    public bool OpenBrowser { get; init; } = true;
    public bool Clean { get; init; }
}
