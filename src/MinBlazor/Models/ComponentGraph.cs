namespace MinBlazor.Models;

public sealed record ComponentNode(string Name, string FilePath, string RelativePath);

public sealed record ComponentGraph(
    IReadOnlyList<ComponentNode> Nodes,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Edges
);
