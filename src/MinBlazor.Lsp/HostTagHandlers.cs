using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace MinBlazor.Lsp;

// Semantic tokens so @hostTag stays highlighted after Roslyn takes over
public sealed class HostTagSemanticTokensHandler(DocumentStore documents)
    : ISemanticTokensFullHandler
{
    private const string Marker = "@hostTag";
    private static readonly TextDocumentSelector Selector = TextDocumentSelector.ForPattern("**/*.razor");

    private static readonly SemanticTokensLegend Legend = new()
    {
        TokenTypes = new Container<SemanticTokenType>(SemanticTokenType.Keyword),
        TokenModifiers = new Container<SemanticTokenModifier>(),
    };

    public SemanticTokensRegistrationOptions GetRegistrationOptions(
        SemanticTokensCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = Selector, Legend = Legend, Full = true };

    public Task<SemanticTokens?> Handle(SemanticTokensParams request, CancellationToken cancellationToken)
    {
        var text = documents.Get(request.TextDocument.Uri);
        if (text is null) return Task.FromResult<SemanticTokens?>(null);

        var data = new List<int>();
        int prevLine = 0, prevCol = 0;

        int pos = 0;
        while ((pos = text.IndexOf(Marker, pos, StringComparison.Ordinal)) >= 0)
        {
            var (line, col) = LineCol(text, pos);
            data.Add(line - prevLine);
            data.Add(line == prevLine ? col - prevCol : col);
            data.Add(Marker.Length);
            data.Add(0); // keyword token type index
            data.Add(0); // no modifiers
            prevLine = line;
            prevCol = col;
            pos += Marker.Length;
        }

        return Task.FromResult<SemanticTokens?>(new SemanticTokens { Data = [..data] });
    }

    private static (int Line, int Col) LineCol(string text, int offset)
    {
        int line = 0, col = 0;
        for (int i = 0; i < offset && i < text.Length; i++)
        {
            if (text[i] == '\n') { line++; col = 0; }
            else col++;
        }
        return (line, col);
    }
}

// Completion: suggest @hostTag when typing @ in a .razor file
public sealed class HostTagCompletionHandler : ICompletionHandler
{
    private static readonly TextDocumentSelector Selector = TextDocumentSelector.ForPattern("**/*.razor");

    private static readonly CompletionItem HostTagItem = new()
    {
        Label = "@hostTag",
        Kind = CompletionItemKind.Keyword,
        InsertText = "hostTag",
        Detail = "minblazor dialect",
        Documentation = new StringOrMarkupContent(new MarkupContent
        {
            Kind = MarkupKind.Markdown,
            Value = "Hoists this element into the host page `<head>` at build time.\n\n```razor\n<link @hostTag href=\"...\" rel=\"stylesheet\" />\n```",
        }),
    };

    public CompletionRegistrationOptions GetRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = Selector, TriggerCharacters = new Container<string>("@") };

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken) =>
        Task.FromResult(new CompletionList(HostTagItem));
}
