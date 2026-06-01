using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Scanner
{
    private const string Package = "package";

    public IReadOnlyList<PackageReference> Packages(Document document)
    {
        var packages = new Dictionary<string, PackageReference>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in document.Tokens)
            if (token.Kind == TokenKind.Directive)
                if (TryParsePackage(document.Text(token), out var package))
                    packages[package.Name] = package;

        return packages.Values.ToList();
    }

    internal static bool TryParsePackage(ReadOnlySpan<char> directive, out PackageReference package)
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
