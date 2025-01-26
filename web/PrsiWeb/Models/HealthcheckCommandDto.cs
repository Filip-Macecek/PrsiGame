namespace PrsiWeb.Models;

public record HealthcheckCommandDto() : PrsiCommandDto(PrsiCommandType.Healthcheck);
