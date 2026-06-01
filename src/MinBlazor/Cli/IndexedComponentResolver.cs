using System.Diagnostics.CodeAnalysis;
using MinBlazor.Compiler;
using MinBlazor.Index;

namespace MinBlazor.Cli;

public sealed class IndexedComponentResolver(
    ComponentTable table,
    IReadOnlyDictionary<string, string> sourcePaths,
    IReadOnlyDictionary<string, string> virtuals) : IComponentResolver
{
    public ResolveResult TryResolve(string name, [NotNullWhen(true)] out string? source)
    {
        source = null;

        var component = table.TryGet(name);
        if (component is null)
            return ResolveResult.NotFound;

        switch (component.Kind)
        {
            case ComponentKind.Source:
                if (!sourcePaths.TryGetValue(name, out var path))
                    return ResolveResult.NotFound;
                source = File.ReadAllText(path);
                return ResolveResult.Resolved;

            case ComponentKind.Virtual:
                if (!virtuals.TryGetValue(name, out source))
                    return ResolveResult.NotFound;
                return ResolveResult.Resolved;

            default:
                return ResolveResult.External;
        }
    }
}
