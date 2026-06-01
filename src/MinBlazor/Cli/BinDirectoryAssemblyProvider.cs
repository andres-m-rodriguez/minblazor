using Microsoft.CodeAnalysis;
using MinBlazor.Index;

namespace MinBlazor.Cli;

public sealed class BinDirectoryAssemblyProvider(string binDir) : IAssemblyProvider
{
    public IEnumerable<(string Name, MetadataReference Reference)> GetAssemblies()
    {
        if (!Directory.Exists(binDir))
            yield break;

        foreach (var dll in Directory.EnumerateFiles(binDir, "*.dll"))
            yield return (
                Path.GetFileNameWithoutExtension(dll),
                MetadataReference.CreateFromFile(dll)
            );
    }
}
