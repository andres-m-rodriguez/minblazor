namespace MinBlazor.Compiler;

public sealed class InlineComponentSource(string source) : IComponentSource
{
    public string Read() => source;
}
