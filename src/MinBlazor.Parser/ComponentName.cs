namespace MinBlazor.Parser;

public static class ComponentNameParser
{
    public static string Of(RazorNode node)
    {
        var span = node.Text.Span;

        int i = 1;
        if (i < span.Length && span[i] == '/')
            i++;

        int start = i;
        while (i < span.Length && (char.IsLetterOrDigit(span[i]) || span[i] is '_' or '.'))
            i++;

        return span[start..i].ToString();
    }

    public static string From(string fileNameWithoutExtension)
    {
        var sb = new System.Text.StringBuilder(fileNameWithoutExtension.Length);
        foreach (var c in fileNameWithoutExtension)
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');

        if (sb.Length == 0 || char.IsDigit(sb[0]))
            sb.Insert(0, '_');

        return sb.ToString();
    }
}


