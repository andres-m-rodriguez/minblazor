using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace MinBlazor.Lsp;

public sealed class DocumentStore
{
    private readonly ConcurrentDictionary<DocumentUri, string> _documents = new();

    public void Set(DocumentUri uri, string text) => _documents[uri] = text;

    public void Remove(DocumentUri uri) => _documents.TryRemove(uri, out _);

    public string? Get(DocumentUri uri) => _documents.TryGetValue(uri, out var text) ? text : null;
}
