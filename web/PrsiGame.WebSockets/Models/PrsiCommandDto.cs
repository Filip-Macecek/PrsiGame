using System;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public abstract class PrsiCommandDto
    {
        protected PrsiCommandDto(PrsiCommandType prsiCommandType)
        {
            PrsiCommandType = prsiCommandType;
        }

        public PrsiCommandType PrsiCommandType { get; }
    }
}
