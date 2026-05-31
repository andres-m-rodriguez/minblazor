using MinBlazor.Razor;

namespace MinBlazor.Razor.Tests;

public class TransformerTests
{
    [Test]
    public async Task ExtractsHostTags_AndStripsThemFromTheDocument()
    {
        var source = "<div><script @hostTag src=\"x.js\"></script><Counter /></div>";

        var document = new Parser(new Lexer(source)).Parse();
        var result = new Transformer().Transform(document);

        await Assert.That(result.HostTags.Count).IsEqualTo(1);
        await Assert
            .That(result.Document.Text(result.HostTags[0].Tag).ToString())
            .IsEqualTo("<script @hostTag src=\"x.js\"></script>");
        await Assert.That(new Emitter().Emit(result.Document)).IsEqualTo("<div><Counter /></div>");
    }

    [Test]
    public async Task ExtractsDirectives_AsPackages_AndStripsThem()
    {
        var source = "#:package MudBlazor@7.10.0\n#:package Blazored.LocalStorage\n<h1>Hi</h1>";

        var document = new Parser(new Lexer(source)).Parse();
        var result = new Transformer().Transform(document);

        await Assert.That(result.Packages.Count).IsEqualTo(2);
        await Assert.That(result.Packages[0].Name).IsEqualTo("MudBlazor");
        await Assert.That(result.Packages[0].Version).IsEqualTo("7.10.0");
        await Assert.That(result.Packages[1].Name).IsEqualTo("Blazored.LocalStorage");
        await Assert.That(result.Packages[1].Version).IsNull();
        await Assert.That(new Emitter().Emit(result.Document)).IsEqualTo("<h1>Hi</h1>");
    }
}
