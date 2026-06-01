namespace MinBlazor.Compiler;

public sealed class Diagnostics
{
    private readonly List<Diagnostic> _items = [];

    public IReadOnlyList<Diagnostic> Items => _items;

    public void Message(string message) => Add(DiagnosticSeverity.Message, message);

    public void Warning(string message) => Add(DiagnosticSeverity.Warning, message);

    public void Error(string message) => Add(DiagnosticSeverity.Error, message);

    private void Add(DiagnosticSeverity severity, string message) =>
        _items.Add(new Diagnostic(severity, message));
}
