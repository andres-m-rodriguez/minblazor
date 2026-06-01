using MinBlazor.Compiler;
using MinBlazor.Razor;

namespace MinBlazor.Razor.Tests;

public class ComponentRegistryTests
{
    [Test]
    public async Task ResolvesAddedInlineSource()
    {
        var registry = new ComponentRegistry();
        registry.Add("Card", "<div>@ChildContent</div>");

        await Assert.That(registry.TryResolve("Card", out var source)).IsEqualTo(ResolveResult.Resolved);
        await Assert.That(source).IsEqualTo("<div>@ChildContent</div>");
    }

    [Test]
    public async Task ReturnsFalseForUnknownComponent()
    {
        var registry = new ComponentRegistry();

        await Assert.That(registry.TryResolve("Missing", out _)).IsEqualTo(ResolveResult.NotFound);
        await Assert.That(registry.Contains("Missing")).IsFalse();
    }

    [Test]
    public async Task DuplicateNameFails()
    {
        var registry = new ComponentRegistry();

        await Assert.That(registry.Add("Card", "<p>one</p>").IsSuccess).IsTrue();

        var duplicate = registry.Add("Card", "<p>two</p>");
        await Assert.That(duplicate.IsSuccess).IsFalse();
        await Assert.That(duplicate.Error).Contains("Card");
    }
}
