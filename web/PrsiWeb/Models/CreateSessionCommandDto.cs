namespace PrsiWeb.Models;

public record CreateSessionCommandDto(PlayerDto Author) : PrsiCommandDto(PrsiCommandType.CreateSession);
