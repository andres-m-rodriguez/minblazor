namespace MinBlazor.Services;

public sealed class FileWatcher : IDisposable
{
    private readonly Action _onChange;
    private readonly FileSystemWatcher _watcher;
    private readonly Lock _deboundLock = new();
    private Timer? _debounce;
    private int _rebuilding = 0;

    public FileWatcher(string sourceDir, Action onChange)
    {
        _onChange = onChange;

        _watcher = new FileSystemWatcher(sourceDir)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
    }

    private void OnFileChanged(object _, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.Name ?? "");
        if (!ext.Equals(".razor", StringComparison.OrdinalIgnoreCase) &&
            !ext.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            return;

        lock (_deboundLock)
        {
            _debounce?.Dispose();
            _debounce = new Timer(_ => Fire(), null, dueTime: 300, period: Timeout.Infinite);
        }
    }

    private void Fire()
    {
        if (Interlocked.CompareExchange(ref _rebuilding, 1, 0) != 0)
            return;
        try { _onChange(); }
        finally { Interlocked.Exchange(ref _rebuilding, 0); }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        lock (_deboundLock) _debounce?.Dispose();
    }
}
