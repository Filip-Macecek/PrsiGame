using Microsoft.Extensions.Caching.Memory;
using PrsiWeb.Entities;

namespace PrsiWeb.Services;

public class InMemoryGameSessionRepository : IGameSessionRepository
{
    private readonly IMemoryCache _cache;

    public InMemoryGameSessionRepository(IMemoryCache cache)
    {
        _cache = cache;
    }

    public GameSession? Get(Guid sessionId)
    {
        return _cache.Get<GameSession>(sessionId);
    }

    public GameSession CreateNew(Player author)
    {
        var session = new GameSession(Guid.NewGuid(), [author], SessionState.Lobby, author);
        return _cache.Set(session.Id, session);
    }

    public GameSession AddPlayer(Guid sessionId, Player player)
    {
        lock (_cache)
        {
            var session = _cache.Get<GameSession>(sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session with ID {sessionId} not found.");
            }

            var newSession = session with
            {
                Players = [..session.Players, player]
            };
            _cache.Set(session.Id, newSession);
            return newSession;
        }
    }

    public GameSession RemovePlayer(Guid sessionId, Player player)
    {
        lock (_cache)
        {
            var session = _cache.Get<GameSession>(sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session with ID {sessionId} not found.");
            }

            var newSession = session with
            {
                // TODO: Will the except work?
                Players = [..session.Players.Except([player]).ToList()]
            };
            _cache.Set(session.Id, newSession);
            return newSession;
        }
    }

    public GameSession Start(Guid sessionId)
    {
        lock (_cache)
        {
            var session = _cache.Get<GameSession>(sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session with ID {sessionId} not found.");
            }

            var newSession = session with
            {
                State = SessionState.InGame
            };
            _cache.Set(session.Id, newSession);
            return newSession;
        }
    }

    public GameSession End(Guid sessionId)
    {
        lock (_cache)
        {
            var session = _cache.Get<GameSession>(sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session with ID {sessionId} not found.");
            }

            var newSession = session with
            {
                State = SessionState.Ended
            };
            _cache.Set(session.Id, newSession);
            return newSession;
        }
    }
}
