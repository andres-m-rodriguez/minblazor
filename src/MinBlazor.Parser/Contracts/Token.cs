namespace MinBlazor.Parser;

public enum NodeKind
{
    Text,
    ComponentOpen,
    ComponentClose,
    ComponentSelfClose,
    HostTag,
    Directive,
}

internal readonly record struct Token(NodeKind Kind, int Start, int End)
{
    public int Length => End - Start;
}
