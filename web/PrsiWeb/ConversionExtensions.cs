using PrsiWeb.Entities;
using PrsiWeb.Models;

namespace PrsiWeb;

public static class ConversionExtensions
{
    public static PlayerDto ToDto(this Player player)
    {
        return new PlayerDto(player.Id, player.Name);
    }

    public static SessionDto ToDto(this GameSession gameSession)
    {
        return new SessionDto(gameSession.Id, gameSession.Players.Select(p => p.ToDto()), gameSession.State);
    }
}
