namespace PrsiGame.WebSockets.Models
{
    public class HealthcheckCommandDto : PrsiCommandDto
    {
        public HealthcheckCommandDto() : base(PrsiCommandType.Healthcheck)
        {
        }
    }
}
