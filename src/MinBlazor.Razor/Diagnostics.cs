using MinBlazor.Core;
using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Diagnostics : IDiagnostics
{
    private readonly List<Diagnostic> _items = [];

    public IReadOnlyList<Diagnostic> Items => _items;

    public void Message(string message) => Add(DiagnosticSeverity.Message, message);

    public void Warning(string message) => Add(DiagnosticSeverity.Warning, message);

    public void Error(string message) => Add(DiagnosticSeverity.Error, message);

    private void Add(DiagnosticSeverity severity, string message) =>
        _items.Add(new Diagnostic(severity, message));
}
