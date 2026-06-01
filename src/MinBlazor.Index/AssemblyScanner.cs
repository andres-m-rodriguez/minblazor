using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MinBlazor.Index;

public sealed class AssemblyScanner(IAssemblyProvider provider)
{
    private const string IComponentName = "Microsoft.AspNetCore.Components.IComponent";

    private readonly IAssemblyProvider _provider = provider;

    public IReadOnlyList<IndexedComponent> Scan(IReadOnlyCollection<string> assemblyNames)
    {
        if (assemblyNames.Count == 0)
            return [];

        var assemblies = _provider.GetAssemblies().ToList();
        if (assemblies.Count == 0)
            return [];

        var compilation = CSharpCompilation.Create(
            "minblazor-index",
            references: assemblies.Select(a => a.Reference)
        );

        var iComponent = compilation.GetTypeByMetadataName(IComponentName);
        if (iComponent is null)
            return [];

        var wanted = new HashSet<string>(assemblyNames, StringComparer.OrdinalIgnoreCase);
        var results = new List<IndexedComponent>();

        foreach (var (name, reference) in assemblies)
        {
            if (!wanted.Contains(name))
                continue;

            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
                continue;

            foreach (var type in Types(assembly.GlobalNamespace))
            {
                if (
                    type.DeclaredAccessibility != Accessibility.Public
                    || type.TypeKind != TypeKind.Class
                    || type.IsAbstract
                    || !type.AllInterfaces.Contains(iComponent, SymbolEqualityComparer.Default)
                )
                    continue;

                var ns = type.ContainingNamespace.ToDisplayString();
                results.Add(
                    new IndexedComponent(
                        type.Name,
                        ComponentKind.Package,
                        string.IsNullOrEmpty(ns) ? null : ns
                    )
                );
            }
        }

        results.Sort((a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
        return results;
    }

    private static IEnumerable<INamedTypeSymbol> Types(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
            yield return type;

        foreach (var child in ns.GetNamespaceMembers())
        foreach (var type in Types(child))
            yield return type;
    }
}
