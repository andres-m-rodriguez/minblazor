using MinBlazor.Cli;

namespace MinBlazor.Lsp;

// The server speaks LSP over stdout, so the pipeline must never write there.
public sealed class NullOutput : IOutput
{
    public static readonly NullOutput Instance = new();

    public void Info(string message) { }

    public void Error(string message) { }
}
