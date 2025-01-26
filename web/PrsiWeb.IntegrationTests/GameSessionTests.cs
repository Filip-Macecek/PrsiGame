using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PrsiGame.WebSockets;
using PrsiWeb.Entities;
using PrsiWeb.Models;
using PrsiWeb.Services;

namespace PrsiWeb.IntegrationTests;

[TestFixture]
public class GameSessionTests : TestBase
{
    private IGameSessionRepository _gameSessionRepository;
    private WebSocketClientService _webSocketClientService;

    [SetUp]
    public void Setup()
    {
        _gameSessionRepository = WebApplicationFactory.Services.GetRequiredService<IGameSessionRepository>();
        _webSocketClientService = WebApplicationFactory.Services.GetRequiredService<WebSocketClientService>();
    }

    [Test]
    public async Task Connect_WhenNotWebSocket_Fails()
    {
        var player = new Player(Guid.NewGuid(), "Filda");
        var session = _gameSessionRepository.CreateNew(player);
        var sessionId = session.Id;

        var newPlayer = new Player(Guid.NewGuid(), "Jana");
        var dto = new ConnectToSessionDto(newPlayer.Id, sessionId);

        var client = WebApplicationFactory.CreateClient();
        var response = await client.PostAsync("session/connect", new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Test]
    public async Task Connect_WhenCreateSessionCommandIsSent_NewSessionIsRetrieved()
    {
        var player = new Player(Guid.NewGuid(), "Filda");

        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
        using var socket = new JsonWebSocket("", await webSocket.ConnectAsync(uri, CancellationToken.None));

        await socket.SendAsync(new CreateSessionCommandDto(player.ToDto()), default);
        var message = await socket.ReceiveAsync<SessionDto>(default);
        message.Should().NotBeNull();
        message.State.Should().Be(SessionState.Lobby);
        message.Players.Should().HaveCount(1);
    }

    [Test]
    public async Task Connect_WhenJoinLobby_StateIsSent()
    {
        var session = _gameSessionRepository.CreateNew(new Player(Guid.NewGuid(), "Author"));

        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
        using var socket = new JsonWebSocket("", await webSocket.ConnectAsync(uri, CancellationToken.None));

        var newPlayer = new Player(Guid.NewGuid(), "Filda");
        await socket.SendAsync(new JoinLobbyCommandDto(session.Id, newPlayer.ToDto()), default);
        var message = await socket.ReceiveAsync<SessionDto>(default);
        message.Should().NotBeNull();
        message.State.Should().Be(SessionState.Lobby);
        message.Players.Should().HaveCount(2);
    }

    [Test]
    public async Task Connect_WhenNewPlayerJoin_AllConnectedClientsAreUpdated()
    {
        var clientWebSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?id=join_lobby_thread");

        var author = new Player(Guid.NewGuid(), "Filda");
        using var socket = new JsonWebSocket("Join lobby thread", await clientWebSocket.ConnectAsync(uri, CancellationToken.None));

        await socket.SendAsync(new CreateSessionCommandDto(author.ToDto()), CancellationToken.None);
        var newSession = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);

        var joinLobbyTask = Task.Run(async () =>
        {
            using var newPlayerSocket = new JsonWebSocket("Join lobby thread", await clientWebSocket.ConnectAsync(uri, CancellationToken.None));
            var newPlayer = new Player(Guid.NewGuid(), "Janca");
            await newPlayerSocket.SendAsync(new JoinLobbyCommandDto(newSession!.Id, newPlayer.ToDto()), CancellationToken.None);
        });

        var joinLobbyTaskWait = async () => await joinLobbyTask.WaitAsync(3.Seconds());
        await joinLobbyTaskWait.Should().NotThrowAsync<TimeoutException>();

        var receiveUpdatedSessionWait = async () =>
        {
            var updatedSession = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds());
            updatedSession.Should().NotBeNull();
            updatedSession.Players.Should().HaveCount(2);
            updatedSession.Players.Should().ContainSingle(p => p.Name == "Filda");
            updatedSession.Players.Should().ContainSingle(p => p.Name == "Janca");
        };
        await receiveUpdatedSessionWait.Should().NotThrowAsync<TimeoutException>();
    }

    [Test]
    public async Task Connect_WhenPlayerDisconnects_AuthorIsUpdated()
    {
        var clientWebSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?id=join_lobby_thread");

        var author = new Player(Guid.NewGuid(), "Filda");
        using var socket = new JsonWebSocket("Join lobby thread", await clientWebSocket.ConnectAsync(uri, CancellationToken.None));

        await socket.SendAsync(new CreateSessionCommandDto(author.ToDto()), CancellationToken.None);
        var newSession = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);

        var joinLobbyTask = Task.Run(async () =>
        {
            using var newPlayerSocket = new JsonWebSocket("Join lobby thread", await clientWebSocket.ConnectAsync(uri, CancellationToken.None));
            var newPlayer = new Player(Guid.NewGuid(), "Janca");
            await newPlayerSocket.SendAsync(new JoinLobbyCommandDto(newSession!.Id, newPlayer.ToDto()), CancellationToken.None);
            await newPlayerSocket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
        });

        var joinLobbyTaskWait = async () => await joinLobbyTask.WaitAsync(3.Seconds());
        await joinLobbyTaskWait.Should().NotThrowAsync<TimeoutException>();

        var receiveUpdatedSessionWait = async () =>
        {
            var sessionUpdatedAfterJoin = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds());
            sessionUpdatedAfterJoin.Should().NotBeNull();
            sessionUpdatedAfterJoin.Players.Should().HaveCount(2);
            sessionUpdatedAfterJoin.Players.Should().ContainSingle(p => p.Name == "Filda");
            sessionUpdatedAfterJoin.Players.Should().ContainSingle(p => p.Name == "Janca");

            var sessionAfterDisconnect = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds());
            sessionAfterDisconnect.Should().NotBeNull();
            sessionAfterDisconnect.Players.Should().HaveCount(1);
            sessionAfterDisconnect.Players.Should().ContainSingle(p => p.Name == "Filda");
        };
        await receiveUpdatedSessionWait.Should().NotThrowAsync<TimeoutException>();
    }

    // [Test]
    // public async Task Connect_WhenClientDisconnectsGracefullyAndNotAuthor_OtherClientStillReceivesUpdates()
    // {
    //     var author = new Player(Guid.NewGuid(), "Filda");
    //     var session = _gameSessionRepository.CreateNew(author);
    //
    //     var otherClient = Task.Run(async () =>
    //     {
    //         var newPlayer = new Player(Guid.NewGuid(), "Janca");
    //         var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
    //         var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
    //         var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
    //
    //         using var socket = new JsonWebSocket("", await webSocket.ConnectAsync(uri, CancellationToken.None));
    //         _webSocketClientService.Add(session, new WebSocketClient(Guid.NewGuid(), socket, newPlayer.Id));
    //         _gameSessionRepository.AddPlayer(session.Id, newPlayer);
    //
    //         var sessionAfterClosingTheOther = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
    //         sessionAfterClosingTheOther.Should().NotBeNull();
    //         sessionAfterClosingTheOther.Players.Should().HaveCount(1);
    //         sessionAfterClosingTheOther.Players.Should().ContainSingle(p => p.Id == newPlayer.Id);
    //     });
    //
    //     var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
    //     var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
    //     var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
    //     using var socket = new JsonWebSocket("", await webSocket.ConnectAsync(uri, CancellationToken.None));
    //     _webSocketClientService.Add(session, new WebSocketClient(session.Id, socket, author.Id));
    //
    //     await socket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
    //
    //     var wait = async () => await otherClient.WaitAsync(2.Seconds());
    //     await wait.Should().NotThrowAsync<TimeoutException>();
    // }

    // [Theory]
    // public async Task Connect_WhenClientDisconnects_OtherClientStillReceivesUpdates(bool gracefulDisconnect)
    // {
    //     var player = new Player(Guid.NewGuid(), "Filda");
    //     var session = _gameSessionRepository.CreateNew(player);
    //     var sessionId = session.Id;
    //
    //     var otherClient = Task.Run(async () =>
    //     {
    //         var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
    //         var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
    //         var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
    //
    //         using var socket = new JsonWebSocket(await webSocket.ConnectAsync(uri, CancellationToken.None));
    //         await socket.SendAsync(new JoinLobbyCommandDto(session.Id, new PlayerDto(Guid.NewGuid(), "Janca")), CancellationToken.None);
    //         _ = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
    //         var updateAfterClosingTheOther = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
    //         updateAfterClosingTheOther.Should().NotBeNull();
    //     });
    //
    //     var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
    //     var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
    //     var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
    //     using var socket = new JsonWebSocket(await webSocket.ConnectAsync(uri, CancellationToken.None));
    //
    //     // Ignore initial status.
    //     _ = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
    //     if (gracefulDisconnect)
    //     {
    //         await socket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
    //     }
    //     else
    //     {
    //         socket.WebSocket.Abort();
    //     }
    //
    //     _webSocketClientService.UpdateAll(session.ToDto());
    //     var wait = async () => await otherClient.WaitAsync(2.Seconds());
    //     await wait.Should().NotThrowAsync<TimeoutException>();
    // }
}
