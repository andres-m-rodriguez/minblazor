using System.Net;

namespace MinBlazor.Services;

public sealed class StaticServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly StaticAssets _assets;
    private readonly ManualResetEventSlim _stopped = new(false);
    private Thread? _thread;

    public StaticServer(StaticAssets assets, int port)
    {
        _assets = assets;
        _listener.Prefixes.Add($"http://localhost:{port}/");
    }

    public void Start()
    {
        _listener.Start();
        _thread = new Thread(AcceptLoop) { IsBackground = true };
        _thread.Start();
    }

    public void Stop()
    {
        try
        {
            if (_listener.IsListening)
                _listener.Stop();
        }
        catch { }

        _stopped.Set();
    }

    public void WaitForExit() => _stopped.Wait();

    private void AcceptLoop()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = _listener.GetContext();
            }
            catch
            {
                break;
            }

            Handle(context);
        }

        _stopped.Set();
    }

    private void Handle(HttpListenerContext context)
    {
        try
        {
            var path = Uri.UnescapeDataString(context.Request.Url!.AbsolutePath);
            if (path is "/" or "")
                path = "/index.html";

            if (_assets.TryResolve(path, out var file))
                Send(context.Response, file);
            else if (!Path.HasExtension(path) && _assets.TryResolve("/index.html", out var index))
                Send(context.Response, index);
            else
                context.Response.StatusCode = 404;
        }
        catch
        {
            context.Response.StatusCode = 500;
        }
        finally
        {
            context.Response.Close();
        }
    }

    private static void Send(HttpListenerResponse response, string file)
    {
        var bytes = File.ReadAllBytes(file);
        response.ContentType = MimeTypes.For(Path.GetExtension(file));
        response.ContentLength64 = bytes.Length;
        response.OutputStream.Write(bytes, 0, bytes.Length);
    }

    public void Dispose()
    {
        Stop();
        ((IDisposable)_listener).Dispose();
        _stopped.Dispose();
    }
}
