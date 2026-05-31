using System.Text.Json;
using MinBlazor.Models;
using MinBlazor.Razor.Models;

namespace MinBlazor.Cli;

public sealed class GraphCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IOutput _output;

    public GraphCommand(IOutput output) => _output = output;

    public int Execute(GraphOptions options)
    {
        var compiled = new Pipeline(_output).Compile(options.RazorFile, Pipeline.CacheDirectory(options.RazorFile));
        if (!compiled.IsSuccess)
        {
            _output.Error(compiled.Error!);
            return 1;
        }

        var compilation = compiled.Value!.Compilation;
        var graph = BuildGraph(compilation);

        if (options.Json)
        {
            _output.Info(JsonSerializer.Serialize(new { entry = compilation.Entry.Name, components = graph }, JsonOptions));
            return 0;
        }

        foreach (var diagnostic in compiled.Value!.Diagnostics)
            _output.Info($"{diagnostic.Severity}: {diagnostic.Message}");

        PrintTree(compilation.Entry.Name, graph);
        return 0;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildGraph(Compilation compilation)
    {
        var graph = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [compilation.Entry.Name] = Sorted(compilation.Entry.References),
        };

        foreach (var component in compilation.Components)
            graph[component.Name] = Sorted(component.References);

        return graph;
    }

    private static IReadOnlyList<string> Sorted(IReadOnlySet<string> references) =>
        references.OrderBy(reference => reference, StringComparer.Ordinal).ToList();

    private void PrintTree(string entry, IReadOnlyDictionary<string, IReadOnlyList<string>> graph)
    {
        _output.Info(entry);

        var expanded = new HashSet<string>(StringComparer.Ordinal) { entry };
        var children = graph.TryGetValue(entry, out var references) ? references : [];

        for (int i = 0; i < children.Count; i++)
            PrintNode(children[i], graph, expanded, "", i == children.Count - 1);
    }

    private void PrintNode(string name, IReadOnlyDictionary<string, IReadOnlyList<string>> graph, HashSet<string> expanded, string prefix, bool isLast)
    {
        var connector = isLast ? "└─ " : "├─ ";
        var alreadyExpanded = !expanded.Add(name);
        var hasChildren = graph.TryGetValue(name, out var children) && children.Count > 0;

        _output.Info($"{prefix}{connector}{name}{(alreadyExpanded && hasChildren ? " (shown above)" : "")}");

        if (alreadyExpanded || !hasChildren)
            return;

        var childPrefix = prefix + (isLast ? "   " : "│  ");
        for (int i = 0; i < children!.Count; i++)
            PrintNode(children[i], graph, expanded, childPrefix, i == children.Count - 1);
    }
}
