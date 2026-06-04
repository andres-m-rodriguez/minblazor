namespace MinBlazor.Models;

public abstract record CliCommand
{
    public sealed record Run(RunOptions Options) : CliCommand;

    public sealed record Build(BuildOptions Options) : CliCommand;

    public sealed record Restore(RestoreOptions Options) : CliCommand;

    public sealed record Clean(CleanOptions Options) : CliCommand;

    public sealed record List(ListOptions Options) : CliCommand;

    public sealed record Publish(PublishOptions Options) : CliCommand;

    public sealed record ShowHelp : CliCommand;

    public sealed record ShowVersion : CliCommand;
}
