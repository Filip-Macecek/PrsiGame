using MediatR;
using PrsiWeb.Entities;
using PrsiWeb.Services;

namespace PrsiWeb.Commands;

public sealed class JoinLobbyCommandHandler : IRequestHandler<JoinLobbyCommand>
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly WebSocketClientService _webSocketClientService;

    public JoinLobbyCommandHandler(IGameSessionRepository gameSessionRepository, WebSocketClientService webSocketClientService)
    {
        _gameSessionRepository = gameSessionRepository;
        _webSocketClientService = webSocketClientService;
    }

    public Task Handle(JoinLobbyCommand request, CancellationToken cancellationToken)
    {
        var updatedSession = _gameSessionRepository.AddPlayer(request.SessionId, request.Player);
        if (updatedSession is not null)
        {
            _webSocketClientService.Add(updatedSession, new WebSocketClient(Guid.NewGuid(), request.WebSocket, request.Player.Id));
            _webSocketClientService.UpdateAll(updatedSession.ToDto());
        }

        return Task.CompletedTask;
    }
}
