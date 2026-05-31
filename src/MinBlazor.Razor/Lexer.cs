using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Lexer
{
    private readonly ReadOnlyMemory<char> _src;
    private int _cursor;
    private Token? _current;

    public Lexer(string source)
    {
        _src = source.AsMemory();
        _current = Scan();
    }

    public ReadOnlyMemory<char> Source => _src;

    public Token? Peek() => _current;

    public Token? Next()
    {
        var token = _current;
        _current = Scan();
        return token;
    }

    private Token? Scan()
    {
        var src = _src.Span;

        if (_cursor >= src.Length)
            return null;

        if (TryParseHostTag(src, _cursor, out var hostToken))
        {
            _cursor = hostToken.End;
            return hostToken;
        }

        if (TryParseComponent(src, _cursor, out var componentToken))
        {
            _cursor = componentToken.End;
            return componentToken;
        }

        var text = ParseText(src, _cursor);
        _cursor = text.End;
        return text;
    }

    private static bool TryParseHostTag(ReadOnlySpan<char> src, int pos, out Token hostToken)
    {
        hostToken = default;

        if (src[pos] != '<' || ReadTag(src, pos) is not { } tag || !tag.HasHostTag)
            return false;

        hostToken = new Token(TokenKind.HostTag, pos, ElementEnd(src, tag));
        return true;
    }

    private static bool TryParseComponent(ReadOnlySpan<char> src, int pos, out Token componentToken)
    {
        componentToken = default;

        if (src[pos] != '<' || ReadTag(src, pos) is not { } tag || !IsComponent(src, tag))
            return false;

        var kind =
            tag.IsClose ? TokenKind.ComponentClose
            : tag.SelfClosing ? TokenKind.ComponentSelfClose
            : TokenKind.ComponentOpen;

        componentToken = new Token(kind, pos, tag.End);
        return true;
    }

    private static Token ParseText(ReadOnlySpan<char> src, int pos)
    {
        int i = pos;

        while (
            i < src.Length && !TryParseHostTag(src, i, out _) && !TryParseComponent(src, i, out _)
        )
            i++;

        return new Token(TokenKind.Text, pos, i);
    }

    private static bool IsComponent(ReadOnlySpan<char> src, TagInfo tag) =>
        char.IsAsciiLetterUpper(src[tag.NameStart]);

    private static int ElementEnd(ReadOnlySpan<char> src, TagInfo tag)
    {
        if (tag.SelfClosing)
            return tag.End;

        int close = FindClose(src, tag, tag.End);
        return close < 0 ? tag.End : close;
    }

    private static int FindClose(ReadOnlySpan<char> src, TagInfo open, int from)
    {
        var name = src.Slice(open.NameStart, open.NameEnd - open.NameStart);
        int depth = 1;
        int i = from;

        while (i < src.Length)
        {
            if (src[i] != '<' || ReadTag(src, i) is not { } tag)
            {
                i++;
                continue;
            }

            if (SameName(src, tag, name))
            {
                if (tag.IsClose)
                {
                    depth--;
                    if (depth == 0)
                        return tag.End;
                }
                else if (!tag.SelfClosing)
                {
                    depth++;
                }
            }

            i = tag.End;
        }

        return -1;
    }

    private static bool SameName(ReadOnlySpan<char> src, TagInfo tag, ReadOnlySpan<char> name) =>
        src.Slice(tag.NameStart, tag.NameEnd - tag.NameStart).SequenceEqual(name);

    private static TagInfo? ReadTag(ReadOnlySpan<char> src, int ltIndex)
    {
        int i = ltIndex + 1;
        if (i >= src.Length)
            return null;

        bool isClose = src[i] == '/';
        if (isClose)
            i++;

        int nameStart = i;
        while (i < src.Length && (char.IsLetterOrDigit(src[i]) || src[i] is '_' or '.'))
            i++;

        if (i == nameStart)
            return null;

        int nameEnd = i;
        bool inString = false;
        bool hasHostTag = false;
        char quote = '\0';
        char lastNonWhitespace = '\0';

        while (i < src.Length)
        {
            char c = src[i];

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

            if (c == '<')
                return null;

            if (c == '>')
                return new TagInfo(
                    nameStart,
                    nameEnd,
                    isClose,
                    lastNonWhitespace == '/',
                    hasHostTag,
                    i + 1
                );

            if (
                c == '@'
                && !isClose
                && char.IsWhiteSpace(src[i - 1])
                && Markers.Matches(src, i + 1, Markers.HostTag)
            )
                hasHostTag = true;

            if (!char.IsWhiteSpace(c))
                lastNonWhitespace = c;
            i++;
        }

        return null;
    }
}
