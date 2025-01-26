using MediatR;
using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PrsiGame.WebSockets;
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
    private readonly IMediator _mediator;

    public SessionController(
        ILogger<SessionController> logger,
        WebSocketClientService clientService,
        IGameSessionRepository gameSessionRepository,
        IMediator mediator)
    {
        _logger = logger;
        _clientService = clientService;
        _gameSessionRepository = gameSessionRepository;
        _mediator = mediator;
    }

    [HttpGet("connect")]
    public async Task Connect(string? id, CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using var socket = new JsonWebSocket($"({id ?? ""}) Connect endpoint", await HttpContext.WebSockets.AcceptWebSocketAsync(), s => _logger.LogInformation(s));

        while (socket.State == WebSocketState.Open)
        {
            JsonDocument? json = null;
            if (id != "receive_task")
            {
                json = await socket.ReceiveJsonAsync(ct);
            }
            else
            {
                Thread.Sleep(9000000);
            }

            if (json == null) continue; // TODO
            try
            {
                if (!json.RootElement.TryGetProperty("PrsiCommandType", out _))
                {
                    continue;
                }

                var commandType = json.RootElement.GetProperty("PrsiCommandType").GetString();
                if (!Enum.TryParse<PrsiCommandType>(commandType, ignoreCase: true, out var prsiCommandType))
                {
                    throw new InvalidOperationException("Unknown command type.");
                }

                object command = prsiCommandType switch
                {
                    PrsiCommandType.CreateSession => socket.Convert<CreateSessionCommandDto>(json)!.ToCommand(socket),
                    PrsiCommandType.JoinLobby => socket.Convert<JoinLobbyCommandDto>(json)!.ToCommand(socket),
                    PrsiCommandType.StartGame => throw new NotImplementedException(),
                    PrsiCommandType.AddTurn => throw new NotImplementedException(),
                    PrsiCommandType.Disconnect => throw new NotImplementedException(),
                    PrsiCommandType.Healthcheck => throw new NotImplementedException(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                await _mediator.Send(command!, ct);
            }
            catch (Exception e)
            {
                // TODO: Send error.
                break;
            }
        }
    }
}
