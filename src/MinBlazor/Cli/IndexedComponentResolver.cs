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

        return component.Kind switch
        {
            ComponentKind.Package => ResolveResult.External,

            ComponentKind.Source when sourcePaths.TryGetValue(name, out var path)
                => (source = File.ReadAllText(path)) is not null
                    ? ResolveResult.Resolved
                    : ResolveResult.NotFound,

            ComponentKind.Virtual when virtuals.TryGetValue(name, out source)
                => ResolveResult.Resolved,

            _ => ResolveResult.NotFound,
        };
    }
}
