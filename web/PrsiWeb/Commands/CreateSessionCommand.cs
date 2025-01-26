using MediatR;
using PrsiGame.WebSockets;
using PrsiGame.WebSockets.Models;

namespace PrsiWeb.Commands;

public sealed record CreateSessionCommand(JsonWebSocket WebSocket, PlayerDto PlayerDto) : IRequest;
