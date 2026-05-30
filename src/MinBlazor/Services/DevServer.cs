using System.Diagnostics;

namespace MinBlazor.Services;

public sealed class DevServer : IDisposable
{
    private readonly Process _process;
    private bool _started;

    public DevServer(string scaffoldDir)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = scaffoldDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                ArgumentList = { "run", "--project", "." },
            },
        };

        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                OutputLine?.Invoke(e.Data);
        };

        _process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                ErrorLine?.Invoke(e.Data);
        };
    }

    public event Action<string>? OutputLine;
    public event Action<string>? ErrorLine;

    public void Start()
    {
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        _started = true;
    }

    public void Stop()
    {
        try
        {
            if (_started && !_process.HasExited)
                _process.Kill(entireProcessTree: true);
        }
        catch
        {
        }
    }

    public int WaitForExit()
    {
        _process.WaitForExit();
        return _process.ExitCode;
    }

    public void Dispose() => _process.Dispose();
}
