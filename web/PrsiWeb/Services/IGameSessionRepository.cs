using PrsiWeb.Entities;

namespace PrsiWeb.Services;

public interface IGameSessionRepository
{
    public GameSession? Get(Guid sessionId);

    public GameSession CreateNew(Player author);

    public GameSession? AddPlayer(Guid sessionId, Player player);

    public GameSession RemovePlayer(Guid sessionId, Guid playerId);

    public GameSession Start(Guid sessionId);

    public GameSession End(Guid sessionId);
}
