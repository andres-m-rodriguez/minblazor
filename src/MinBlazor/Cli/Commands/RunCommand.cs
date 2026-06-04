using MinBlazor.Diff;
using MinBlazor.Models;
using MinBlazor.Parser;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class RunCommand(IOutput output)
{
    public int Execute(RunOptions options)
    {
        output.Info($"building {Path.GetFileName(options.RazorFile)}\n");

        var context = new PipelineContext(
            options.RazorFile,
            clean: options.Clean,
            port: options.Port,
            openBrowser: options.OpenBrowser,
            noShadow: options.NoShadow,
            hotReload: true
        );

        var initial = new PipelineRunner(output).RunUpTo(PipelineStep.Build, context);
        if (!initial.IsSuccess)
        {
            output.Error(initial.Error!);
            return 1;
        }

        var assets = StaticAssets.Load(context.ManifestPath!);
        if (!assets.IsSuccess)
        {
            output.Error(assets.Error!);
            return 1;
        }

        using var broadcaster = new ReloadBroadcaster();
        var bus = new HotReloadBus(broadcaster);
        using var server = new StaticServer(assets.Value!, options.Port, broadcaster);

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
            output.Error($"Failed to start server: {ex.Message}");
            return 1;
        }

        var url = $"http://localhost:{options.Port}/";
        output.Info($"\nServing on {url}  (Ctrl+C to stop)");

        if (options.OpenBrowser && !Browser.TryOpen(url))
            output.Info($"Open your browser at {url}");

        var lastDoc = context.Compilation!.Entry.Document;
        var sourceDir = Path.GetDirectoryName(options.RazorFile)!;

        using var watcher = new FileWatcher(
            sourceDir,
            () =>
            {
                var newSource = File.ReadAllText(options.RazorFile);
                var newDoc = new Transformer()
                    .Transform(new RazorParser(newSource).Parse())
                    .Document;
                var change = RazorDiff.Classify(lastDoc, newDoc);
                lastDoc = newDoc;

                switch (change)
                {
                    case RazorChange.None:
                        break;

                    case RazorChange.CssOnly css:
                        output.Info("reloading styles");
                        bus.Publish(new HotReloadEvent.CssUpdate(css.Content));
                        break;

                    case RazorChange.Full:
                        output.Info("rebuilding...");
                        bus.Publish(new HotReloadEvent.RebuildStarted());
                        var rebuildCtx = new PipelineContext(
                            options.RazorFile,
                            port: options.Port,
                            noShadow: options.NoShadow,
                            hotReload: true
                        );
                        var rebuild = new PipelineRunner(output).RunUpTo(
                            PipelineStep.Build,
                            rebuildCtx,
                            skip: new HashSet<PipelineStep>
                            {
                                PipelineStep.Shadow,
                                PipelineStep.Restore,
                            }
                        );
                        if (!rebuild.IsSuccess)
                        {
                            output.Error(rebuild.Error!);
                            bus.Publish(new HotReloadEvent.BuildFailed());
                            break;
                        }
                        var newAssets = StaticAssets.Load(rebuildCtx.ManifestPath!);
                        if (!newAssets.IsSuccess)
                        {
                            output.Error(newAssets.Error!);
                            break;
                        }
                        server.UpdateAssets(newAssets.Value!);
                        output.Info("reloading");
                        bus.Publish(new HotReloadEvent.Reload());
                        break;
                }
            }
        );

        server.WaitForExit();
        return 0;
    }
}
