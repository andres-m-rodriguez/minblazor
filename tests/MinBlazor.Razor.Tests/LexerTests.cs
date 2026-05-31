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

        var document = new Parser(new Lexer(source)).Parse();

        var hasComponent = document.Tokens.Any(token =>
            token.Kind is TokenKind.ComponentOpen
                or TokenKind.ComponentClose
                or TokenKind.ComponentSelfClose);

        await Assert.That(hasComponent).IsFalse();
        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }

    [Test]
    public async Task HostTag_SpansWholeElement_AndStaysLossless()
    {
        var source = "<div><script @hostTag src=\"x.js\"></script><Counter /></div>";

        var document = new Parser(new Lexer(source)).Parse();

        var host = document.Tokens.Single(token => token.Kind == TokenKind.HostTag);
        await Assert
            .That(document.Text(host).ToString())
            .IsEqualTo("<script @hostTag src=\"x.js\"></script>");

        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }

    [Test]
    public async Task Directive_AtLineStart_IsDirectiveToken_AndStaysLossless()
    {
        var source = "#:package MudBlazor@7.10.0\n<h1>Hi</h1>";

        var document = new Parser(new Lexer(source)).Parse();

        var directive = document.Tokens.Single(token => token.Kind == TokenKind.Directive);
        await Assert.That(document.Text(directive).ToString()).IsEqualTo("#:package MudBlazor@7.10.0\n");
        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }

    [Test]
    public async Task Hash_NotAtLineStart_IsText()
    {
        var source = "<p>a #: b</p>";

        var document = new Parser(new Lexer(source)).Parse();

        await Assert.That(document.Tokens.Any(token => token.Kind == TokenKind.Directive)).IsFalse();
        await Assert.That(new Emitter().Emit(document)).IsEqualTo(source);
    }
}
