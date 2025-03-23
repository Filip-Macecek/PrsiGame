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
        return new PlayerDto(player.Id, player.Name, player.PrsiPlayer?.CardsOnHand);
    }

    public static SessionDto ToDto(this GameSession gameSession)
    {
        return new SessionDto(gameSession.Id, gameSession.Players.Select(p => p.ToDto()), (SessionStateDto)gameSession.State, gameSession.Game?.ToDto());
    }

    public static GameDto ToDto(this Game game)
    {
        return new GameDto(game.Turns.Select(t => t.ToDto()), game.LickPile, game.DiscardPile);
    }

    public static TurnDto ToDto(this Turn turn)
    {
        // TODO
        return new TurnDto();
    }

    public static CreateSessionCommand ToCommand(this CreateSessionCommandDto commandDto, JsonWebSocket socket)
    {
        return new CreateSessionCommand(socket, commandDto.Author);
    }

    public static JoinLobbyCommand ToCommand(this JoinLobbyCommandDto commandDto, JsonWebSocket socket)
    {
        return new JoinLobbyCommand(socket, commandDto.SessionId, new Player(commandDto.Player.Id, commandDto.Player.Name, PrsiPlayer: null));
    }

    public static StartGameCommand ToCommand(this StartGameDto commandDto, JsonWebSocket socket)
    {
        return new StartGameCommand(socket);
    }
}
