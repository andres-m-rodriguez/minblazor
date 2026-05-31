using System.Diagnostics.CodeAnalysis;

namespace MinBlazor.Razor;

public sealed class ComponentRegistry : IComponentResolver
{
    private readonly Dictionary<string, IComponentSource> _sources = new(StringComparer.Ordinal);

    public void Add(string name, IComponentSource source)
    {
        if (!_sources.TryAdd(name, source))
            throw new DuplicateComponentException(name);
    }

    public void Add(string name, string razorSource) =>
        Add(name, new InlineComponentSource(razorSource));

    public bool Contains(string name) => _sources.ContainsKey(name);

    public bool TryResolve(string name, [MaybeNullWhen(false)] out string source)
    {
        if (_sources.TryGetValue(name, out var entry))
        {
            source = entry.Read();
            return true;
        }

        source = null;
        return false;
    }
}
