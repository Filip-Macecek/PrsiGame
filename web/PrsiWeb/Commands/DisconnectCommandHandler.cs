using MediatR;
using PrsiWeb.Services;

namespace PrsiWeb.Commands;

public sealed class DisconnectCommandHandler : IRequestHandler<DisconnectCommand>
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly WebSocketClientService _webSocketClientService;

    public DisconnectCommandHandler(
        IGameSessionRepository gameSessionRepository,
        WebSocketClientService webSocketClientService)
    {
        _gameSessionRepository = gameSessionRepository;
        _webSocketClientService = webSocketClientService;
    }

    public Task Handle(DisconnectCommand request, CancellationToken cancellationToken)
    {
        var session = _gameSessionRepository.RemovePlayer(request.SessionId, request.PlayerId);
        if (session.Author.Id == request.PlayerId)
        {
            _gameSessionRepository.End(session.Id);
            _webSocketClientService.UpdateAll(session.ToDto());
            _webSocketClientService.Remove(session.Id);
        }
        else
        {
            _webSocketClientService.UpdateAll(session.ToDto());
            _webSocketClientService.Remove(request.SessionId, request.PlayerId);
        }

        return Task.CompletedTask;
    }
}
