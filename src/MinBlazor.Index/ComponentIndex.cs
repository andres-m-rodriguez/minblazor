namespace MinBlazor.Index;

public sealed class ComponentIndex
{
    private readonly List<IndexedComponent> _components = [];

    public IReadOnlyList<IndexedComponent> All => _components;

    public IReadOnlyList<string> Names =>
        _components.Select(c => c.Name).Distinct(StringComparer.Ordinal).Order().ToList();

    public IReadOnlyList<string> Namespaces =>
        _components
            .Select(c => c.Namespace)
            .Where(ns => ns is not null)
            .Distinct(StringComparer.Ordinal)
            .Order()
            .ToList()!;

    public void AddSource(ISourceComponentProvider provider)
    {
        foreach (var (name, path) in provider.GetComponents())
            _components.Add(
                new IndexedComponent(name, ComponentKind.Source, Namespace: null, path)
            );
    }

    public void AddVirtual(string name, string razorSource)
    {
        _components.Add(
            new IndexedComponent(name, ComponentKind.Virtual, Namespace: null, SourcePath: null)
        );
    }

    public void AddPackage(AssemblyScanner scanner, IReadOnlyCollection<string> assemblyNames)
    {
        foreach (var component in scanner.Scan(assemblyNames))
            _components.Add(component);
    }
}
