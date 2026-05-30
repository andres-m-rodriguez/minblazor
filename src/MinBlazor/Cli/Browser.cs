using System.Diagnostics;

namespace MinBlazor.Cli;

public static class Browser
{
    public static bool TryOpen(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
