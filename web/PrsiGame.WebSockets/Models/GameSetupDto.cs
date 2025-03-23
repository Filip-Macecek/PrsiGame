using System;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class GameSetupDto
    {
        public GameSetupDto(ushort playerCount, ushort playerCardCount)
        {
            PlayerCount = playerCount;
            PlayerCardCount = playerCardCount;
        }

        public ushort PlayerCount { get; }

        public ushort PlayerCardCount { get; }
    }
}
