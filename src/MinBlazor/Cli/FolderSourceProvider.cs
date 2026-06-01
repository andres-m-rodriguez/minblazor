using MinBlazor.Index;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class FolderSourceProvider(string rootDir) : ISourceComponentProvider
{
    public IEnumerable<(string Name, string Path)> GetComponents()
    {
        foreach (var path in Directory.EnumerateFiles(rootDir, "*.razor", SearchOption.TopDirectoryOnly))
            yield return (ComponentName.From(System.IO.Path.GetFileNameWithoutExtension(path)), path);
    }
}
