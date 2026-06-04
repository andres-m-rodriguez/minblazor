using System.Net.WebSockets;
using System.Text;

namespace MinBlazor.Services;

public sealed class ReloadBroadcaster : IDisposable
{
    private readonly List<WebSocket> _clients = [];
    private readonly Lock _lock = new();

    public void Add(WebSocket ws)
    {
        lock (_lock)
            _clients.Add(ws);
    }

    public void Remove(WebSocket ws)
    {
        lock (_lock)
            _clients.Remove(ws);
    }

    public void Broadcast(string message)
    {
        var bytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));

        List<WebSocket> snapshot;
        lock (_lock)
            snapshot = [.. _clients];

        foreach (var ws in snapshot)
        {
            if (ws.State == WebSocketState.Open)
                ws.SendAsync(
                    bytes,
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None
                );
        }
    }

    public void Dispose()
    {
        List<WebSocket> snapshot;
        lock (_lock)
        {
            snapshot = [.. _clients];
            _clients.Clear();
        }

        foreach (var ws in snapshot)
            ws.Dispose();
    }
}
