using MediatR;
using PrsiGame.WebSockets;
using PrsiWeb.Entities;

namespace PrsiWeb.Commands;

public sealed record JoinLobbyCommand(JsonWebSocket WebSocket, Guid SessionId, Player Player) : IRequest;
