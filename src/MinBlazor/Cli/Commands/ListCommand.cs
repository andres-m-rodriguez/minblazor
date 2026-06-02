using MinBlazor.Models;

namespace MinBlazor.Cli;

public sealed class ListCommand(IOutput output)
{
    public int Execute(ListOptions _)
    {
        var root = Path.Combine(Path.GetTempPath(), "minblazor");
        if (!Directory.Exists(root)) { output.Info("No cached scaffolds."); return 0; }

        var dirs = Directory.GetDirectories(root);
        if (dirs.Length == 0) { output.Info("No cached scaffolds."); return 0; }

        output.Info($"{dirs.Length} cached scaffold(s) in {root}:\n");
        foreach (var dir in dirs.OrderByDescending(d => Directory.GetLastWriteTimeUtc(d)))
        {
            var name = Path.GetFileName(dir);
            var age = DateTime.UtcNow - Directory.GetLastWriteTimeUtc(dir);
            var ageStr = age.TotalDays >= 1 ? $"{(int)age.TotalDays}d ago" : $"{(int)age.TotalHours}h ago";
            var hasBin = Directory.Exists(Path.Combine(dir, "bin")) ? " [built]" : "";
            output.Info($"  {name}  {ageStr}{hasBin}");
        }
        return 0;
    }
}
