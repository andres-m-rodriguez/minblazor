using System.Security.Cryptography;
using System.Text;
using MinBlazor.Models;
using MinBlazor.Razor;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class RunCommand
{
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
        var indexed = FolderIndexer.Index(sourceDir, registry);
        if (!indexed.IsSuccess)
        {
            _output.Error(indexed.Error!);
            return 1;
        }

        var diagnostics = new Diagnostics();
        var compilation = new Compiler(registry, diagnostics).Compile(entryName, entrySource);

        foreach (var diagnostic in diagnostics.Items)
            _output.Info($"{diagnostic.Severity}: {diagnostic.Message}");

        new Scaffold().Write(scaffoldDir, sourceDir, compilation, options.Port);

        return Serve(scaffoldDir, options.Port, options.OpenBrowser);
    }

    private static string CacheDirectory(string razorPath)
    {
        var path = Path.GetFullPath(razorPath);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path)))[..16].ToLowerInvariant();
        return Path.Combine(Path.GetTempPath(), "minblazor", hash);
    }

    private int Serve(string scaffoldDir, int port, bool openBrowser)
    {
        var builder = new Builder();
        builder.Output += _output.Info;

        _output.Info("Building (first run may take a while)\n");

        var built = builder.Build(scaffoldDir);
        if (!built.IsSuccess)
        {
            _output.Error(built.Error!);
            return 1;
        }

        var assets = StaticAssets.Load(built.Value!);
        if (!assets.IsSuccess)
        {
            _output.Error(assets.Error!);
            return 1;
        }

        using var server = new StaticServer(assets.Value!, port);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            server.Stop();
        };

        try
        {
            server.Start();
        }
        catch (Exception ex)
        {
            _output.Error($"Failed to start server: {ex.Message}");
            return 1;
        }

        var url = $"http://localhost:{port}/";
        _output.Info($"\nServing on {url}  (Ctrl+C to stop)");

        if (openBrowser && !Browser.TryOpen(url))
            _output.Info($"Open your browser at {url}");

        server.WaitForExit();
        return 0;
    }
}
