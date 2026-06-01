namespace MinBlazor.Compiler;

public sealed record ComponentNode(string Name, IReadOnlyList<ComponentNode> Children);

public sealed record ComponentGraph(
    IReadOnlyList<ComponentNode> Roots,
    IReadOnlySet<string> Components
);
