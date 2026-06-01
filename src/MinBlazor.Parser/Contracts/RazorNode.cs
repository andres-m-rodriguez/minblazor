namespace MinBlazor.Parser;

public sealed record RazorNode(NodeKind Kind, ReadOnlyMemory<char> Text);
