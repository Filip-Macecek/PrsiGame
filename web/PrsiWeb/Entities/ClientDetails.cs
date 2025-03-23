using PrsiGame.WebSockets;

namespace PrsiWeb.Entities;

public sealed record ClientDetails(Guid Id, JsonWebSocket WebSocket, Guid PlayerId, Guid SessionId);
