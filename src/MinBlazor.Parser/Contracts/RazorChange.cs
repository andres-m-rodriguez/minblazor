namespace MinBlazor.Parser;

public abstract record RazorChange
{
    public sealed record None : RazorChange;

    public sealed record Full : RazorChange;

    public sealed record CssOnly(string Content) : RazorChange;
}
