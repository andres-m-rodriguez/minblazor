using MinBlazor.Models;

namespace MinBlazor.Cli;

public static class ArgumentParser
{
    public static Result<CliCommand> Parse(string[] args)
    {
        if (args.Length == 0)
            return Ok(new CliCommand.ShowHelp());

        return args[0] switch
        {
            "--help" or "-h" or "help" => Ok(new CliCommand.ShowHelp()),
            "--version" or "-v" => Ok(new CliCommand.ShowVersion()),
            "run" => ParseRun(args),
            "build" => ParseBuild(args),
            "build2" => ParseBuild2(args),
            var other => Fail($"Unknown command '{other}'. Try 'minblazor --help'."),
        };
    }

    private static Result<CliCommand> ParseBuild(string[] args)
    {
        string? file = null;
        bool clean = false;

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--clean":
                    clean = true;
                    break;

                default:
                    if (arg.StartsWith('-'))
                        return Fail($"Unknown option '{arg}'.");
                    if (file is not null)
                        return Fail("Only one .razor file can be built at a time.");
                    file = arg;
                    break;
            }
        }

        if (file is null)
            return Fail("No .razor file given. Usage: minblazor build Index.razor");

        var razorPath = Path.GetFullPath(file);
        if (!razorPath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            return Fail($"Expected a .razor file, got: {Path.GetFileName(razorPath)}");
        if (!File.Exists(razorPath))
            return Fail($"File not found: {razorPath}");

        return Ok(new CliCommand.Build(new BuildOptions { RazorFile = razorPath, Clean = clean }));
    }

    private static Result<CliCommand> ParseRun(string[] args)
    {
        string? file = null;
        int port = AppInfo.DefaultPort;
        bool open = true;
        bool clean = false;

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--port" or "-p":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out port))
                        return Fail("--port requires a number, e.g. --port 5005");
                    if (port is < 1 or > 65535)
                        return Fail("--port must be between 1 and 65535.");
                    break;

                case "--no-open":
                    open = false;
                    break;

                case "--clean":
                    clean = true;
                    break;

                default:
                    if (arg.StartsWith('-'))
                        return Fail($"Unknown option '{arg}'.");
                    if (file is not null)
                        return Fail("Only one .razor file can be run at a time.");
                    file = arg;
                    break;
            }
        }

        if (file is null)
            return Fail("No .razor file given. Usage: minblazor run Index.razor");

        var razorPath = Path.GetFullPath(file);
        if (!razorPath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            return Fail($"Expected a .razor file, got: {Path.GetFileName(razorPath)}");
        if (!File.Exists(razorPath))
            return Fail($"File not found: {razorPath}");

        var options = new RunOptions
        {
            RazorFile = razorPath,
            Port = port,
            OpenBrowser = open,
            Clean = clean,
        };
        return Ok(new CliCommand.Run(options));
    }

    private static Result<CliCommand> ParseBuild2(string[] args)
    {
        string? file = args.Skip(1).FirstOrDefault(a => !a.StartsWith('-'));

        if (file is null)
            return Fail("No .razor file given. Usage: minblazor build2 Index.razor");

        var razorPath = Path.GetFullPath(file);
        if (!razorPath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            return Fail($"Expected a .razor file, got: {Path.GetFileName(razorPath)}");
        if (!File.Exists(razorPath))
            return Fail($"File not found: {razorPath}");

        return Ok(new CliCommand.Build2(new Build2Options { RazorFile = razorPath }));
    }

    private static Result<CliCommand> Ok(CliCommand command) => Result<CliCommand>.Ok(command);

    private static Result<CliCommand> Fail(string message) => Result<CliCommand>.Fail(message);
}
