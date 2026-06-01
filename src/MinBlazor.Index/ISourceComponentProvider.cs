namespace MinBlazor.Index;

public interface ISourceComponentProvider
{
    IEnumerable<(string Name, string Path)> GetComponents();
}
