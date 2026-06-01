using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class RazorParser
{
    private readonly Lexer _lexer;

    public RazorParser(string source) => _lexer = new Lexer(source);

    public Document Parse()
    {
        var source = _lexer.Source;
        var nodes = new List<RazorNode>();

        while (_lexer.Next() is { } token)
            nodes.Add(new RazorNode(token.Kind, source.Slice(token.Start, token.Length)));

        return new Document(source, nodes);
    }
}


