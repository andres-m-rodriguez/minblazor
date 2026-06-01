namespace MinBlazor.Razor.Models;

public sealed record RazorNode(NodeKind Kind, ReadOnlyMemory<char> Text);
