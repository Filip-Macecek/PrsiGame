using System;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class JoinLobbyCommandDto : PrsiCommandDto
    {
        public JoinLobbyCommandDto(Guid sessionId, PlayerDto player) : base(PrsiCommandType.JoinLobby)
        {
            SessionId = sessionId;
            Player = player;
        }

        public Guid SessionId { get; }

        public PlayerDto Player { get; }
    }
}
