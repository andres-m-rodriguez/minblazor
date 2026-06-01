using MinBlazor.Compiler;
using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class RazorCompiler(IComponentResolver resolver, Diagnostics diagnostics)
{
    private readonly IComponentResolver _resolver = resolver;
    private readonly Diagnostics _diagnostics = diagnostics;
    private readonly Analyzer _analyzer = new();
    private readonly Transformer _transformer = new();

    public Compilation Compile(string entryName, string entrySource)
    {
        var entry = CompileUnit(entryName, entrySource);

        var components = new List<CompiledComponent>();
        var packages = new Dictionary<string, PackageReference>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string> { entryName };
        var queue = new Queue<string>();

        foreach (var package in entry.Packages)
            packages[package.Name] = package;

        foreach (var reference in entry.References)
            if (visited.Add(reference))
                queue.Enqueue(reference);

        while (queue.Count > 0)
        {
            var name = queue.Dequeue();

            var resolved = _resolver.TryResolve(name, out var source);
            if (resolved == ResolveResult.NotFound)
            {
                _diagnostics.Warning($"Could not resolve component '{name}'.");
                continue;
            }
            if (resolved == ResolveResult.External)
                continue;

            var unit = CompileUnit(name, source);
            components.Add(unit);

            foreach (var package in unit.Packages)
                packages[package.Name] = package;

            foreach (var reference in unit.References)
                if (visited.Add(reference))
                    queue.Enqueue(reference);
        }

        return new Compilation(
            entry,
            components,
            packages.Values.OrderBy(p => p.Name, StringComparer.Ordinal).ToList()
        );
    }

    private CompiledComponent CompileUnit(string name, string source)
    {
        var document = new RazorParser(source).Parse();
        var transformed = _transformer.Transform(document);
        var references = _analyzer.Analyze(transformed.Document).Components;

        return new CompiledComponent(
            name,
            transformed.Document,
            transformed.HostTags,
            references,
            transformed.Packages
        );
    }
}




