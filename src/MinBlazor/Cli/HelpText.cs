namespace MinBlazor.Cli;

internal static class HelpText
{
    public static string Version => $"minblazor {AppInfo.Version}";

    public const string Usage = """
        minblazor - run a single .razor file as a Blazor WebAssembly app.

        Usage:
          minblazor run <file.razor> [options]

        Options:
          -p, --port <n>   Port to serve on (default 5005)
              --no-open    Don't open the browser
              --clean      Delete the .minblazor folder before running
          -h, --help       Show help
          -v, --version    Show version
        """;
}
