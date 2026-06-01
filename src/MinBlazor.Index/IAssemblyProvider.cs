using Microsoft.CodeAnalysis;

namespace MinBlazor.Index;

public interface IAssemblyProvider
{
    IEnumerable<(string Name, MetadataReference Reference)> GetAssemblies();
}
