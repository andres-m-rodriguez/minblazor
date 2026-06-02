using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class CleanCommand(IOutput output)
{
    public int Execute(CleanOptions options)
    {
        if (options.RazorFile is not null)
        {
            var dir = PipelineCache.DirectoryFor(options.RazorFile);
            if (Directory.Exists(dir)) { Directory.Delete(dir, recursive: true); output.Info($"Cleaned {dir}"); }
            else output.Info("Nothing to clean.");
        }
        else
        {
            var root = Path.Combine(Path.GetTempPath(), "minblazor");
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); output.Info("Cleaned all minblazor caches."); }
            else output.Info("Nothing to clean.");
        }
        return 0;
    }
}
