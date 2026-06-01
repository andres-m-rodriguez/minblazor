namespace MinBlazor.Parser;

public sealed class Transformer
{
    public TransformResult Transform(Document document)
    {
        var kept = new List<RazorNode>();
        var hostTags = new List<HostTag>();
        var packages = new List<PackageReference>();

        foreach (var node in document.Nodes)
        {
            switch (node.Kind)
            {
                case NodeKind.HostTag:
                    hostTags.Add(new HostTag(node, Markers.Find(node.Text.Span, Markers.HostTag)));
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
