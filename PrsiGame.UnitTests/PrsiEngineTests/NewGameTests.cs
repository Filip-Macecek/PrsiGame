using FluentAssertions;
using PrsiGame.Common;
using PrsiGame.Types;

namespace PrsiGame.UnitTests.PrsiEngineTests;

public class NewGameTests
{
    [Theory]
    [InlineData(2, 5)]
    [InlineData(3, 7)]
    [InlineData(4, 4)]
    public void NewGame_ReturnsValidGame(ushort playerCount, ushort playerCardCount)
    {
        var deck = PrsiEngine.ShuffleCards();
        var game = PrsiEngine.NewGame(deck, new GameSetup(playerCount, playerCardCount));

        game.Players.Should().HaveCount(playerCount);
        game.Players.Should().AllSatisfy(p =>
        {
            p.CardsOnHand.Should().HaveCount(playerCardCount);
            p.State.Should().Be(p.PlayerOrder == 0 ? PlayerState.OnTurn : PlayerState.Waiting);
        });

        game.State.Should().Be(GameState.Started);
        game.Turns.Should().BeEmpty();
        game.LickPile.Should().HaveCount(32 - (playerCount * playerCardCount + 1));
        game.DiscardPile.Should().ContainSingle();
    }
}
