using PrsiWeb.Entities;

namespace PrsiWeb.Services;

public interface IPersistenceService
{
    public void Set(Player player);

    public void Set(Session session);

    Player? GetPlayer(Guid id);

    Session? GetSession(Guid id);
}
