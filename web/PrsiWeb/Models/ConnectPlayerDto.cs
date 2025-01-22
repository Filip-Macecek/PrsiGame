namespace PrsiWeb.Models;

public record ConnectPlayerDto(
    Guid? PlayerId,
    string? Name
);
