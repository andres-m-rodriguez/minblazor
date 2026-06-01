using System.Diagnostics.CodeAnalysis;

namespace MinBlazor.Compiler;

public interface IComponentResolver
{
    ResolveResult TryResolve(string componentName, [NotNullWhen(true)] out string? source);
}
