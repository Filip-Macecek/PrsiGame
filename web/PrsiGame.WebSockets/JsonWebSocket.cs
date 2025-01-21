using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PrsiGame.WebSockets;

public sealed class JsonWebSocket : IDisposable
{
    private const int BufferSize = 1024 * 4;

    public JsonWebSocket(WebSocket webSocket)
    {
        WebSocket = webSocket;
    }

    public WebSocketState State => WebSocket.State;
    public WebSocket WebSocket { get; private set; }

    public async Task SendAsync<T>(T dto, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(dto);
        var utf8 = Encoding.UTF8.GetBytes(json);

        try
        {
            await WebSocket.SendAsync(utf8, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (IOException)
        {
            Dispose();
        }
    }

    public async Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken) where T : class
    {
        if (WebSocket.CloseStatus != null)
        {
            return null;
        }

        var bytes = new List<byte>();
        var buffer = new byte[BufferSize];

        WebSocketReceiveResult result;
        try
        {
            result = await WebSocket.ReceiveAsync(buffer, cancellationToken);
        }
        catch (IOException ex)
        {
            // await WebSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Close", cancellationToken);
            Dispose();
            return null;
        }

        if (result.CloseStatus != null)
        {
            await WebSocket.CloseOutputAsync(result.CloseStatus.Value, "Close", cancellationToken);
            Dispose();
            return null;
        }

        while (!result.EndOfMessage)
        {
            bytes.AddRange(buffer[..result.Count]);
            result = await WebSocket.ReceiveAsync(buffer, cancellationToken);
        }
        bytes.AddRange(buffer[..result.Count]);

        var socketMessage = Encoding.UTF8.GetString(bytes.ToArray(), 0, result.Count);
        return JsonSerializer.Deserialize<T>(socketMessage);
    }

    public void Dispose()
    {
        WebSocket.Dispose();
    }
}
