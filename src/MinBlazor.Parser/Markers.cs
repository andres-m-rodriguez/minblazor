namespace MinBlazor.Parser;

public static class Markers
{
    public const string HostTag = "hostTag";

    public static bool Matches(ReadOnlySpan<char> src, int start, ReadOnlySpan<char> marker)
    {
        if (start + marker.Length > src.Length)
            return false;
        if (!src.Slice(start, marker.Length).SequenceEqual(marker))
            return false;

        int after = start + marker.Length;
        return after >= src.Length || !(char.IsLetterOrDigit(src[after]) || src[after] == '_');
    }

    public static int Find(ReadOnlySpan<char> tag, ReadOnlySpan<char> marker)
    {
        for (int i = 1; i < tag.Length; i++)
        {
            if (tag[i] == '@' && char.IsWhiteSpace(tag[i - 1]) && Matches(tag, i + 1, marker))
                return i;
        }

        return -1;
    }
}

