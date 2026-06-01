using System.Diagnostics.CodeAnalysis;
using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class ComponentRegistry : IComponentResolver
{
    private readonly Dictionary<string, IComponentSource> _sources = new(StringComparer.Ordinal);

    public Result Add(string name, IComponentSource source)
    {
        if (!_sources.TryAdd(name, source))
            return Result.Fail($"A component named '{name}' is already registered.");

        return Result.Ok();
    }

    public Result Add(string name, string razorSource) =>
        Add(name, new InlineComponentSource(razorSource));

    public bool Contains(string name) => _sources.ContainsKey(name);

    public IReadOnlyCollection<string> Names => _sources.Keys;

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
