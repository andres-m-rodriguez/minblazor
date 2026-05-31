using System.Diagnostics.CodeAnalysis;
using MinBlazor.Razor;
using MinBlazor.Services;

namespace MinBlazor.Cli;

public sealed class FolderResolver : IComponentResolver
{
    private readonly Dictionary<string, string> _paths = new(StringComparer.Ordinal);

    public FolderResolver(string rootDir)
    {
        foreach (
            var path in Directory.EnumerateFiles(rootDir, "*.razor", SearchOption.AllDirectories)
        )
        {
            if (
                path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Contains(".minblazor")
            )
                continue;

            _paths.TryAdd(ComponentName.From(Path.GetFileNameWithoutExtension(path)), path);
        }
    }

    public bool TryResolve(string componentName, [MaybeNullWhen(false)] out string source)
    {
        if (_paths.TryGetValue(componentName, out var path))
        {
            source = File.ReadAllText(path);
            return true;
        }

        source = null;
        return false;
    }
}
