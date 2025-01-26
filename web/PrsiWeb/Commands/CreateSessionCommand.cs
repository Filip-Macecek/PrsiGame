using MediatR;
using PrsiGame.WebSockets;
using PrsiWeb.Models;

namespace PrsiWeb.Commands;

public sealed record CreateSessionCommand(JsonWebSocket WebSocket, PlayerDto PlayerDto) : IRequest;
