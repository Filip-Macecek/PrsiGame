using System;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class StartGameDto : PrsiCommandDto
    {
        public StartGameDto() : base(PrsiCommandType.StartGame)
        {
        }
    }
}
