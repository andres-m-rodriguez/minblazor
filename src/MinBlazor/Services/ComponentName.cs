using System.Text;

namespace MinBlazor.Services;

public static class ComponentName
{
    public static string From(string fileNameWithoutExtension)
    {
        var sb = new StringBuilder(fileNameWithoutExtension.Length);
        foreach (var c in fileNameWithoutExtension)
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');

        if (sb.Length == 0 || char.IsDigit(sb[0]))
            sb.Insert(0, '_');

        return sb.ToString();
    }
}
