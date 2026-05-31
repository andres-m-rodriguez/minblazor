using System.Text;
using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

internal static class Directives
{
    private const string Package = "package";

    public static (string Source, IReadOnlyList<PackageReference> Packages) Extract(string source)
    {
        var packages = new List<PackageReference>();
        var kept = new StringBuilder(source.Length);
        int i = 0;

        while (i < source.Length)
        {
            int lineEnd = LineEnd(source, i);

            if (StartsDirective(source, i))
            {
                if (TryParsePackage(source.AsSpan(i, lineEnd - i), out var package))
                    packages.Add(package);
            }
            else
            {
                kept.Append(source, i, lineEnd - i);
            }

            i = lineEnd;
        }

        return (kept.ToString(), packages);
    }

    private static bool StartsDirective(string source, int i) =>
        (i == 0 || source[i - 1] == '\n')
        && i + 1 < source.Length
        && source[i] == '#'
        && source[i + 1] == ':';

    private static int LineEnd(string source, int i)
    {
        int newline = source.IndexOf('\n', i);
        return newline < 0 ? source.Length : newline + 1;
    }

    private static bool TryParsePackage(ReadOnlySpan<char> line, out PackageReference package)
    {
        package = null!;

        var body = line[2..].Trim();
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
