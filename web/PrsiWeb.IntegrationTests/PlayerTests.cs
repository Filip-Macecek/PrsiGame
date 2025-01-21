using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PrsiWeb.Models;
using PrsiWeb.Services;

namespace PrsiWeb.IntegrationTests;

[TestFixture]
public class PlayerTests : TestBase
{
    [Test]
    public async Task AddPlayer_WritesToMemoryCache()
    {
        var dto = new NewPlayerDto(Name: "Filda");

        var client = WebApplicationFactory.CreateClient();
        var response = await client.PutAsync("player", new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Should().NotBeNull();
        var playerResponse = JsonConvert.DeserializeObject<PlayerDto>(await response.Content.ReadAsStringAsync());
        playerResponse.Id.Should().NotBe(Guid.Empty);
        playerResponse.Name.Should().Be("Filda");

        var persistence = WebApplicationFactory.Services.GetRequiredService<IPersistenceService>();
        var player = persistence.GetPlayer(playerResponse.Id);
        player.Should().NotBeNull();
        player.Name.Should().Be("Filda");
        player.Id.Should().Be(playerResponse.Id);
    }
}
