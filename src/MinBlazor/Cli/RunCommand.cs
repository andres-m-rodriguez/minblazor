using MinBlazor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class RunCommand
{
    private const string ListeningMarker = "Now listening on:";

    private readonly IOutput _output;

    public RunCommand(IOutput output) => _output = output;

    public int Execute(RunOptions options)
    {
        var razorPath = options.RazorFile;
        var sourceDir = Path.GetDirectoryName(razorPath)!;
        var scaffoldDir = Path.Combine(sourceDir, ".minblazor");

        if (options.Clean && Directory.Exists(scaffoldDir))
        {
            _output.Info("Cleaning .minblazor");
            Directory.Delete(scaffoldDir, recursive: true);
        }

        _output.Info($"running {Path.GetFileName(razorPath)} on http://localhost:{options.Port}");

        var graph = new Scaffolder(AppInfo.BlazorPackageVersion)
            .Create(scaffoldDir, sourceDir, razorPath, options.Port);

        _output.Info($"components: {string.Join(", ", graph.Nodes.Select(n => n.RelativePath))}");

        return Serve(scaffoldDir, options.OpenBrowser);
    }

    private int Serve(string scaffoldDir, bool openBrowser)
    {
        using var server = new DevServer(scaffoldDir);
        var browserOpened = false;

        server.OutputLine += line =>
        {
            _output.Info(line);

            if (openBrowser && !browserOpened && TryReadListeningUrl(line, out var url))
            {
                browserOpened = true;
                if (!Browser.TryOpen(url))
                    _output.Info($"Open your browser at {url}");
            }
        };
        server.ErrorLine += _output.Info;

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            server.Stop();
        };

        _output.Info("Starting dev server (first run may take a moment)\n");

        try
        {
            server.Start();
        }
        catch (Exception ex)
        {
            _output.Error($"Failed to launch dotnet: {ex.Message}");
            return 1;
        }

        return server.WaitForExit();
    }

    private static bool TryReadListeningUrl(string line, out string url)
    {
        var index = line.IndexOf(ListeningMarker, StringComparison.Ordinal);
        if (index < 0)
        {
            url = string.Empty;
            return false;
        }

        url = line[(index + ListeningMarker.Length)..].Trim();
        return url.Length > 0;
    }
}
