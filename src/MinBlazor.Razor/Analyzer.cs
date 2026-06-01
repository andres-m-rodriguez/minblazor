using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

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
                    if (stack.Count > 0)
                        stack.Peek().Children.Add(new ComponentNode(name, []));
                    else
                        roots.Add(new ComponentNode(name, []));
                    break;
                }

                case NodeKind.ComponentClose when stack.Count > 0:
                {
                    var (name, children) = stack.Pop();
                    if (stack.Count > 0)
                        stack.Peek().Children.Add(new ComponentNode(name, children));
                    else
                        roots.Add(new ComponentNode(name, children));
                    break;
                }
            }
        }

        while (stack.Count > 0)
        {
            var (name, children) = stack.Pop();
            if (stack.Count > 0)
                stack.Peek().Children.Add(new ComponentNode(name, children));
            else
                roots.Add(new ComponentNode(name, children));
        }

        return new ComponentGraph(roots, components);
    }
}
