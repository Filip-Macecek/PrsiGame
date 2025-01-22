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

    public void Set(GameSession gameSession)
    {
        _cache.Set(gameSession.Id, gameSession);
    }

    public Player? GetPlayer(Guid id)
    {
        return _cache.Get(id) as Player;
    }

    public GameSession? GetSession(Guid id)
    {
        return _cache.Get(id) as GameSession;
    }
}
