using MinBlazor.Cli;
using MinBlazor.Services;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace MinBlazor.Lsp;

public sealed class ComponentCompletionHandler : ICompletionHandler
{
    private static readonly TextDocumentSelector Selector = TextDocumentSelector.ForPattern("**/*.razor");

    private readonly DocumentStore _documents;

    public ComponentCompletionHandler(DocumentStore documents) => _documents = documents;

    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = Selector,
            TriggerCharacters = new Container<string>("<"),
        };

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var text = _documents.Get(request.TextDocument.Uri);
        if (text is null)
            return Task.FromResult(new CompletionList());

        var partial = TagCompletion.TagNameAt(text, Offset(text, request.Position));
        if (partial is null)
            return Task.FromResult(new CompletionList());

        var path = request.TextDocument.Uri.GetFileSystemPath();
        if (path is null)
            return Task.FromResult(new CompletionList());

        var available = new Pipeline(NullOutput.Instance).AvailableComponents(path);
        if (!available.IsSuccess)
            return Task.FromResult(new CompletionList());

        var current = ComponentName.From(Path.GetFileNameWithoutExtension(path));

        var items = available.Value!
            .Where(name => name != current && name.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
            .Select(name => new CompletionItem
            {
                Label = name,
                Kind = CompletionItemKind.Class,
                InsertText = name,
            });

        return Task.FromResult(new CompletionList(items));
    }

    private static int Offset(string text, Position position)
    {
        int line = 0;
        int i = 0;
        while (i < text.Length && line < position.Line)
        {
            if (text[i] == '\n')
                line++;
            i++;
        }

        return Math.Min(i + position.Character, text.Length);
    }
}
