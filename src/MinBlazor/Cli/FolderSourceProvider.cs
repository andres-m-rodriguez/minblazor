using MinBlazor.Index;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class FolderSourceProvider(string rootDir) : ISourceComponentProvider
{
    public IEnumerable<(string Name, string Path)> GetComponents()
    {
        foreach (
            var path in Directory.EnumerateFiles(rootDir, "*.razor", SearchOption.AllDirectories)
        )
        {
            if (IsScaffold(path))
                continue;

            yield return (
                ComponentName.From(System.IO.Path.GetFileNameWithoutExtension(path)),
                path
            );
        }
    }

    private static bool IsScaffold(string path) =>
        path.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar)
            .Contains(".minblazor");
}
