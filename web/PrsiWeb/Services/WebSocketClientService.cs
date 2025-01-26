using System.Net.WebSockets;
using PrsiWeb.Entities;
using PrsiWeb.Models;

namespace PrsiWeb.Services;

public sealed class WebSocketClientService
{
    private readonly ILogger _logger;
    private readonly IDictionary<Guid, List<WebSocketClient>> _clients;

    public WebSocketClientService(ILogger<WebSocketClientService> logger)
    {
        _logger = logger;
        _clients = new Dictionary<Guid, List<WebSocketClient>>();
    }

    public WebSocketClient? GetClient(WebSocket webSocket)
    {
        lock (_clients)
        {
            var client = _clients.Values
                .Select(clients => clients.SingleOrDefault(c => c.WebSocket.WebSocket == webSocket))
                .Where(c => c is not null);
            return client.SingleOrDefault();
        }
    }

    public void Remove(Guid sessionId, Guid playerId)
    {
        lock (_clients)
        {
            _clients.TryGetValue(sessionId, out var clients);
            if (clients != null)
            {
                var clientsToDelete = clients.Where(s => s.PlayerId == playerId).ToList();
                if (clientsToDelete.Any())
                {
                    foreach (var client in clientsToDelete)
                    {
                        client.WebSocket.Dispose();
                        clients.Remove(client);
                    }
                }
            }
        }
    }

    public void Remove(Guid sessionId)
    {
        lock (_clients)
        {
            _clients.TryGetValue(sessionId, out var clients);
            if (clients != null)
            {
                foreach (var client in clients)
                {
                    client.WebSocket.Dispose();
                }

                _clients.Remove(sessionId);
            }
        }
    }

    public void Add(GameSession gameSession, WebSocketClient client)
    {
        lock (_clients)
        {
            if (!_clients.ContainsKey(gameSession.Id))
            {
                _clients.Add(gameSession.Id, []);
            }
            _clients[gameSession.Id].Add(client);
        }
    }

    public void UpdateAll(SessionDto sessionDto)
    {
        lock (_clients)
        {
            var webSocketClients = _clients[sessionDto.Id].Where(c => c.WebSocket.State == WebSocketState.Open).ToList();
            _logger.LogInformation($"Updating client ids: {webSocketClients.Select(c => c.Id.ToString())}");

            foreach (var client in webSocketClients)
            {
                client.WebSocket.SendAsync(sessionDto, CancellationToken.None).GetAwaiter().GetResult();
            }
        }
    }
}
