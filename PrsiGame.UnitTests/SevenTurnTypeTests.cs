using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class SevenTurnTypeTests
{
    [Theory]
    [InlineData(CardId.EightOfHearts, true)]
    [InlineData(CardId.EightOfDiamonds, false)]
    public void Validate_WhenRegularCardWasPlayed(CardId cardId, bool shouldPass)
    {
        var turn = SevenTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), SevenCard.Create(CardId.SevenOfHearts).Value);
        var regularCard = RegularCard.Create(cardId).Value;
        var validationResult = turn.Validate(regularCard, colorOverride: null, specialCardApplies: false);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.AceOfHearts, false, true)]
    [InlineData(CardId.AceOfDiamonds, false, false)]
    [InlineData(CardId.AceOfHearts, true, false)]
    [InlineData(CardId.AceOfDiamonds, true, false)]
    public void Validate_WhenAceCardWasPlayed(CardId cardId, bool applies, bool shouldPass)
    {
        var turn = SevenTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), SevenCard.Create(CardId.SevenOfHearts).Value);
        var aceCard = AceCard.Create(cardId).Value;
        var validationResult = turn.Validate(aceCard, null, applies);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.SevenOfHearts, false, true)]
    [InlineData(CardId.SevenOfDiamonds, false, true)]
    [InlineData(CardId.SevenOfHearts, true, true)]
    [InlineData(CardId.SevenOfDiamonds, true, true)]
    public void Validate_WhenSevenWasPlayed(CardId cardId, bool applies, bool shouldPass)
    {
        var turn = SevenTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), SevenCard.Create(CardId.SevenOfHearts).Value);
        var sevenCard = SevenCard.Create(cardId).Value;
        var validationResult = turn.Validate(sevenCard, null, applies);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.QueenOfHearts, CardColor.Hearts, true)]
    [InlineData(CardId.QueenOfHearts, CardColor.Diamonds, false)]
    [InlineData(CardId.QueenOfDiamonds, CardColor.Diamonds, false)]
    public void Validate_WhenQueenIsPlayed(CardId cardId, CardColor colorOverride, bool shouldPass)
    {
        var turn = SevenTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), SevenCard.Create(CardId.SevenOfHearts).Value);
        var aceCard = QueenCard.Create(cardId).Value;
        var validationResult = turn.Validate(aceCard, colorOverride, specialCardApplies: false);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }
}
