using Microsoft.Extensions.Caching.Memory;
using PrsiWeb.Entities;

namespace PrsiWeb.Services;

public sealed class PersistenceService : IPersistenceService
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public void Set(Player player)
    {
        _cache.Set(player.Id, player);
    }

    public void Set(Session session)
    {
        _cache.Set(session.Id, session);
    }

    public Player? GetPlayer(Guid id)
    {
        return _cache.Get(id) as Player;
    }

    public Session? GetSession(Guid id)
    {
        return _cache.Get(id) as Session;
    }
}
