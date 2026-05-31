namespace MinBlazor.Razor;

public sealed class DuplicateComponentException(string componentName)
    : Exception($"A component named '{componentName}' is already registered.")
{
    public string ComponentName { get; } = componentName;
}
