namespace MinBlazor.Models;

public abstract record CliCommand
{
    public sealed record Run(RunOptions Options) : CliCommand;

    public sealed record Build(BuildOptions Options) : CliCommand;

    public sealed record Graph(GraphOptions Options) : CliCommand;

    public sealed record ShowHelp : CliCommand;

    public sealed record ShowVersion : CliCommand;
}
