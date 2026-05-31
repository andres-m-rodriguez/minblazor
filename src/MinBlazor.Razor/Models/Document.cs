namespace MinBlazor.Razor.Models;

public sealed record Document(ReadOnlyMemory<char> Source, IReadOnlyList<Token> Tokens)
{
    public ReadOnlySpan<char> Text(Token token) => Source.Span.Slice(token.Start, token.Length);
}
