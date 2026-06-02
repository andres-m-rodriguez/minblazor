using System.Security.Cryptography;
using System.Text;

namespace MinBlazor.Cli;

public static class PipelineCache
{
    public static string DirectoryFor(string razorPath)
    {
        var path = Path.GetFullPath(razorPath);
        if (OperatingSystem.IsWindows())
            path = path.ToLowerInvariant();

        var hash = Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path)))[..16]
            .ToLowerInvariant();
        return Path.Combine(Path.GetTempPath(), "minblazor", hash);
    }
}
