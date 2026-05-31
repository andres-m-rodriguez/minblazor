using MinBlazor.Razor;

namespace MinBlazor.Cli;

public sealed class FileComponentSource(string path) : IComponentSource
{
    public string Read() => File.ReadAllText(path);
}
