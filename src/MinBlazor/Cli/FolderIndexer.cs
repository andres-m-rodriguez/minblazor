using MinBlazor.Razor;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public static class FolderIndexer
{
    public static void Index(string rootDir, ComponentRegistry registry)
    {
        foreach (var path in Directory.EnumerateFiles(rootDir, "*.razor", SearchOption.AllDirectories))
        {
            if (IsScaffold(path))
                continue;

            registry.Add(ComponentName.From(Path.GetFileNameWithoutExtension(path)), new FileComponentSource(path));
        }
    }

    private static bool IsScaffold(string path) =>
        path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains(".minblazor");
}
