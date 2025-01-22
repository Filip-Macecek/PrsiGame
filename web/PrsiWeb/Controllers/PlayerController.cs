using Microsoft.AspNetCore.Mvc;
using PrsiWeb.Entities;
using PrsiWeb.Models;
using PrsiWeb.Services;

namespace PrsiWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController : ControllerBase
{
    private readonly ILogger<PlayerController> _logger;
    private readonly IPersistenceService _persistenceService;

    public PlayerController(ILogger<PlayerController> logger, IPersistenceService persistenceService)
    {
        _logger = logger;
        _persistenceService = persistenceService;
    }

    [HttpPut()]
    public PlayerDto Put([FromBody] ConnectPlayerDto connectPlayerDto)
    {
        var player = new Player(Guid.NewGuid(), connectPlayerDto.Name ?? "New Player");
        _persistenceService.Set(player);
        return new PlayerDto(player.Id, player.Name);
    }
}
