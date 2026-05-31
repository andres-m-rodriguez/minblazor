using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Transformer
{
    public TransformResult Transform(Document document)
    {
        var kept = new List<Token>();
        var hostTags = new List<HostTag>();

        foreach (var token in document.Tokens)
        {
            switch (token.Kind)
            {
                case TokenKind.HostTag:
                    hostTags.Add(
                        new HostTag(token, Markers.Find(document.Text(token), Markers.HostTag))
                    );
                    break;

                default:
                    kept.Add(token);
                    break;
            }
        }

        return new TransformResult(new Document(document.Source, kept), hostTags);
    }
}
