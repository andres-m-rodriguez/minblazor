using System.Net;
using System.Net.WebSockets;

namespace MinBlazor.Services;

public sealed class StaticServer : IDisposable
{
    private const string WsPath = "/_minblazor/ws";

    private readonly HttpListener _listener = new();
    private StaticAssets _assets;
    private readonly Lock _assetsLock = new();
    private readonly ReloadBroadcaster? _broadcaster;
    private readonly ManualResetEventSlim _stopped = new(false);
    private Thread? _thread;

    public StaticServer(StaticAssets assets, int port, ReloadBroadcaster? broadcaster = null)
    {
        _assets = assets;
        _broadcaster = broadcaster;
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

    public void UpdateAssets(StaticAssets assets) { lock (_assetsLock) _assets = assets; }

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

            var path = Uri.UnescapeDataString(context.Request.Url!.AbsolutePath);
            if (_broadcaster is not null && context.Request.IsWebSocketRequest && path == WsPath)
                _ = Task.Run(() => HandleWebSocketAsync(context));
            else
                Handle(context, path);
        }

        _stopped.Set();
    }

    private void Handle(HttpListenerContext context, string path)
    {
        try
        {
            if (path is "/" or "")
                path = "/index.html";

            StaticAssets assets;
            lock (_assetsLock) assets = _assets;

            if (assets.TryResolve(path, out var file))
                Send(context.Response, file);
            else if (!Path.HasExtension(path) && assets.TryResolve("/index.html", out var index))
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

    private async Task HandleWebSocketAsync(HttpListenerContext context)
    {
        WebSocket ws;
        try
        {
            var wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
            ws = wsContext.WebSocket;
        }
        catch
        {
            return;
        }

        _broadcaster!.Add(ws);
        try
        {
            var buf = new byte[256];
            while (ws.State == WebSocketState.Open)
                await ws.ReceiveAsync(buf, CancellationToken.None);
        }
        catch { }
        finally
        {
            _broadcaster.Remove(ws);
            ws.Dispose();
        }
    }

    private static void Send(HttpListenerResponse response, string file)
    {
        var bytes = File.ReadAllBytes(file);
        var ext = Path.GetExtension(file);
        response.ContentType = MimeTypes.For(ext);
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
