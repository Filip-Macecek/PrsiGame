using MediatR;
using PrsiGame.WebSockets;

namespace PrsiWeb.Commands;

public sealed record StartGameCommand(JsonWebSocket Socket) : IRequest;
