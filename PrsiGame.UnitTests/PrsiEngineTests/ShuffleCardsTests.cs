using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests.PrsiEngineTests;

public class ShuffleCardsTests
{
    [Fact]
    public void ShuffleCards_HaveCorrectCardCount()
    {
        var shuffledCards = PrsiEngine.ShuffleCards();

        shuffledCards.Should().NotBeEmpty();
        shuffledCards.Should().HaveCount(32);
    }

    [Fact]
    public void ShuffleCards_EveryCardIsDifferent()
    {
        var shuffledCards = PrsiEngine.ShuffleCards();
        var visitedCards = new HashSet<CardId>();

        foreach (var card in shuffledCards)
        {
            visitedCards.Should().NotContain(card);
            visitedCards.Add(card);
        }
    }

    [Fact]
    public void ShuffleCards_OrderIsRandomize()
    {
        var deckA = PrsiEngine.ShuffleCards().ToList();
        var deckB = PrsiEngine.ShuffleCards().ToList();
        var hasDifference = false;

        for (var i = 0; i < 32; i++)
        {
            hasDifference |= deckA[i] != deckB[i];
        }

        hasDifference.Should().BeTrue();
    }
}