using System.Diagnostics.CodeAnalysis;

namespace MinBlazor.Compiler;

public interface IComponentResolver
{
    bool TryResolve(string componentName, [MaybeNullWhen(false)] out string source);
}
