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
    private readonly IPersistenceService _persistenceService;
    private readonly WebSocketClientService _clientService;

    public SessionController(
        ILogger<SessionController> logger,
        IPersistenceService persistenceService,
        WebSocketClientService clientService)
    {
        _logger = logger;
        _persistenceService = persistenceService;
        _clientService = clientService;
    }

    [HttpPut()]
    public SessionDto Put([FromBody] NewSessionDto sessionDto)
    {
        var player = _persistenceService.GetPlayer(sessionDto.PlayerId);

        if (player == null)
        {
            throw new InvalidOperationException($"Player '{sessionDto.PlayerId}' has not been found.");
        }

        var session = new Session(Guid.NewGuid(), [player], SessionState.Lobby);
        _persistenceService.Set(session);

        return session.ToDto();
    }

    [HttpGet()]
    public SessionDto Get([FromQuery] Guid id)
    {
        var session = _persistenceService.GetSession(id);

        if (session == null)
        {
            throw new InvalidOperationException($"Session '{id}' has not been found.");
        }

        return session.ToDto();
    }

    [HttpGet("connect")]
    public async Task Connect(Guid sessionId, CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest || sessionId == Guid.Empty)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        var session = _persistenceService.GetSession(sessionId);

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
            var result = await socket.ReceiveAsync<NewPlayerDto>(ct);

            if (result != null)
            {
                var player = new Player(Guid.NewGuid(), result!.Name ?? "New Player");
                session.Players.Add(player);

                var response = session.ToDto();
                _clientService.UpdateAll(response);
            }
        }
    }
}
