namespace MinBlazor.Services;

public abstract record HotReloadEvent
{
    public sealed record RebuildStarted : HotReloadEvent;
    public sealed record BuildFailed : HotReloadEvent;
    public sealed record Reload : HotReloadEvent;
    public sealed record CssUpdate(string Content) : HotReloadEvent;
}
