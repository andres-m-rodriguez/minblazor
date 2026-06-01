namespace MinBlazor.Razor.Models;

public sealed record Document(ReadOnlyMemory<char> Source, IReadOnlyList<RazorNode> Nodes);
