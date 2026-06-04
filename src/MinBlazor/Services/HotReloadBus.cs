using System.Text.Json;

namespace MinBlazor.Services;

public sealed class HotReloadBus(ReloadBroadcaster broadcaster)
{
    public void Publish(HotReloadEvent evt)
    {
        var message = evt switch
        {
            HotReloadEvent.RebuildStarted => """{"type":"rebuild-started"}""",
            HotReloadEvent.BuildFailed => """{"type":"build-failed"}""",
            HotReloadEvent.Reload => """{"type":"reload"}""",
            HotReloadEvent.CssUpdate css =>
                $$"""{"type":"css","content":{{JsonSerializer.Serialize(css.Content)}}}""",
            _ => throw new ArgumentOutOfRangeException(nameof(evt)),
        };

        broadcaster.Broadcast(message);
    }
}
