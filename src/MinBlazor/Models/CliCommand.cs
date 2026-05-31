namespace MinBlazor.Models;

public abstract record CliCommand
{
    public sealed record Run(RunOptions Options) : CliCommand;

    public sealed record ShowHelp : CliCommand;

    public sealed record ShowVersion : CliCommand;
}
