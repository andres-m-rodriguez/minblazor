using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Transformer
{
    public TransformResult Transform(Document document)
    {
        var kept = new List<Token>();
        var hostTags = new List<HostTag>();
        var packages = new List<PackageReference>();

        foreach (var token in document.Tokens)
        {
            switch (token.Kind)
            {
                case TokenKind.HostTag:
                    hostTags.Add(
                        new HostTag(token, Markers.Find(document.Text(token), Markers.HostTag))
                    );
                    break;

                case TokenKind.Directive:
                    if (Scanner.TryParsePackage(document.Text(token), out var package))
                        packages.Add(package);
                    break;

                default:
                    kept.Add(token);
                    break;
            }
        }

        return new TransformResult(new Document(document.Source, kept), hostTags, packages);
    }
}
