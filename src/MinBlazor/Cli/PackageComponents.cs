using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MinBlazor.Cli;

// Finds Blazor components (public, non-abstract IComponent types) inside the assemblies of
// the given packages. The scaffold's build output holds every referenced assembly as a real
// PE file, so Roslyn can resolve IComponent and the component base chain from that one folder.
public static class PackageComponents
{
    private const string IComponentName = "Microsoft.AspNetCore.Components.IComponent";

    private static readonly ConcurrentDictionary<string, (DateTime Stamp, IReadOnlyList<string> Components)> Cache = new();

    public static IReadOnlyList<string> Scan(string binDir, IReadOnlyCollection<string> packageNames)
    {
        if (packageNames.Count == 0 || !Directory.Exists(binDir))
            return [];

        var stamp = Directory.GetLastWriteTimeUtc(binDir);
        var key = binDir + "|" + string.Join(",", packageNames.OrderBy(name => name, StringComparer.Ordinal));

        if (Cache.TryGetValue(key, out var cached) && cached.Stamp == stamp)
            return cached.Components;

        var components = ScanCore(binDir, packageNames);
        Cache[key] = (stamp, components);
        return components;
    }

    private static IReadOnlyList<string> ScanCore(string binDir, IReadOnlyCollection<string> packageNames)
    {
        var references = Directory
            .EnumerateFiles(binDir, "*.dll")
            .Select(dll => (MetadataReference)MetadataReference.CreateFromFile(dll))
            .ToList();

        var compilation = CSharpCompilation.Create("minblazor-scan", references: references);

        var component = compilation.GetTypeByMetadataName(IComponentName);
        if (component is null)
            return [];

        var wanted = new HashSet<string>(packageNames, StringComparer.OrdinalIgnoreCase);
        var names = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var reference in references)
        {
            if (reference is not PortableExecutableReference { FilePath: { } path })
                continue;

            if (!wanted.Contains(Path.GetFileNameWithoutExtension(path)))
                continue;

            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
                continue;

            foreach (var type in Types(assembly.GlobalNamespace))
                if (type.DeclaredAccessibility == Accessibility.Public
                    && type.TypeKind == TypeKind.Class
                    && !type.IsAbstract
                    && type.AllInterfaces.Contains(component, SymbolEqualityComparer.Default))
                    names.Add(type.Name);
        }

        return names.ToList();
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
