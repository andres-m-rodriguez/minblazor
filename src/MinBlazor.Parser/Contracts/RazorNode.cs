namespace MinBlazor.Parser;

public record RazorNode(NodeKind Kind, ReadOnlyMemory<char> Text);
