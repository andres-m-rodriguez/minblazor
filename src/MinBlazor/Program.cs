using Microsoft.Build.Locator;
using MinBlazor.Cli;
using MinBlazor.Models;

MSBuildLocator.RegisterDefaults();

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
    case CliCommand.Restore r:
        return new RestoreCommand(output).Execute(r.Options);
    case CliCommand.Clean c:
        return new CleanCommand(output).Execute(c.Options);
    case CliCommand.List l:
        return new ListCommand(output).Execute(l.Options);
    case CliCommand.Publish p:
        return new PublishCommand(output).Execute(p.Options);
    default:
        return 1;
}
