using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PrsiGame.WebSockets;
using PrsiWeb.Entities;
using PrsiGame.WebSockets.Models;
using PrsiWeb.Services;

namespace PrsiWeb.IntegrationTests;

[TestFixture]
public class GameSessionTests : TestBase
{
    private IGameSessionRepository _gameSessionRepository;

    [SetUp]
    public void Setup()
    {
        _gameSessionRepository = WebApplicationFactory.Services.GetRequiredService<IGameSessionRepository>();
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
    public async Task Connect_WhenHealthcheckNotSent_SocketIsClosed()
    {
        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
        using var socket = new JsonWebSocket("", await webSocket.ConnectAsync(uri, CancellationToken.None));
        var receiveTask = async () =>
        {
            var receiveResult = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(5.Seconds()); // assuming the timeout for healthcheck is 3 seconds.
            receiveResult.IsFailed.Should().BeTrue();
            receiveResult.Errors.First().Should().BeOfType(typeof(WebSocketClosedError));
        };
        await receiveTask.Should().NotThrowAsync<TimeoutException>();
    }

    [Test]
    public async Task Connect_WhenHealthcheckIsSent_SocketDoesNotClose()
    {
        var webSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect");
        using var socket = new JsonWebSocket("", await webSocket.ConnectAsync(uri, CancellationToken.None));
        var receiveTask = async () =>
        {
            Thread.Sleep(2.Seconds());
            await socket.SendAsync(new HealthcheckCommandDto(), CancellationToken.None);
            Thread.Sleep(2.Seconds());
            await socket.SendAsync(new HealthcheckCommandDto(), CancellationToken.None);
            var receiveResult = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(5.Seconds()); // assuming the timeout for healthcheck is 3 seconds.
            receiveResult.IsFailed.Should().BeTrue();
            receiveResult.Errors.First().Should().BeOfType(typeof(WebSocketClosedError));
        };
        await receiveTask.Should().NotThrowAsync<TimeoutException>();
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
        var messageResult = await socket.ReceiveAsync<SessionDto>(default);
        messageResult.IsFailed.Should().BeFalse();
        messageResult.Value!.State.Should().Be(SessionStateDto.Lobby);
        messageResult.Value.Players.Should().HaveCount(1);
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
        var messageResult = await socket.ReceiveAsync<SessionDto>(default);
        messageResult.Value!.Should().NotBeNull();
        messageResult.Value.State.Should().Be(SessionStateDto.Lobby);
        messageResult.Value.Players.Should().HaveCount(2);
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
        var newSession = (await socket.ReceiveAsync<SessionDto>(CancellationToken.None)).Value;

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
            updatedSession.Value.Should().NotBeNull();
            updatedSession.Value.Players.Should().HaveCount(2);
            updatedSession.Value.Players.Should().ContainSingle(p => p.Name == "Filda");
            updatedSession.Value.Players.Should().ContainSingle(p => p.Name == "Janca");
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
        var newSession = (await socket.ReceiveAsync<SessionDto>(CancellationToken.None)).Value;

        var joinLobbyAndDisconnectTask = Task.Run(async () =>
        {
            using var newPlayerSocket = new JsonWebSocket("Join lobby thread", await clientWebSocket.ConnectAsync(uri, CancellationToken.None));
            var newPlayer = new Player(Guid.NewGuid(), "Janca");
            await newPlayerSocket.SendAsync(new JoinLobbyCommandDto(newSession!.Id, newPlayer.ToDto()), CancellationToken.None);

            await newPlayerSocket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure",
                CancellationToken.None);
        });

        var joinLobbyTaskWait = async () => await joinLobbyAndDisconnectTask.WaitAsync(3.Seconds());
        await joinLobbyTaskWait.Should().NotThrowAsync<TimeoutException>();

        var receiveUpdatedSessionWait = async () =>
        {
            var sessionUpdatedAfterJoin = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds());
            sessionUpdatedAfterJoin.Value.Should().NotBeNull();
            sessionUpdatedAfterJoin.Value.Players.Should().HaveCount(2);
            sessionUpdatedAfterJoin.Value.Players.Should().ContainSingle(p => p.Name == "Filda");
            sessionUpdatedAfterJoin.Value.Players.Should().ContainSingle(p => p.Name == "Janca");

            var sessionAfterDisconnect = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds());
            sessionAfterDisconnect.Value.Should().NotBeNull();
            sessionAfterDisconnect.Value.Players.Should().HaveCount(1);
            sessionAfterDisconnect.Value.Players.Should().ContainSingle(p => p.Name == "Filda");
        };
        await receiveUpdatedSessionWait.Should().NotThrowAsync<TimeoutException>();
    }

    [Test]
    public async Task Connect_WhenGameIsStarted_AllConnectedClientsAreUpdatedWithCorrectSessionStatus()
    {
        var clientWebSocket = WebApplicationFactory.Server.CreateWebSocketClient();
        var serverBaseAddress = WebApplicationFactory.Server.BaseAddress;
        var uri = new Uri($"ws://{serverBaseAddress.Host}/session/connect?id=join_lobby_thread");

        var author = new Player(Guid.NewGuid(), "Filda");
        using var socket = new JsonWebSocket("Author thread", await clientWebSocket.ConnectAsync(uri, CancellationToken.None));

        await socket.SendAsync(new CreateSessionCommandDto(author.ToDto()), CancellationToken.None);
        var newSession = (await socket.ReceiveAsync<SessionDto>(CancellationToken.None)).Value;

        var otherClient = Task.Run(async () =>
        {
            using var newPlayerSocket = new JsonWebSocket("Second player thread", await clientWebSocket.ConnectAsync(uri, CancellationToken.None));
            var newPlayer = new Player(Guid.NewGuid(), "Janca");
            await newPlayerSocket.SendAsync(new JoinLobbyCommandDto(newSession!.Id, newPlayer.ToDto()), CancellationToken.None);
            _ = await newPlayerSocket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds()); // discard first update
            await newPlayerSocket.SendAsync(new StartGameDto(), CancellationToken.None);
            var startedSession = await newPlayerSocket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds());
            startedSession.IsSuccess.Should().BeTrue();
            startedSession.Value.Should().NotBeNull();
            startedSession.Value.Players.Count().Should().Be(2);
            startedSession.Value.State.Should().Be(SessionStateDto.InGame);
        });

        await otherClient.WaitAsync(3.Seconds());

        _ = await socket.ReceiveAsync<SessionDto>(CancellationToken.None).WaitAsync(3.Seconds()); // discard first update
        var startedSession = await socket.ReceiveAsync<SessionDto>(CancellationToken.None);
        startedSession.IsSuccess.Should().BeTrue();
        startedSession.Value.Should().NotBeNull();
        startedSession.Value.Players.Count().Should().Be(2);
        startedSession.Value.State.Should().Be(SessionStateDto.InGame);
    }
}
