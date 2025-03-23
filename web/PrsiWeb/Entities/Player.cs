namespace PrsiWeb.Entities;

public sealed record Player(Guid Id, string Name, PrsiGame.Types.Player? PrsiPlayer);
