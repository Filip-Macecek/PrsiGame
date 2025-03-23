using MediatR;
using System.Net.WebSockets;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using PrsiGame.WebSockets;
using PrsiWeb.Commands;
using PrsiGame.WebSockets.Models;

namespace PrsiWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ILogger<SessionController> _logger;
    private readonly IMediator _mediator;

    public SessionController(
        ILogger<SessionController> logger,
        IMediator mediator)
    {
        _logger = logger;
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
            Result<IDictionary<string, object>> jsonResult;
            try
            {
                var timeout = new TimeSpan(0, 0, 0, seconds: 3);
                jsonResult = await socket.ReceiveJsonAsync(ct).WaitAsync(timeout, ct);
            }
            catch (TimeoutException e)
            {
                var disconnectCommand = new DisconnectCommand(socket);
                await _mediator.Send(disconnectCommand, ct);
                _logger.LogError(e, "Client has timed out.");
                return;
            }

            if (jsonResult.IsFailed && !jsonResult.HasError(e => e is WebSocketClosedError))
            {
                continue;
            }
            try
            {
                var command = GetCommand(jsonResult, socket);
                if (command != null)
                {
                    await _mediator.Send(command, ct);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during processing a command. Disconnecting.");
                var disconnectCommand = new DisconnectCommand(socket);
                await _mediator.Send(disconnectCommand, ct);
                return;
            }
        }
    }

    private IRequest? GetCommand(Result<IDictionary<string, object>> jsonResult, JsonWebSocket socket)
    {
        if (jsonResult.HasError(e => e is WebSocketClosedError))
        {
            return new DisconnectCommand(socket);
        }

        var json = jsonResult.Value;
        if (!json.TryGetValue("PrsiCommandType", out var value))
        {
            return null;
        }

        var commandType = value.ToString();
        if (!Enum.TryParse<PrsiCommandType>(commandType, ignoreCase: true, out var prsiCommandType))
        {
            throw new InvalidOperationException("Unknown command type.");
        }

        return prsiCommandType switch
        {
            PrsiCommandType.CreateSession => socket.Convert<CreateSessionCommandDto>(json)!.ToCommand(socket),
            PrsiCommandType.JoinLobby => socket.Convert<JoinLobbyCommandDto>(json)!.ToCommand(socket),
            PrsiCommandType.StartGame => socket.Convert<StartGameDto>(json)!.ToCommand(socket),
            PrsiCommandType.AddTurn => throw new NotImplementedException(),
            PrsiCommandType.Disconnect => throw new NotImplementedException(),
            PrsiCommandType.Healthcheck => null, // We just make sure the timeout is not reached.
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
