namespace MinBlazor.Parser;

public sealed record StyleBlockNode(ReadOnlyMemory<char> Text, ReadOnlyMemory<char> Content)
    : RazorNode(NodeKind.StyleBlock, Text);
