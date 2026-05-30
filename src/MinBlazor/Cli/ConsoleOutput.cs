namespace MinBlazor.Cli;

public sealed class ConsoleOutput : IOutput
{
    public void Info(string message) => Console.WriteLine(message);

    public void Error(string message)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"error: {message}");
        Console.ForegroundColor = previous;
    }
}
