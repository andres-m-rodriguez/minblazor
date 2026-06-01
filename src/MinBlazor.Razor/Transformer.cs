using MinBlazor.Razor.Models;

namespace MinBlazor.Razor;

public sealed class Transformer
{
    public TransformResult Transform(Document document)
    {
        var kept = new List<RazorNode>();
        var hostTags = new List<RazorNode>();
        var packages = new List<PackageReference>();

        foreach (var node in document.Nodes)
        {
            switch (node.Kind)
            {
                case NodeKind.HostTag:
                    hostTags.Add(node);
                    break;

                case NodeKind.Directive:
                    if (Scanner.TryParsePackage(node.Text.Span, out var package))
                        packages.Add(package);
                    break;

                default:
                    kept.Add(node);
                    break;
            }
        }

        return new TransformResult(new Document(document.Source, kept), hostTags, packages);
    }
}


