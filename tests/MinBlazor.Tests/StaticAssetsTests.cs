using MinBlazor.Core;
using MinBlazor.Services;

namespace MinBlazor.Tests;

public class StaticAssetsTests
{
    [Test]
    public async Task ResolvesRootRelativeAndForeignContentRoots()
    {
        var assets = Build();

        await Assert.That(assets.TryResolve("/index.html", out var index)).IsTrue();
        await Assert.That(File.Exists(index)).IsTrue();

        await Assert.That(assets.TryResolve("/_framework/app.wasm", out var wasm)).IsTrue();
        await Assert.That(File.Exists(wasm)).IsTrue();

        // a foreign content root index, the way MudBlazor's _content assets map
        await Assert.That(assets.TryResolve("/_content/Lib/lib.css", out var css)).IsTrue();
        await Assert.That(css!.Contains("root1")).IsTrue();
    }

    [Test]
    public async Task ReturnsFalseForUnknownIntermediateOrMissingFile()
    {
        var assets = Build();

        await Assert.That(assets.TryResolve("/nope.js", out _)).IsFalse();      // unknown segment
        await Assert.That(assets.TryResolve("/_framework", out _)).IsFalse();   // intermediate node, no asset
        await Assert.That(assets.TryResolve("/missing.txt", out _)).IsFalse();  // mapped but file absent on disk
    }

    private static StaticAssets Build()
    {
        var dir = Path.Combine(Path.GetTempPath(), "minblazor-tests", Guid.NewGuid().ToString("n"));
        var root0 = Path.Combine(dir, "root0");
        var root1 = Path.Combine(dir, "root1");
        Directory.CreateDirectory(Path.Combine(root0, "_framework"));
        Directory.CreateDirectory(root1);

        File.WriteAllText(Path.Combine(root0, "index.html"), "<html></html>");
        File.WriteAllText(Path.Combine(root0, "_framework", "app.wasm"), "wasm");
        File.WriteAllText(Path.Combine(root1, "lib.css"), "body{}");

        var manifestPath = Path.Combine(dir, "manifest.json");
        File.WriteAllText(manifestPath, Manifest(root0, root1));

        var result = StaticAssets.Load(manifestPath);
        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error);

        return result.Value!;
    }

    private static string Manifest(string root0, string root1)
    {
        var r0 = root0.Replace("\\", "\\\\");
        var r1 = root1.Replace("\\", "\\\\");
        return $$"""
        {
          "ContentRoots": ["{{r0}}", "{{r1}}"],
          "Root": { "Children": {
            "index.html": { "Children": null, "Asset": { "ContentRootIndex": 0, "SubPath": "index.html" } },
            "_framework": { "Children": { "app.wasm": { "Children": null, "Asset": { "ContentRootIndex": 0, "SubPath": "_framework/app.wasm" } } }, "Asset": null },
            "_content": { "Children": { "Lib": { "Children": { "lib.css": { "Children": null, "Asset": { "ContentRootIndex": 1, "SubPath": "lib.css" } } }, "Asset": null } }, "Asset": null },
            "missing.txt": { "Children": null, "Asset": { "ContentRootIndex": 0, "SubPath": "missing.txt" } }
          }, "Asset": null }
        }
        """;
    }
}

