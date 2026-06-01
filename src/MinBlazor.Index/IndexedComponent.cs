namespace MinBlazor.Index;

public enum ComponentKind
{
    Source,
    Virtual,
    Package,
}

public sealed record IndexedComponent(
    string Name,
    ComponentKind Kind,
    string? Namespace);
