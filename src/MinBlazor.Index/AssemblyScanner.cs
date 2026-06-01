using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MinBlazor.Index;

public sealed class AssemblyScanner
{
    private const string IComponentName = "Microsoft.AspNetCore.Components.IComponent";

    private readonly List<(string Name, MetadataReference Reference)> _assemblies = [];
    private CSharpCompilation? _compilation;
    private INamedTypeSymbol? _iComponent;

    public void Load(IAssemblyProvider provider)
    {
        _assemblies.AddRange(provider.GetAssemblies());
        _compilation = null;
        _iComponent = null;
    }

    public IReadOnlyList<IndexedComponent> Scan(IReadOnlyCollection<string> assemblyNames)
    {
        if (_assemblies.Count == 0 || assemblyNames.Count == 0)
            return [];

        _compilation ??= CSharpCompilation.Create(
            "minblazor-index",
            references: _assemblies.Select(a => a.Reference));

        _iComponent ??= _compilation.GetTypeByMetadataName(IComponentName);

        if (_iComponent is null)
            return [];

        var wanted = new HashSet<string>(assemblyNames, StringComparer.OrdinalIgnoreCase);
        var results = new List<IndexedComponent>();

        foreach (var (name, reference) in _assemblies)
        {
            if (!wanted.Contains(name))
                continue;

            if (_compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
                continue;

            foreach (var type in Types(assembly.GlobalNamespace))
            {
                if (type.DeclaredAccessibility != Accessibility.Public
                    || type.TypeKind != TypeKind.Class
                    || type.IsAbstract
                    || !type.AllInterfaces.Contains(_iComponent, SymbolEqualityComparer.Default))
                    continue;

                var ns = type.ContainingNamespace.ToDisplayString();
                results.Add(new IndexedComponent(
                    type.Name,
                    ComponentKind.Package,
                    string.IsNullOrEmpty(ns) ? null : ns));
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
