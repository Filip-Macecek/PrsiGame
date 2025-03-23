using System;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class ConnectToSessionDto
    {
        public ConnectToSessionDto(Guid playerId, Guid sessionId)
        {
            PlayerId = playerId;
            SessionId = sessionId;
        }

        public Guid PlayerId { get; }

        public Guid SessionId { get; }
    }
}
