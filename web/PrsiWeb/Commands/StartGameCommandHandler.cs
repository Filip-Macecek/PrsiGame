using MediatR;
using PrsiWeb.Services;

namespace PrsiWeb.Commands;

public sealed class StartGameCommandHandler : IRequestHandler<StartGameCommand>
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly WebSocketClientService _webSocketClientService;

    public StartGameCommandHandler(IGameSessionRepository gameSessionRepository, WebSocketClientService webSocketClientService)
    {
        _gameSessionRepository = gameSessionRepository;
        _webSocketClientService = webSocketClientService;
    }

    public Task Handle(StartGameCommand request, CancellationToken cancellationToken)
    {
        var clientDetails = _webSocketClientService.GetClientDetails(request.Socket.WebSocket);
        if (clientDetails is null)
        {
            throw new ArgumentException("Invalid client socket.");
        }
        var updatedSession = _gameSessionRepository.Start(clientDetails.SessionId);
        _webSocketClientService.UpdateAll(updatedSession.ToDto());

        return Task.CompletedTask;
    }
}
