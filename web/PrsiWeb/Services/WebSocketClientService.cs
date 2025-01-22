using System.Net.WebSockets;
using PrsiGame.WebSockets;
using PrsiWeb.Entities;
using PrsiWeb.Models;

namespace PrsiWeb.Services;

public sealed class WebSocketClientService
{
    private readonly IDictionary<Guid, List<JsonWebSocket>> _clients;

    public WebSocketClientService()
    {
        _clients = new Dictionary<Guid, List<JsonWebSocket>>();
    }

    public void Add(GameSession gameSession, JsonWebSocket webSocket)
    {
        lock (_clients)
        {
            if (!_clients.ContainsKey(gameSession.Id))
            {
                _clients.Add(gameSession.Id, []);
            }
            _clients[gameSession.Id].Add(webSocket);
        }
    }

    public void UpdateAll(SessionDto sessionDto)
    {
        lock (_clients)
        {
            foreach (var client in _clients[sessionDto.Id].Where(c => c.State == WebSocketState.Open))
            {
                client.SendAsync(sessionDto, CancellationToken.None).GetAwaiter().GetResult();
            }
        }
    }
}
