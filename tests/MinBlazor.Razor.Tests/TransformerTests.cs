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
}
