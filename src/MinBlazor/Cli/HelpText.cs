namespace MinBlazor.Cli;

internal static class HelpText
{
    public static string Version => $"minblazor {AppInfo.Version}";

    public const string Usage = """
        minblazor - run a single .razor file as a Blazor WebAssembly app.

        Usage:
          minblazor run <file.razor> [options]      Build and serve with a dev server
          minblazor build <file.razor> [options]    Build only, no server
          minblazor graph <file.razor> [format]     Print the component dependency tree

        Options:
          -p, --port <n>   Port to serve on (default 5005)   (run only)
              --no-open    Don't open the browser             (run only)
              --clean      Delete the build cache first
              --json       Output the graph as JSON           (graph only)
              --mermaid    Output the graph as a Mermaid chart (graph only)
          -h, --help       Show help
          -v, --version    Show version
        """;
}
