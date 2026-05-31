using MinBlazor.Razor;

namespace MinBlazor.Razor.Tests;

public class RoundTripTests
{
    [Test]
    public async Task RoundTrips_SimpleComponent()
    {
        await AssertRoundTrips("Simple.razor");
    }

    [Test]
    public async Task RoundTrips_ComponentWithCode()
    {
        await AssertRoundTrips("WithCode.razor");
    }

    private static async Task AssertRoundTrips(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestFiles", fileName);
        var source = await File.ReadAllTextAsync(path);

        var document = new Parser(new Lexer(source)).Parse();
        var regenerated = new Emitter().Emit(document);

        await Assert.That(regenerated).IsEqualTo(source);
    }
}
