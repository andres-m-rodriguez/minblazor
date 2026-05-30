namespace MinBlazor.Services;

public sealed class ComponentAnalyzer
{
    private const string ImportsFile = "_Imports.razor";
    private const string ScaffoldFolder = ".minblazor";

    public ComponentGraph Analyze(string entryPath, string rootDir)
    {
        var index = BuildIndex(rootDir);
        var visited = new Dictionary<string, ComponentNode>(StringComparer.Ordinal);
        var edges = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var queue = new Queue<ComponentNode>();

        var entryName = ComponentName.From(Path.GetFileNameWithoutExtension(entryPath));
        if (index.TryGetValue(entryName, out var entry))
        {
            visited[entryName] = entry;
            queue.Enqueue(entry);
        }

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var used = new List<string>();

            foreach (var name in FindReferences(File.ReadAllText(node.FilePath)))
            {
                if (name == node.Name || !index.TryGetValue(name, out var target))
                    continue;

                if (!used.Contains(target.Name))
                    used.Add(target.Name);

                if (visited.TryAdd(target.Name, target))
                    queue.Enqueue(target);
            }

            edges[node.Name] = used;
        }

        return new ComponentGraph(visited.Values.ToList(), edges);
    }

    private static Dictionary<string, ComponentNode> BuildIndex(string rootDir)
    {
        var index = new Dictionary<string, ComponentNode>(StringComparer.Ordinal);

        foreach (
            var path in Directory.EnumerateFiles(rootDir, "*.razor", SearchOption.AllDirectories)
        )
        {
            if (IsImports(path) || IsInScaffold(path))
                continue;

            var name = ComponentName.From(Path.GetFileNameWithoutExtension(path));
            if (index.ContainsKey(name))
                continue;

            index[name] = new ComponentNode(name, path, Path.GetRelativePath(rootDir, path));
        }

        return index;
    }

    private static IEnumerable<string> FindReferences(string content)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i + 1 < content.Length; i++)
        {
            if (content[i] != '<' || !char.IsAsciiLetterUpper(content[i + 1]))
                continue;

            int start = i + 1;
            int end = start;
            while (
                end < content.Length && (char.IsLetterOrDigit(content[end]) || content[end] == '_')
            )
                end++;

            names.Add(content[start..end]);
            i = end;
        }

        return names;
    }

    private static bool IsImports(string path) =>
        string.Equals(Path.GetFileName(path), ImportsFile, StringComparison.OrdinalIgnoreCase);

    private static bool IsInScaffold(string path) =>
        path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Contains(ScaffoldFolder);
}
