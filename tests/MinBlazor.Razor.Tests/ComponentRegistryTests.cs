using MinBlazor.Razor;

namespace MinBlazor.Razor.Tests;

public class ComponentRegistryTests
{
    [Test]
    public async Task ResolvesAddedInlineSource()
    {
        var registry = new ComponentRegistry();
        registry.Add("Card", "<div>@ChildContent</div>");

        await Assert.That(registry.TryResolve("Card", out var source)).IsTrue();
        await Assert.That(source).IsEqualTo("<div>@ChildContent</div>");
    }

    [Test]
    public async Task ReturnsFalseForUnknownComponent()
    {
        var registry = new ComponentRegistry();

        await Assert.That(registry.TryResolve("Missing", out _)).IsFalse();
        await Assert.That(registry.Contains("Missing")).IsFalse();
    }

    [Test]
    public async Task DuplicateNameThrows()
    {
        var registry = new ComponentRegistry();
        registry.Add("Card", "<p>one</p>");

        await Assert
            .That(() => registry.Add("Card", "<p>two</p>"))
            .Throws<DuplicateComponentException>();
    }
}
