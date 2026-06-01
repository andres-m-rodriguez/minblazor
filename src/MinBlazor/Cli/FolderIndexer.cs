using MinBlazor.Compiler;
using MinBlazor.Services;
using Result = MinBlazor.Parser.Result;

namespace MinBlazor.Cli;

public static class FolderIndexer
{
    public static Result Index(string rootDir, ComponentRegistry registry)
    {
        foreach (
            var path in Directory.EnumerateFiles(rootDir, "*.razor", SearchOption.AllDirectories)
        )
        {
            if (IsScaffold(path))
                continue;

            var added = registry.Add(
                ComponentName.From(Path.GetFileNameWithoutExtension(path)),
                new FileComponentSource(path)
            );
            if (!added.IsSuccess)
                return added;
        }

        return Result.Ok();
    }

    private static bool IsScaffold(string path) =>
        path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Contains(".minblazor");
}
