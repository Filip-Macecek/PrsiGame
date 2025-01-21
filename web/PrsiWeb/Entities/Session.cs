namespace PrsiWeb.Entities;

public record Session(Guid Id, List<Player> Players, SessionState State);
