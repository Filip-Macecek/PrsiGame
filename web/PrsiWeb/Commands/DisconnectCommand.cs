using MediatR;
using PrsiGame.WebSockets;
using PrsiWeb.Models;

namespace PrsiWeb.Commands;

public sealed record DisconnectCommand(JsonWebSocket WebSocket, Guid SessionId, PlayerDto Player) : IRequest;
