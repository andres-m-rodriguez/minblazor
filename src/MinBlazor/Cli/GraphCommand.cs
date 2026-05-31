using System.Text.Json;
using System.Text.Json.Serialization;
using MinBlazor.Models;
using MinBlazor.Razor.Models;

namespace MinBlazor.Cli;

public sealed class GraphCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

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
        var adjacency = Adjacency(compilation);
        var root = BuildNode(compilation.Entry.Name, adjacency, new HashSet<string>(StringComparer.Ordinal));

        if (options.Json)
        {
            _output.Info(JsonSerializer.Serialize(root, JsonOptions));
            return 0;
        }

        foreach (var diagnostic in compiled.Value!.Diagnostics)
            _output.Info($"{diagnostic.Severity}: {diagnostic.Message}");

        _output.Info(root.Name);
        PrintChildren(root, "");
        return 0;
    }

    // name -> the components it references, for every resolved component (entry + closure).
    private static IReadOnlyDictionary<string, IReadOnlyList<string>> Adjacency(Compilation compilation)
    {
        var adjacency = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [compilation.Entry.Name] = Sorted(compilation.Entry.References),
        };

        foreach (var component in compilation.Components)
            adjacency[component.Name] = Sorted(component.References);

        return adjacency;
    }

    private static IReadOnlyList<string> Sorted(IReadOnlySet<string> references) =>
        references.OrderBy(reference => reference, StringComparer.Ordinal).ToList();

    // Expands each component once (first time it is reached); later occurrences are marked
    // Repeated so shared components and cycles stay bounded. Unresolved components (e.g. from
    // a package) have no entry in the adjacency and render as leaves.
    private static GraphNode BuildNode(string name, IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency, HashSet<string> expanded)
    {
        if (!expanded.Add(name))
            return new GraphNode(name) { Repeated = true };

        if (!adjacency.TryGetValue(name, out var references) || references.Count == 0)
            return new GraphNode(name);

        return new GraphNode(name)
        {
            Children = references.Select(reference => BuildNode(reference, adjacency, expanded)).ToList(),
        };
    }

    private void PrintChildren(GraphNode node, string prefix)
    {
        var children = node.Children ?? [];
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var last = i == children.Count - 1;
            var marker = child.Repeated ? " (shown above)" : "";

            _output.Info($"{prefix}{(last ? "└─ " : "├─ ")}{child.Name}{marker}");
            PrintChildren(child, prefix + (last ? "   " : "│  "));
        }
    }

    private sealed record GraphNode(string Name)
    {
        public bool Repeated { get; init; }
        public IReadOnlyList<GraphNode>? Children { get; init; }
    }
}
