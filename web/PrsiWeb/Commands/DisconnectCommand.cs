using MediatR;
using PrsiGame.WebSockets;

namespace PrsiWeb.Commands;

public sealed record DisconnectCommand(JsonWebSocket WebSocket) : IRequest;
