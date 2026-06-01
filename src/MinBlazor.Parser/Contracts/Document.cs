namespace MinBlazor.Parser;

public sealed record Document(ReadOnlyMemory<char> Source, IReadOnlyList<RazorNode> Nodes);
