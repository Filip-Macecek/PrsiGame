using MediatR;
using System.Net.WebSockets;
using System.Text.Json;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using PrsiGame.WebSockets;
using PrsiWeb.Commands;
using PrsiWeb.Models;
using PrsiWeb.Services;

namespace PrsiWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ILogger<SessionController> _logger;
    private readonly WebSocketClientService _clientService;
    private readonly IMediator _mediator;

    public SessionController(
        ILogger<SessionController> logger,
        WebSocketClientService clientService,
        IMediator mediator)
    {
        _logger = logger;
        _clientService = clientService;
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
            var oneSecond = new TimeSpan(0, 0, 0, 1);

            Result<JsonDocument> jsonResult;
            try
            {
                jsonResult = await socket.ReceiveJsonAsync(ct).WaitAsync(oneSecond, ct);
            }
            catch (TimeoutException)
            {
                return;
            }

            if (jsonResult.IsFailed && !jsonResult.HasError(e => e is WebSocketClosedError))
            {
                continue;
            }
            try
            {
                var command = GetCommand(jsonResult, socket);
                await _mediator.Send(command!, ct);
            }
            catch (Exception e)
            {
                // TODO: Send error.
                break;
            }
        }
    }

    private object? GetCommand(Result<JsonDocument> jsonResult, JsonWebSocket socket)
    {
        if (jsonResult.HasError(e => e is WebSocketClosedError))
        {
            var client = _clientService.GetClient(socket.WebSocket);
            if (client is null)
            {
                return null;
            }
            return new DisconnectCommand(socket, client.SessionId, client.PlayerId);
        }

        var json = jsonResult.Value;
        if (!json.RootElement.TryGetProperty("PrsiCommandType", out _))
        {
            return null;
        }

        var commandType = json.RootElement.GetProperty("PrsiCommandType").GetString();
        if (!Enum.TryParse<PrsiCommandType>(commandType, ignoreCase: true, out var prsiCommandType))
        {
            throw new InvalidOperationException("Unknown command type.");
        }

        object? command = prsiCommandType switch
        {
            PrsiCommandType.CreateSession => socket.Convert<CreateSessionCommandDto>(json)!.ToCommand(socket),
            PrsiCommandType.JoinLobby => socket.Convert<JoinLobbyCommandDto>(json)!.ToCommand(socket),
            PrsiCommandType.StartGame => throw new NotImplementedException(),
            PrsiCommandType.AddTurn => throw new NotImplementedException(),
            PrsiCommandType.Disconnect => throw new NotImplementedException(),
            PrsiCommandType.Healthcheck => null, // We just make sure the timeout is not reached.
            _ => throw new ArgumentOutOfRangeException()
        };
        return command;
    }
}
