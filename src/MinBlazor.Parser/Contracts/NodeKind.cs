namespace MinBlazor.Parser;

public enum NodeKind
{
    Text,
    ComponentOpen,
    ComponentClose,
    ComponentSelfClose,
    HostTag,
    Directive,
    StyleBlock,
}
