namespace MinBlazor.Razor.Models;

public enum NodeKind
{
    Text,
    ComponentOpen,
    ComponentClose,
    ComponentSelfClose,
    HostTag,
    Directive,
}
