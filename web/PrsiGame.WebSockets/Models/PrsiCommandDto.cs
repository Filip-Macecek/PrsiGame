namespace PrsiGame.WebSockets.Models
{
    public abstract class PrsiCommandDto
    {
        protected PrsiCommandDto(PrsiCommandType prsiCommandType)
        {
            PrsiCommandType = prsiCommandType;
        }

        public PrsiCommandType PrsiCommandType { get; }
    }
}
