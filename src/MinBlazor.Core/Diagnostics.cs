namespace MinBlazor.Core;

public enum DiagnosticSeverity { Warning, Error }

public sealed record Diagnostic(DiagnosticSeverity Severity, string Message);

public interface IDiagnostics
{
    void Warning(string message);
    void Error(string message);
}

public sealed class Diagnostics : IDiagnostics
{
    private readonly List<Diagnostic> _items = [];

    public IReadOnlyList<Diagnostic> Items => _items;

    public void Warning(string message) => _items.Add(new Diagnostic(DiagnosticSeverity.Warning, message));

    public void Error(string message) => _items.Add(new Diagnostic(DiagnosticSeverity.Error, message));
}
