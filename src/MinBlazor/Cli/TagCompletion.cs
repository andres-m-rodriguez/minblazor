namespace MinBlazor.Cli;

public static class TagCompletion
{
    // If the cursor sits in a component tag-open position (just after '<', within the tag
    // name), returns the partial name typed so far (possibly empty). Otherwise null.
    public static string? TagNameAt(string text, int offset)
    {
        if (offset < 0 || offset > text.Length)
            return null;

        int start = offset;
        while (start > 0 && IsNameChar(text[start - 1]))
            start--;

        if (start == 0 || text[start - 1] != '<')
            return null;

        return text[start..offset];
    }

    private static bool IsNameChar(char c) => char.IsLetterOrDigit(c) || c == '_';
}
