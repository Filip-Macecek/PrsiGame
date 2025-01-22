using PrsiWeb.Entities;

namespace PrsiWeb.Services;

public interface IPersistenceService
{
    public void Set(Player player);

    public void Set(GameSession gameSession);

    Player? GetPlayer(Guid id);

    GameSession? GetSession(Guid id);
}
