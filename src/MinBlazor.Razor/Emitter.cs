using System.Text;
using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Emitter
{
    public string Emit(Document document)
    {
        var builder = new StringBuilder(document.Source.Length);

        foreach (var token in document.Tokens)
            builder.Append(document.Text(token));

        return builder.ToString();
    }

    public IReadOnlyList<string> EmitHostTags(Document document, IReadOnlyList<HostTag> hostTags)
    {
        var emitted = new List<string>(hostTags.Count);

        foreach (var hostTag in hostTags)
            emitted.Add(Clean(document.Text(hostTag.Tag), hostTag.Marker));

        return emitted;
    }

    private static string Clean(ReadOnlySpan<char> tag, int marker)
    {
        if (marker < 0)
            return tag.ToString();

        return string.Concat(tag[..(marker - 1)], tag[(marker + 1 + Markers.HostTag.Length)..]);
    }
}
