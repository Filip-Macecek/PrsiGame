using PrsiWeb.Entities;

namespace PrsiWeb.Models;

public record SessionDto(Guid Id, IEnumerable<PlayerDto> Players, SessionState State);
