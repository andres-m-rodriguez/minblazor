using MinBlazor.Models;

namespace MinBlazor.Services;

public sealed class ComponentParser
{
    public IReadOnlyList<Component> Parse(string content) => ParseLevel(content, 0);

    private static IReadOnlyList<Component> ParseLevel(string content, int baseOffset)
    {
        var components = new List<Component>();
        int i = 0;

        while (i < content.Length)
        {
            if (!StartsComponent(content, i))
            {
                i++;
                continue;
            }

            var tag = ReadOpenTag(content, i);
            var idxStart = baseOffset + i;

            if (tag.SelfClosing)
            {
                components.Add(new Component(tag.Name, idxStart, []));
                i = tag.AfterTag;
                continue;
            }

            int closeStart = FindMatchingClose(content, tag.Name, tag.AfterTag);
            int innerEnd = closeStart < 0 ? content.Length : closeStart;

            var inner = content[tag.AfterTag..innerEnd];
            var children = ParseLevel(inner, baseOffset + tag.AfterTag);
            components.Add(new Component(tag.Name, idxStart, children));

            i = closeStart < 0 ? content.Length : SkipToTagEnd(content, closeStart);
        }

        return components;
    }

    private static bool StartsComponent(string content, int index) =>
        content[index] == '<'
        && index + 1 < content.Length
        && char.IsAsciiLetterUpper(content[index + 1]);

    private static OpenTag ReadOpenTag(string content, int ltIndex)
    {
        int i = ltIndex + 1;
        int nameStart = i;
        while (i < content.Length && (char.IsLetterOrDigit(content[i]) || content[i] == '_'))
            i++;
        var name = content[nameStart..i];

        bool inString = false;
        char quote = '\0';
        char lastNonWhitespace = '\0';

        while (i < content.Length)
        {
            char c = content[i];

            if (inString)
            {
                if (c == quote)
                    inString = false;
                i++;
                continue;
            }

            if (c is '"' or '\'')
            {
                inString = true;
                quote = c;
                i++;
                continue;
            }

            if (c == '>')
                return new OpenTag(name, lastNonWhitespace == '/', i + 1);

            if (!char.IsWhiteSpace(c))
                lastNonWhitespace = c;
            i++;
        }

        return new OpenTag(name, false, content.Length);
    }

    private static int FindMatchingClose(string content, string name, int from)
    {
        int depth = 1;
        int i = from;

        while (i < content.Length)
        {
            if (content[i] != '<')
            {
                i++;
                continue;
            }

            if (IsClosingTag(content, i, name))
            {
                depth--;
                if (depth == 0)
                    return i;
                i = SkipToTagEnd(content, i);
                continue;
            }

            if (IsOpeningTag(content, i, name))
            {
                var nested = ReadOpenTag(content, i);
                if (!nested.SelfClosing)
                    depth++;
                i = nested.AfterTag;
                continue;
            }

            i++;
        }

        return -1;
    }

    private static bool IsOpeningTag(string content, int ltIndex, string name) =>
        MatchesName(content, ltIndex + 1, name);

    private static bool IsClosingTag(string content, int ltIndex, string name) =>
        ltIndex + 1 < content.Length
        && content[ltIndex + 1] == '/'
        && MatchesName(content, ltIndex + 2, name);

    private static bool MatchesName(string content, int start, string name)
    {
        if (start + name.Length > content.Length)
            return false;
        if (string.CompareOrdinal(content, start, name, 0, name.Length) != 0)
            return false;

        int after = start + name.Length;
        return after >= content.Length || IsBoundary(content[after]);
    }

    private static bool IsBoundary(char c) => char.IsWhiteSpace(c) || c is '>' or '/';

    private static int SkipToTagEnd(string content, int ltIndex)
    {
        bool inString = false;
        char quote = '\0';

        for (int i = ltIndex; i < content.Length; i++)
        {
            char c = content[i];

            if (inString)
            {
                if (c == quote)
                    inString = false;
                continue;
            }

            if (c is '"' or '\'')
            {
                inString = true;
                quote = c;
                continue;
            }

            if (c == '>')
                return i + 1;
        }

        return content.Length;
    }

    private readonly record struct OpenTag(string Name, bool SelfClosing, int AfterTag);
}
