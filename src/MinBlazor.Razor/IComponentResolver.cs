using System.Diagnostics.CodeAnalysis;
using MinBlazor.Compiler;

namespace MinBlazor.Razor;

public interface IComponentResolver
{
    ResolveResult TryResolve(string componentName, [NotNullWhen(true)] out string? source);
}
