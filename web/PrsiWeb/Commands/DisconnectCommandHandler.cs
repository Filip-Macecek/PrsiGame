using MediatR;
using PrsiWeb.Services;

namespace PrsiWeb.Commands;

public sealed class DisconnectCommandHandler : IRequestHandler<DisconnectCommand>
{
    private readonly ILogger<DisconnectCommandHandler> _logger;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly WebSocketClientService _webSocketClientService;

    public DisconnectCommandHandler(
        ILogger<DisconnectCommandHandler> logger,
        IGameSessionRepository gameSessionRepository,
        WebSocketClientService webSocketClientService)
    {
        _logger = logger;
        _gameSessionRepository = gameSessionRepository;
        _webSocketClientService = webSocketClientService;
    }

    public Task Handle(DisconnectCommand request, CancellationToken cancellationToken)
    {
        var webSocketClient = _webSocketClientService.GetClientDetails(request.WebSocket.WebSocket);
        if (webSocketClient == null)
        {
            _logger.LogInformation($"WebSocketClient was not found for websocket '{request.WebSocket.Id}'");
            return Task.CompletedTask;
        }
        var session = _gameSessionRepository.RemovePlayer(webSocketClient.SessionId, webSocketClient.PlayerId);
        if (session.Author.Id == webSocketClient.PlayerId)
        {
            _gameSessionRepository.End(session.Id);
            _webSocketClientService.UpdateAll(session.ToDto());
            _webSocketClientService.Remove(session.Id);
        }
        else
        {
            _webSocketClientService.UpdateAll(session.ToDto());
            _webSocketClientService.Remove(webSocketClient.SessionId, webSocketClient.PlayerId);
        }

        return Task.CompletedTask;
    }
}
