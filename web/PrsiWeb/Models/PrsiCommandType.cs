namespace PrsiWeb.Models;

public enum PrsiCommandType
{
    CreateSession = 1,
    JoinLobby = 2,
    StartGame = 3,
    AddTurn = 4,
    Disconnect = 5,
    Healthcheck = 6
}
