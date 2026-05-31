using MinBlazor.Models;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class RunCommand
{
    private readonly IOutput _output;

    public RunCommand(IOutput output) => _output = output;

    public int Execute(RunOptions options)
    {
        _output.Info($"running {Path.GetFileName(options.RazorFile)} on http://localhost:{options.Port}");

        var prepared = new Pipeline(_output).Prepare(options.RazorFile, options.Clean);
        if (!prepared.IsSuccess)
        {
            _output.Error(prepared.Error!);
            return 1;
        }

        return Serve(prepared.Value!, options.Port, options.OpenBrowser);
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
