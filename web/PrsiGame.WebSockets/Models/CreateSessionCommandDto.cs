using System;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class CreateSessionCommandDto : PrsiCommandDto
    {
        public CreateSessionCommandDto(PlayerDto author) : base(PrsiCommandType.CreateSession)
        {
            Author = author;
        }

        public PlayerDto Author { get; }
    }
}
