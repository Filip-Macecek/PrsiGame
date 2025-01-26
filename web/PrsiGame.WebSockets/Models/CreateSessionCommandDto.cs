namespace PrsiGame.WebSockets.Models
{
    public class CreateSessionCommandDto : PrsiCommandDto
    {
        public CreateSessionCommandDto(PlayerDto author) : base(PrsiCommandType.CreateSession)
        {
            Author = author;
        }

        public PlayerDto Author { get; }
    }
}
