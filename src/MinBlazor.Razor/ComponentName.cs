using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

internal static class ComponentName
{
    internal static string Of(Document document, Token token)
    {
        var span = document.Text(token);

        int i = 1;
        if (i < span.Length && span[i] == '/')
            i++;

        int start = i;
        while (i < span.Length && (char.IsLetterOrDigit(span[i]) || span[i] is '_' or '.'))
            i++;

        return span[start..i].ToString();
    }
}
