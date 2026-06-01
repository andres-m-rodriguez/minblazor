using System.Text;

namespace MinBlazor.Parser;

public sealed class Emitter
{
    public string Emit(Document document)
    {
        var builder = new StringBuilder(document.Source.Length);

        foreach (var node in document.Nodes)
            builder.Append(node.Text.Span);

        return builder.ToString();
    }

    public IReadOnlyList<string> EmitHostTags(IReadOnlyList<RazorNode> hostTags)
    {
        var emitted = new List<string>(hostTags.Count);

        foreach (var hostTag in hostTags)
            emitted.Add(Clean(hostTag.Text.Span));

        return emitted;
    }

    private static string Clean(ReadOnlySpan<char> tag)
    {
        var marker = Markers.Find(tag, Markers.HostTag);
        if (marker < 0)
            return tag.ToString();

        return string.Concat(tag[..(marker - 1)], tag[(marker + 1 + Markers.HostTag.Length)..]);
    }
}
