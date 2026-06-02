using System.Text.Json;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace MinBlazor.Lsp;

public sealed class VirtualComponentCompletionHandler : ICompletionHandler
{
    private const string CacheFile = ".virtual-components.json";
    private const string ShadowDir = "_minblazor";

    private static readonly TextDocumentSelector Selector =
        TextDocumentSelector.ForPattern("**/*.razor");

    public CompletionRegistrationOptions GetRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = Selector, TriggerCharacters = new Container<string>("<") };

    public Task<CompletionList> Handle(
        CompletionParams request, CancellationToken cancellationToken)
    {
        var path = request.TextDocument.Uri.GetFileSystemPath();
        if (path is null) return Task.FromResult(new CompletionList());

        var sourceDir = Path.GetDirectoryName(path)!;
        var cachePath = Path.Combine(sourceDir, ShadowDir, CacheFile);

        if (!File.Exists(cachePath)) return Task.FromResult(new CompletionList());

        string[] names;
        try
        {
            names = JsonSerializer.Deserialize<string[]>(File.ReadAllText(cachePath)) ?? [];
        }
        catch
        {
            return Task.FromResult(new CompletionList());
        }

        if (names.Length == 0) return Task.FromResult(new CompletionList());

        var items = names.Select(name => new CompletionItem
        {
            Label = name,
            Kind = CompletionItemKind.Class,
            Detail = "virtual component (Build.cs)",
            InsertText = name,
        });

        return Task.FromResult(new CompletionList(items));
    }
}
