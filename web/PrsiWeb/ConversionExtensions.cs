using PrsiGame.Types;
using PrsiGame.WebSockets;
using PrsiWeb.Commands;
using PrsiWeb.Entities;
using PrsiGame.WebSockets.Models;
using Player = PrsiWeb.Entities.Player;

namespace PrsiWeb;

public static class ConversionExtensions
{
    public static PlayerDto ToDto(this Player player)
    {
        return new PlayerDto(player.Id, player.Name);
    }

    public static SessionDto ToDto(this GameSession gameSession)
    {
        return new SessionDto(gameSession.Id, gameSession.Players.Select(p => p.ToDto()), (SessionStateDto)gameSession.State);
    }

    public static CreateSessionCommand ToCommand(this CreateSessionCommandDto commandDto, JsonWebSocket socket)
    {
        return new CreateSessionCommand(socket, commandDto.Author);
    }

    public static JoinLobbyCommand ToCommand(this JoinLobbyCommandDto commandDto, JsonWebSocket socket)
    {
        return new JoinLobbyCommand(socket, commandDto.SessionId, new Player(commandDto.Player.Id, commandDto.Player.Name));
    }

    public static StartGameCommand ToCommand(this StartGameDto commandDto, JsonWebSocket socket)
    {
        return new StartGameCommand(socket);
    }
}
