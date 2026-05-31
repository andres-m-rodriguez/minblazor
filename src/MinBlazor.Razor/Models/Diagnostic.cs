namespace MinBlazor.Razor.Models;

public enum DiagnosticSeverity
{
    Message,
    Warning,
    Error,
}

public sealed record Diagnostic(DiagnosticSeverity Severity, string Message);
