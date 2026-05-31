namespace MinBlazor.Models;

public sealed record AssetManifest(string[] ContentRoots, AssetNode Root);

public sealed record AssetNode(Dictionary<string, AssetNode>? Children, AssetEntry? Asset);

public sealed record AssetEntry(int ContentRootIndex, string SubPath);
