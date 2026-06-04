namespace MinBlazor.Parser;

public static class RazorDiff
{
    public static RazorChange Classify(Document old, Document current)
    {
        var oldStyle = old.Nodes.Where(n => n.Kind == NodeKind.StyleBlock).ToList();
        var newStyle = current.Nodes.Where(n => n.Kind == NodeKind.StyleBlock).ToList();
        var oldRest = old.Nodes.Where(n => n.Kind != NodeKind.StyleBlock).ToList();
        var newRest = current.Nodes.Where(n => n.Kind != NodeKind.StyleBlock).ToList();

        if (!NodesEqual(oldRest, newRest))
            return new RazorChange.Full();

        if (NodesEqual(oldStyle, newStyle))
            return new RazorChange.None();

        var css = string.Concat(newStyle.Select(n => ExtractCss(n.Text)));
        return new RazorChange.CssOnly(css);
    }

    private static bool NodesEqual(IReadOnlyList<RazorNode> a, IReadOnlyList<RazorNode> b)
    {
        if (a.Count != b.Count)
            return false;
        for (int i = 0; i < a.Count; i++)
            if (a[i].Kind != b[i].Kind || !a[i].Text.Span.SequenceEqual(b[i].Text.Span))
                return false;
        return true;
    }

    private static string ExtractCss(ReadOnlyMemory<char> styleBlock)
    {
        var span = styleBlock.Span;
        var openEnd = span.IndexOf('>');
        if (openEnd < 0)
            return string.Empty;
        var closeStart = span.LastIndexOf('<');
        if (closeStart <= openEnd)
            return string.Empty;
        return span.Slice(openEnd + 1, closeStart - openEnd - 1).Trim().ToString();
    }
}
