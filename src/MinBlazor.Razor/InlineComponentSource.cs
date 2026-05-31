namespace MinBlazor.Razor;

public sealed class InlineComponentSource(string source) : IComponentSource
{
    public string Read() => source;
}
