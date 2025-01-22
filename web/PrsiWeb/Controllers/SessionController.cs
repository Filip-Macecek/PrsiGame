using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PrsiGame.WebSockets;
using PrsiWeb.Entities;
using PrsiWeb.Models;
using PrsiWeb.Services;

namespace PrsiWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ILogger<SessionController> _logger;
    private readonly WebSocketClientService _clientService;
    private readonly IGameSessionRepository _gameSessionRepository;

    public SessionController(
        ILogger<SessionController> logger,
        WebSocketClientService clientService,
        IGameSessionRepository gameSessionRepository)
    {
        _logger = logger;
        _clientService = clientService;
        _gameSessionRepository = gameSessionRepository;
    }

    [HttpGet("connect")]
    public async Task Connect(Guid sessionId, CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest || sessionId == Guid.Empty)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        var session = _gameSessionRepository.Get(sessionId);

        if (session == null)
        {
            throw new InvalidOperationException($"Session '{sessionId}' has not been found.");
        }

        using var socket = new JsonWebSocket(await HttpContext.WebSockets.AcceptWebSocketAsync());
        _clientService.Add(session, socket);

        if (socket.State == WebSocketState.Open)
        {
            var sessionDto = session.ToDto();
            await socket.SendAsync(sessionDto, ct);
        }

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync<ConnectPlayerDto>(ct);

            if (result != null)
            {
                // TODO: Reconnect.
                var player = new Player(Guid.NewGuid(), result!.Name ?? "New Player");
                var updatedSession = _gameSessionRepository.AddPlayer(sessionId, player);

                var response = updatedSession.ToDto();
                _clientService.UpdateAll(response);
            }
        }
    }
}
