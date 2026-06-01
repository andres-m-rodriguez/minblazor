using MinBlazor.Razor;

namespace MinBlazor.Razor.Tests;

public class AnalyzerTests
{
    [Test]
    public async Task BuildsNestedComponentGraph()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestFiles", "Simple.razor");
        var source = await File.ReadAllTextAsync(path);

        var document = new RazorParser(source).Parse();
        var graph = new Analyzer().Analyze(document);

        await Assert.That(graph.Roots.Count).IsEqualTo(1);

        var card = graph.Roots[0];
        await Assert.That(card.Name).IsEqualTo("Card");
        await Assert.That(card.Children.Count).IsEqualTo(2);
        await Assert.That(card.Children[0].Name).IsEqualTo("Counter");
        await Assert.That(card.Children[1].Name).IsEqualTo("Counter");

        await Assert.That(graph.Components.Count).IsEqualTo(2);
        await Assert.That(graph.Components.Contains("Card")).IsTrue();
        await Assert.That(graph.Components.Contains("Counter")).IsTrue();
    }
}


