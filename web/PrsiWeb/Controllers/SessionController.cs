using Microsoft.AspNetCore.Mvc;

namespace PrsiWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ILogger<SessionController> _logger;

    public SessionController(ILogger<SessionController> logger)
    {
        _logger = logger;
    }

    [HttpPut(Name = "GetWeatherForecast")]
    public async Task Put()
    {

    }
}
