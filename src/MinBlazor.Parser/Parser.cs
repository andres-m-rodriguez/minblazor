namespace MinBlazor.Parser;

public sealed class RazorParser
{
    private readonly Lexer _lexer;

    public RazorParser(string source) => _lexer = new Lexer(source);

    public Document Parse()
    {
        var source = _lexer.Source;
        var nodes = new List<RazorNode>();

        while (_lexer.Next() is { } token)
        {
            var text = source.Slice(token.Start, token.Length);
            RazorNode node =
                token.Kind == NodeKind.StyleBlock
                    ? new StyleBlockNode(text, StyleContent(text))
                    : new RazorNode(token.Kind, text);
            nodes.Add(node);
        }

        return new Document(source, nodes);
    }

    private static ReadOnlyMemory<char> StyleContent(ReadOnlyMemory<char> text)
    {
        var span = text.Span;
        var openEnd = span.IndexOf('>');
        if (openEnd < 0)
            return ReadOnlyMemory<char>.Empty;
        var closeStart = span.LastIndexOf('<');
        if (closeStart <= openEnd)
            return ReadOnlyMemory<char>.Empty;
        return text.Slice(openEnd + 1, closeStart - openEnd - 1);
    }
}
