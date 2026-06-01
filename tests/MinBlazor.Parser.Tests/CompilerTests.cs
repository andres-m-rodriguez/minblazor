using System.Diagnostics.CodeAnalysis;
using MinBlazor.Compiler;
using MinBlazor.Parser;

namespace MinBlazor.Parser.Tests;

public class CompilerTests
{
    private sealed class MapResolver : IComponentResolver
    {
        private readonly Dictionary<string, string> _sources;

        public MapResolver(Dictionary<string, string> sources) => _sources = sources;

        public bool TryResolve(string componentName, [MaybeNullWhen(false)] out string source) =>
            _sources.TryGetValue(componentName, out source);
    }

    [Test]
    public async Task CompilesTransitiveClosure_AndFiltersUnresolved()
    {
        var resolver = new MapResolver(new()
        {
            ["Card"] = "<div>@ChildContent</div>",
            ["Counter"] = "<CounterViewer />",
            ["CounterViewer"] = "<p>view</p>",
        });
        var diagnostics = new Diagnostics();

        var compilation = new Compiler(resolver, diagnostics)
            .Compile("Index", "<Card><Counter /><NavLink /></Card>");

        await Assert.That(compilation.Entry.Name).IsEqualTo("Index");

        var names = compilation.Components.Select(c => c.Name).ToHashSet();
        await Assert.That(names.Count).IsEqualTo(3);
        await Assert.That(names.Contains("Card")).IsTrue();
        await Assert.That(names.Contains("Counter")).IsTrue();
        await Assert.That(names.Contains("CounterViewer")).IsTrue();
        await Assert.That(names.Contains("NavLink")).IsFalse();

        await Assert.That(diagnostics.Items.Count).IsEqualTo(1);
        await Assert.That(diagnostics.Items[0].Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task ExtractsPackageDirectives_AndStripsThem()
    {
        var resolver = new MapResolver(new());
        var diagnostics = new Diagnostics();

        var source = "#:package Humanizer@2.14.1\n@page \"/\"\n<h1>Hi</h1>";
        var compilation = new Compiler(resolver, diagnostics).Compile("Index", source);

        await Assert.That(compilation.Packages.Count).IsEqualTo(1);
        await Assert.That(compilation.Packages[0].Name).IsEqualTo("Humanizer");
        await Assert.That(compilation.Packages[0].Version).IsEqualTo("2.14.1");

        await Assert
            .That(new Emitter().Emit(compilation.Entry.Document))
            .IsEqualTo("@page \"/\"\n<h1>Hi</h1>");
    }

    [Test]
    public async Task SortsPackages_ForDeterministicOutput()
    {
        var compilation = new Compiler(new MapResolver(new()), new Diagnostics())
            .Compile("Index", "#:package Zebra@1.0.0\n#:package Alpha@2.0.0\n<h1>Hi</h1>");

        var names = compilation.Packages.Select(p => p.Name).ToList();
        await Assert.That(names[0]).IsEqualTo("Alpha");
        await Assert.That(names[1]).IsEqualTo("Zebra");
    }
}
