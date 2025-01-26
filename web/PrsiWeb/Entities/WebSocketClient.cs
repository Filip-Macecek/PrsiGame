using PrsiGame.WebSockets;

namespace PrsiWeb.Entities;

public sealed record WebSocketClient(Guid Id, JsonWebSocket WebSocket, Guid PlayerId);
