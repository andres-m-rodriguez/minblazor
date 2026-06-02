using MinBlazor.Core;
using MinBlazor.Services;

namespace MinBlazor.Cli.Steps;

public sealed class ServeStep(IOutput output) : IPipelineStep
{
    public PipelineStep Step => PipelineStep.Serve;

    public Result Execute(PipelineContext context)
    {
        var assets = StaticAssets.Load(context.ManifestPath!);
        if (!assets.IsSuccess)
            return Result.Fail(assets.Error!);

        using var server = new StaticServer(assets.Value!, context.Port);

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
            return Result.Fail($"Failed to start server: {ex.Message}");
        }

        var url = $"http://localhost:{context.Port}/";
        output.Info($"\nServing on {url}  (Ctrl+C to stop)");

        if (context.OpenBrowser && !Browser.TryOpen(url))
            output.Info($"Open your browser at {url}");

        server.WaitForExit();
        return Result.Ok();
    }
}
