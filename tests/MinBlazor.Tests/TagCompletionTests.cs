using MinBlazor.Cli;

namespace MinBlazor.Tests;

public class TagCompletionTests
{
    [Test]
    public async Task ReturnsPartialName_AfterOpenAngle()
    {
        var text = "<div><Ca";
        await Assert.That(TagCompletion.TagNameAt(text, text.Length)).IsEqualTo("Ca");
    }

    [Test]
    public async Task ReturnsEmpty_JustAfterAngle()
    {
        var text = "<div><";
        await Assert.That(TagCompletion.TagNameAt(text, text.Length)).IsEqualTo("");
    }

    [Test]
    public async Task ReturnsNull_InPlainText()
    {
        var text = "hello world";
        await Assert.That(TagCompletion.TagNameAt(text, text.Length)).IsNull();
    }

    [Test]
    public async Task ReturnsNull_ForClosingTag()
    {
        var text = "</Car";
        await Assert.That(TagCompletion.TagNameAt(text, text.Length)).IsNull();
    }
}
