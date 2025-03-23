using MediatR;
using PrsiWeb.Entities;
using PrsiWeb.Services;

namespace PrsiWeb.Commands;

public sealed class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand>
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly WebSocketClientService _webSocketClientService;

    public CreateSessionCommandHandler(IGameSessionRepository gameSessionRepository, WebSocketClientService webSocketClientService)
    {
        _gameSessionRepository = gameSessionRepository;
        _webSocketClientService = webSocketClientService;
    }

    public Task Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = _gameSessionRepository.CreateNew(new Player(request.PlayerDto.Id, request.PlayerDto.Name, PrsiPlayer: null));
        _webSocketClientService.Add(session, new ClientDetails(Guid.NewGuid(), request.WebSocket, request.PlayerDto.Id, session.Id));
        _webSocketClientService.UpdateAll(session.ToDto());
        return Task.CompletedTask;
    }
}
