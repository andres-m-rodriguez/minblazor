namespace MinBlazor.Models;

public sealed record Component(string Name, int IdxStart, IReadOnlyList<Component> Components);
