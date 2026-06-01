using MinBlazor.Core;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MinBlazor.Models;

namespace MinBlazor.Services;

public sealed class StaticAssets
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private readonly IReadOnlyList<string> _contentRoots;
    private readonly AssetNode _root;

    private StaticAssets(IReadOnlyList<string> contentRoots, AssetNode root)
    {
        _contentRoots = contentRoots;
        _root = root;
    }

    public static Result<StaticAssets> Load(string manifestPath)
    {
        try
        {
            var manifest = JsonSerializer.Deserialize<AssetManifest>(File.ReadAllText(manifestPath), Options);
            if (manifest?.ContentRoots is null || manifest.Root is null)
                return Result<StaticAssets>.Fail($"Could not read static web assets manifest: {manifestPath}");

            return Result<StaticAssets>.Ok(new StaticAssets(manifest.ContentRoots, manifest.Root));
        }
        catch (Exception ex)
        {
            return Result<StaticAssets>.Fail($"Failed to load static web assets manifest: {ex.Message}");
        }
    }

    public bool TryResolve(string urlPath, [MaybeNullWhen(false)] out string physicalPath)
    {
        physicalPath = null;

        var node = _root;
        var trimmed = urlPath.Trim('/');
        if (trimmed.Length > 0)
        {
            foreach (var segment in trimmed.Split('/'))
            {
                if (node.Children is null || !node.Children.TryGetValue(segment, out var child))
                    return false;

                node = child;
            }
        }

        if (node.Asset is null)
            return false;

        var root = _contentRoots[node.Asset.ContentRootIndex];
        physicalPath = Path.GetFullPath(Path.Combine(root, node.Asset.SubPath));
        return File.Exists(physicalPath);
    }
}

