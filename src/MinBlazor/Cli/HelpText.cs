namespace MinBlazor.Cli;

internal static class HelpText
{
    public static string Version => $"minblazor {AppInfo.Version}";

    public const string Usage = """
        minblazor - run a single .razor file as a Blazor WebAssembly app.

        Usage:
          minblazor run <file.razor> [options]      Build, serve and watch
          minblazor build <file.razor> [options]    Build only
          minblazor restore <file.razor> [options]  Restore packages only
          minblazor publish <file.razor> [options]  Publish optimized static output
          minblazor clean [file.razor]              Delete build cache
          minblazor list                            Show cached scaffolds

        Options:
          -p, --port <n>   Port to serve on (default 5005)   (run only)
              --no-open    Don't open the browser             (run only)
          -o, --output <dir>  Output directory              (publish only)
              --clean      Delete the build cache first
              --no-shadow  Skip editor shadow project
          -h, --help       Show help
          -v, --version    Show version
        """;
}
