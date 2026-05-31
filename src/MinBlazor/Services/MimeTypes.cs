namespace MinBlazor.Services;

internal static class MimeTypes
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html",
        [".js"] = "text/javascript",
        [".mjs"] = "text/javascript",
        [".css"] = "text/css",
        [".wasm"] = "application/wasm",
        [".json"] = "application/json",
        [".map"] = "application/json",
        [".dat"] = "application/octet-stream",
        [".pdb"] = "application/octet-stream",
        [".dll"] = "application/octet-stream",
        [".blat"] = "application/octet-stream",
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/x-icon",
        [".txt"] = "text/plain",
        [".webmanifest"] = "application/manifest+json",
    };

    public static string For(string extension) =>
        Map.TryGetValue(extension, out var mime) ? mime : "application/octet-stream";
}
