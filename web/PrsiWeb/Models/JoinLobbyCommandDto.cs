namespace PrsiWeb.Models;

public record JoinLobbyCommandDto(Guid SessionId, PlayerDto Player) : PrsiCommandDto(PrsiCommandType.JoinLobby);
