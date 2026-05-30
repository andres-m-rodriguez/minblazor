namespace MinBlazor.Cli;

public interface IOutput
{
    void Info(string message);
    void Error(string message);
}
