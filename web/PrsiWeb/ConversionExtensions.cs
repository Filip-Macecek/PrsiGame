using PrsiWeb.Entities;
using PrsiWeb.Models;

namespace PrsiWeb;

public static class ConversionExtensions
{
    public static PlayerDto ToDto(this Player player)
    {
        return new PlayerDto(player.Id, player.Name);
    }

    public static SessionDto ToDto(this Session session)
    {
        return new SessionDto(session.Id, session.Players.Select(p => p.ToDto()), session.State);
    }
}
