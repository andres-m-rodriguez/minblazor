using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Parser(Lexer lexer)
{
    private readonly Lexer _lexer = lexer;

    public Document Parse()
    {
        var tokens = new List<Token>();

        while (_lexer.Next() is { } token)
            tokens.Add(token);

        return new Document(_lexer.Source, tokens);
    }
}
