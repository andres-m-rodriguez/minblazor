using System.Diagnostics.CodeAnalysis;

namespace MinBlazor.Razor;

public interface IComponentResolver
{
    bool TryResolve(string componentName, [MaybeNullWhen(false)] out string source);
}
