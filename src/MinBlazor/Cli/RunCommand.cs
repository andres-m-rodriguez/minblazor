using System.Security.Cryptography;
using System.Text;
using MinBlazor.Models;
using MinBlazor.Razor;
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
        var scaffoldDir = CacheDirectory(razorPath);

        if (options.Clean && Directory.Exists(scaffoldDir))
        {
            _output.Info("Cleaning cache");
            Directory.Delete(scaffoldDir, recursive: true);
        }

        _output.Info($"running {Path.GetFileName(razorPath)} on http://localhost:{options.Port}");

        var entrySource = File.ReadAllText(razorPath);
        var entryName = ComponentName.From(Path.GetFileNameWithoutExtension(razorPath));

        var registry = new ComponentRegistry();
        try
        {
            FolderIndexer.Index(sourceDir, registry);
        }
        catch (DuplicateComponentException ex)
        {
            _output.Error(ex.Message);
            return 1;
        }

        var diagnostics = new Diagnostics();
        var compilation = new Compiler(registry, diagnostics).Compile(entryName, entrySource);

        foreach (var diagnostic in diagnostics.Items)
            _output.Info($"{diagnostic.Severity}: {diagnostic.Message}");

        new Scaffold().Write(scaffoldDir, sourceDir, compilation, options.Port);

        return Serve(scaffoldDir, options.OpenBrowser);
    }

    private static string CacheDirectory(string razorPath)
    {
        var path = Path.GetFullPath(razorPath);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path)))[..16].ToLowerInvariant();
        return Path.Combine(Path.GetTempPath(), "minblazor", hash);
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
