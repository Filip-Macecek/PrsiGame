using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PrsiGame.WebSockets;
using PrsiWeb.Entities;
using PrsiWeb.Models;
using PrsiWeb.Services;

namespace PrsiWeb.IntegrationTests;

[TestFixture]
public class SessionTests : TestBase
{
    private IPersistenceService _persistenceService;
    private WebSocketClientService _webSocketClientService;

    [SetUp]
    public void Setup()
    {
        _persistenceService = WebApplicationFactory.Services.GetRequiredService<IPersistenceService>();
        _webSocketClientService = WebApplicationFactory.Services.GetRequiredService<WebSocketClientService>();
    }


    [Test]
    public async Task Connect_WhenNotWebSocket_Fails()
    {
        var player = new Player(Guid.NewGuid(), "Filda");
        _persistenceService.Set(player);
        var sessionId = Guid.NewGuid();
        _persistenceService.Set(new Session(sessionId, [player], SessionState.Lobby));
        var newPlayer = new Player(Guid.NewGuid(), "Jana");
        var dto = new ConnectToSessionDto(newPlayer.Id, sessionId);

        var client = WebApplicationFactory.CreateClient();
        var response = await client.PostAsync("session/connect", new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task Connect_StateIsSent()
    {
        var player = new Player(Guid.NewGuid(), "Filda");
        _persistenceService.Set(player);
        var sessionId = Guid.NewGuid();
        _persistenceService.Set(new Session(sessionId, [player], SessionState.Lobby));

        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={sessionId}");
        var socket = await webSocket.ConnectAsync(uri, CancellationToken.None);
        var buffer = new byte[1024 * 4];
        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        var socketMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var message = JsonSerializer.Deserialize<SessionDto>(socketMessage);
        message.Should().NotBeNull();
        message.State.Should().Be(SessionState.Lobby);
        message.Players.Should().HaveCount(1);
    }

    [Test]
    public async Task Connect_WhenPlayerIsSent_UpdatedStateIsSent()
    {
        var player = new Player(Guid.NewGuid(), "Filda");
        _persistenceService.Set(player);
        var sessionId = Guid.NewGuid();
        _persistenceService.Set(new Session(sessionId, [player], SessionState.Lobby));

        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={sessionId}");
        using var socket = await webSocket.ConnectAsync(uri, CancellationToken.None);
        var buffer = new byte[1024 * 4];

        // Ignore initial status.
        _ = await socket.ReceiveAsync(buffer, CancellationToken.None);

        var newPlayerDto = new NewPlayerDto("Jana");
        var json = JsonSerializer.Serialize(newPlayerDto);
        await socket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);

        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        var socketMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var message = JsonSerializer.Deserialize<SessionDto>(socketMessage);
        message.Should().NotBeNull();
        message.Players.Should().HaveCount(2);
        var newPlayer = message.Players.SingleOrDefault(p => p.Name == "Jana");
        var a = newPlayer.Should().NotBeNull();
        newPlayer.Id.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task Connect_WhenPlayerIsSent_AllConnectedClientsAreUpdated()
    {
        var player = new Player(Guid.NewGuid(), "Filda");
        _persistenceService.Set(player);
        var sessionId = Guid.NewGuid();
        _persistenceService.Set(new Session(sessionId, [player], SessionState.Lobby));

        var otherClient = Task.Run(async () =>
        {
            var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
            var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
            var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={sessionId}");
            using var socket = await webSocket.ConnectAsync(uri, CancellationToken.None);
            var buffer = new byte[1024 * 4];

            // Ignore initial status.
            await socket.ReceiveAsync(buffer, CancellationToken.None);

            var otherMessage = await socket.ReceiveAsync(buffer, CancellationToken.None);
            otherMessage.Should().NotBeNull();
        });

        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={sessionId}");
        using var socket = await webSocket.ConnectAsync(uri, CancellationToken.None);
        var buffer = new byte[1024 * 4];

        // Ignore initial status.
        _ = await socket.ReceiveAsync(buffer, CancellationToken.None);

        var newPlayerDto = new NewPlayerDto("Jana");
        var json = JsonSerializer.Serialize(newPlayerDto);
        await socket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);

        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        var socketMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var message = JsonSerializer.Deserialize<SessionDto>(socketMessage);
        message.Should().NotBeNull();
        message.Players.Should().HaveCount(2);
        var newPlayer = message.Players.SingleOrDefault(p => p.Name == "Jana");
        newPlayer.Should().NotBeNull();
        newPlayer.Id.Should().NotBe(Guid.Empty);

        var wait = async () => await otherClient.WaitAsync(5.Seconds());
        await wait.Should().NotThrowAsync();
    }

    [Test]
    public async Task Connect_WhenPlayerIsSent_AllConnectedOnlySubscribedToSessionIsUpdated()
    {
        var player = new Player(Guid.NewGuid(), "Filda");
        _persistenceService.Set(player);
        var sessionId = Guid.NewGuid();
        _persistenceService.Set(new Session(sessionId, [player], SessionState.Lobby));

        var unrelatedSessionId = Guid.NewGuid();
        _persistenceService.Set(new Session(unrelatedSessionId, [], SessionState.Lobby));

        var otherClient = Task.Run(async () =>
        {
            var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
            var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
            var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={unrelatedSessionId}");

            using var socket = new JsonWebSocket(await webSocket.ConnectAsync(uri, CancellationToken.None));

            // Ignore initial status.
            _ = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);

            var receiveResult = async () => await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(1.Seconds());
            await receiveResult.Should().ThrowAsync<TimeoutException>();
        });

        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={sessionId}");
        using var socket = new JsonWebSocket(await webSocket.ConnectAsync(uri, CancellationToken.None));

        // Ignore initial status.
        _ = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
        var newPlayerDto = new NewPlayerDto("Jana");
        await socket.SendAsync(newPlayerDto, CancellationToken.None);

        var message = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
        message.Should().NotBeNull();
        message.Players.Should().HaveCount(2);
        var newPlayer = message.Players.SingleOrDefault(p => p.Name == "Jana");
        newPlayer.Should().NotBeNull();
        newPlayer.Id.Should().NotBe(Guid.Empty);

        var wait = async () => await otherClient.WaitAsync(5.Seconds());
        await wait.Should().NotThrowAsync();
    }

    [Theory]
    public async Task Connect_WhenClientDisconnects_OtherClientStillReceivesUpdates(bool gracefulDisconnect)
    {
        var sessionId = Guid.NewGuid();
        var session = new Session(sessionId, [], SessionState.Lobby);
        _persistenceService.Set(session);

        var otherClient = Task.Run(async () =>
        {
            var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
            var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
            var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={sessionId}");

            using var socket = new JsonWebSocket(await webSocket.ConnectAsync(uri, CancellationToken.None));

            // Ignore initial status.
            _ = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
            var updateAfterClosingTheOther = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
            updateAfterClosingTheOther.Should().NotBeNull();
        });

        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?sessionId={sessionId}");
        using var socket = new JsonWebSocket(await webSocket.ConnectAsync(uri, CancellationToken.None));

        // Ignore initial status.
        _ = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
        if (gracefulDisconnect)
        {
            await socket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
        }
        else
        {
            socket.WebSocket.Abort();
        }

        _webSocketClientService.UpdateAll(session.ToDto());
        var wait = async () => await otherClient.WaitAsync(2.Seconds());
        await wait.Should().NotThrowAsync<TimeoutException>();
    }
}
