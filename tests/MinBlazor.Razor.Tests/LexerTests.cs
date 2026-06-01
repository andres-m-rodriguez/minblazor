using MinBlazor.Razor;
using MinBlazor.Razor.Models;

namespace MinBlazor.Razor.Tests;

public class LexerTests
{
    [Test]
    public async Task BareLessThan_IsText_NotComponent()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestFiles", "LessThan.razor");
        var source = await File.ReadAllTextAsync(path);

        var document = new RazorParser(source).Parse();

        var hasComponent = document.Nodes.Any(n =>
            n.Kind is NodeKind.ComponentOpen
                or NodeKind.ComponentClose
                or NodeKind.ComponentSelfClose);

        await Assert.That(hasComponent).IsFalse();
        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }

    [Test]
    public async Task HostTag_SpansWholeElement_AndStaysLossless()
    {
        var source = "<div><script @hostTag src=\"x.js\"></script><Counter /></div>";

        var document = new RazorParser(source).Parse();

        var host = document.Nodes.Single(n => n.Kind == NodeKind.HostTag);
        await Assert
            .That(host.Text.ToString())
            .IsEqualTo("<script @hostTag src=\"x.js\"></script>");

        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }

    [Test]
    public async Task Directive_AtLineStart_IsDirectiveToken_AndStaysLossless()
    {
        var source = "#:package MudBlazor@7.10.0\n<h1>Hi</h1>";

        var document = new RazorParser(source).Parse();

        var directive = document.Nodes.Single(n => n.Kind == NodeKind.Directive);
        await Assert.That(directive.Text.ToString()).IsEqualTo("#:package MudBlazor@7.10.0\n");
        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }

    [Test]
    public async Task Hash_NotAtLineStart_IsText()
    {
        var source = "<p>a #: b</p>";

        var document = new RazorParser(source).Parse();

        await Assert.That(document.Nodes.Any(n => n.Kind == NodeKind.Directive)).IsFalse();
        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }
}
