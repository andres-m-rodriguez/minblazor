namespace MinBlazor.Scaffold;

public abstract record ScaffoldEntry;

public sealed record ProjectFile(ReadOnlyMemory<char> Content) : ScaffoldEntry;

public sealed record SourceFile(string Name, ReadOnlyMemory<char> Content) : ScaffoldEntry;

public sealed record ComponentFile(string Name, ReadOnlyMemory<char> Source) : ScaffoldEntry;

public sealed record StaticAsset(string Name, ReadOnlyMemory<byte> Content) : ScaffoldEntry;

public sealed record HostPage(ReadOnlyMemory<char> Content) : ScaffoldEntry;
