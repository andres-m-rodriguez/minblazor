using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Transformer
{
    private const string Package = "package";

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
                    if (TryParsePackage(document.Text(token), out var package))
                        packages.Add(package);
                    break;

                default:
                    kept.Add(token);
                    break;
            }
        }

        return new TransformResult(new Document(document.Source, kept), hostTags, packages);
    }

    private static bool TryParsePackage(ReadOnlySpan<char> directive, out PackageReference package)
    {
        package = null!;

        var body = directive[2..].Trim();
        if (
            !body.StartsWith(Package)
            || body.Length == Package.Length
            || !char.IsWhiteSpace(body[Package.Length])
        )
            return false;

        var arg = body[Package.Length..].Trim();
        if (arg.IsEmpty)
            return false;

        int at = arg.IndexOf('@');
        package =
            at < 0
                ? new PackageReference(arg.ToString(), null)
                : new PackageReference(arg[..at].ToString(), arg[(at + 1)..].ToString());

        return true;
    }
}
