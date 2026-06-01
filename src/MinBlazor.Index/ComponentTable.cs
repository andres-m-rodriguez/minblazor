using MinBlazor.Razor.Models;

namespace MinBlazor.Index;

public sealed class ComponentTable
{
    private readonly Dictionary<string, IndexedComponent> _byName = new(StringComparer.Ordinal);
    private readonly List<string> _sortedNames = [];

    public int Count => _byName.Count;

    public Result Add(IndexedComponent component)
    {
        if (!_byName.TryAdd(component.Name, component))
            return Result.Fail($"A component named '{component.Name}' is already indexed.");

        var index = _sortedNames.BinarySearch(component.Name, StringComparer.Ordinal);
        _sortedNames.Insert(index < 0 ? ~index : index, component.Name);

        return Result.Ok();
    }

    public IndexedComponent? TryGet(string name) =>
        _byName.TryGetValue(name, out var component) ? component : null;

    public bool Contains(string name) => _byName.ContainsKey(name);

    public IReadOnlyList<IndexedComponent> All => _byName.Values.ToList();

    public IReadOnlyList<IndexedComponent> OfKind(ComponentKind kind) =>
        _byName.Values.Where(c => c.Kind == kind).ToList();

    // O(log n + k) prefix scan over the sorted name list.
    public IReadOnlyList<string> WithPrefix(string prefix)
    {
        if (prefix.Length == 0)
            return _sortedNames;

        var start = _sortedNames.BinarySearch(prefix, StringComparer.Ordinal);
        if (start < 0)
            start = ~start;

        var results = new List<string>();
        for (var i = start; i < _sortedNames.Count; i++)
        {
            if (!_sortedNames[i].StartsWith(prefix, StringComparison.Ordinal))
                break;
            results.Add(_sortedNames[i]);
        }

        return results;
    }

    public IReadOnlyList<string> Namespaces =>
        _byName.Values
            .Select(c => c.Namespace)
            .Where(ns => ns is not null)
            .Distinct(StringComparer.Ordinal)
            .Order()
            .ToList()!;
}
