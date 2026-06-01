namespace MinBlazor.Parser;

internal readonly record struct TagInfo(
    int NameStart,
    int NameEnd,
    bool IsClose,
    bool SelfClosing,
    bool HasHostTag,
    int End
);
