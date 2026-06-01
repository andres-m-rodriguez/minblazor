using System.Diagnostics.CodeAnalysis;
using MinBlazor.Compiler;
using MinBlazor.Index;

namespace MinBlazor.Cli;

public sealed class IndexedComponentResolver(
    ComponentTable table,
    IReadOnlyDictionary<string, string> sourcePaths,
    IReadOnlyDictionary<string, string> virtuals) : IComponentResolver
{
    public bool TryResolve(string name, [MaybeNullWhen(false)] out string source)
    {
        source = null;

        var component = table.TryGet(name);
        if (component is null)
            return false;

        switch (component.Kind)
        {
            case ComponentKind.Source:
                if (!sourcePaths.TryGetValue(name, out var path))
                    return false;
                source = File.ReadAllText(path);
                return true;

            case ComponentKind.Virtual:
                return virtuals.TryGetValue(name, out source);

            default:
                return false;
        }
    }
}
