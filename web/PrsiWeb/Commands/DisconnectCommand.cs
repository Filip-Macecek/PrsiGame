using MediatR;
using PrsiGame.WebSockets;

namespace PrsiWeb.Commands;

public sealed record DisconnectCommand(JsonWebSocket WebSocket, Guid SessionId, Guid PlayerId) : IRequest;
