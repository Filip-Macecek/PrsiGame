using System;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class HealthcheckCommandDto : PrsiCommandDto
    {
        public HealthcheckCommandDto() : base(PrsiCommandType.Healthcheck)
        {
        }
    }
}
