namespace MinBlazor.Razor.Models;

public enum TokenKind
{
    Text,
    ComponentOpen,
    ComponentClose,
    ComponentSelfClose,
    HostTag,
}

public readonly record struct Token(TokenKind Kind, int Start, int End)
{
    public int Length => End - Start;
}
