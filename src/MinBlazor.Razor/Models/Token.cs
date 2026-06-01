namespace MinBlazor.Razor.Models;

internal readonly record struct Token(NodeKind Kind, int Start, int End)
{
    public int Length => End - Start;
}
