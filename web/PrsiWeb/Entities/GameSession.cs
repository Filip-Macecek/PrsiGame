using PrsiGame.Types;

namespace PrsiWeb.Entities;

public record GameSession(Guid Id, IReadOnlyList<Player> Players, SessionState State, Player Author, Game? Game);
