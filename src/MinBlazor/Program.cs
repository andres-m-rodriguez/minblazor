using MinBlazor.Cli;
using MinBlazor.Models;

IOutput output = new ConsoleOutput();

var parsed = ArgumentParser.Parse(args);
if (!parsed.IsSuccess)
{
    output.Error(parsed.Error!);
    return 1;
}

switch (parsed.Value)
{
    case CliCommand.ShowHelp:
        output.Info(HelpText.Usage);
        return 0;

    case CliCommand.ShowVersion:
        output.Info(HelpText.Version);
        return 0;

    case CliCommand.Run run:
        return new RunCommand(output).Execute(run.Options);

    case CliCommand.Build build:
        return new BuildCommand(output).Execute(build.Options);

    default:
        return 1;
}
