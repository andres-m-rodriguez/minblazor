using MinBlazor.Parser;

namespace MinBlazor.Compiler;

public sealed class Analyzer
{
    public ComponentGraph Analyze(Document document)
    {
        var roots = new List<ComponentNode>();
        var components = new HashSet<string>();
        var stack = new Stack<(string Name, List<ComponentNode> Children)>();

        foreach (var node in document.Nodes)
        {
            switch (node.Kind)
            {
                case NodeKind.ComponentOpen:
                {
                    var name = ComponentName.Of(node);
                    components.Add(name);
                    stack.Push((name, []));
                    break;
                }

                case NodeKind.ComponentSelfClose:
                {
                    var name = ComponentName.Of(node);
                    components.Add(name);
                    Add(stack, roots, new ComponentNode(name, []));
                    break;
                }

                case NodeKind.ComponentClose when stack.Count > 0:
                {
                    var (name, children) = stack.Pop();
                    Add(stack, roots, new ComponentNode(name, children));
                    break;
                }
            }
        }

        while (stack.Count > 0)
        {
            var (name, children) = stack.Pop();
            Add(stack, roots, new ComponentNode(name, children));
        }

        return new ComponentGraph(roots, components);
    }

    private static void Add(
        Stack<(string Name, List<ComponentNode> Children)> stack,
        List<ComponentNode> roots,
        ComponentNode node)
    {
        if (stack.Count > 0)
            stack.Peek().Children.Add(node);
        else
            roots.Add(node);
    }
}
