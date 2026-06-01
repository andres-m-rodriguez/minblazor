using MinBlazor.Core;
using MinBlazor.Compiler;
using MinBlazor.Parser;

namespace MinBlazor.Parser.Tests;

public class EmitterTests
{
    [Test]
    public async Task EmitsHostTags_WithMarkerStripped()
    {
        var source = "<div><script @hostTag src=\"x.js\"></script></div>";

        var document = new RazorParser(source).Parse();
        var result = new Transformer().Transform(document);

        var hostTags = new Emitter().EmitHostTags(result.Document, result.HostTags);

        await Assert.That(hostTags.Count).IsEqualTo(1);
        await Assert.That(hostTags[0]).IsEqualTo("<script src=\"x.js\"></script>");
    }
}



