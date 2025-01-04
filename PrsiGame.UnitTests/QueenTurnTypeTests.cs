using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class QueenTurnTypeTests
{
    private QueenTurn GetTurn()
    {
        return QueenTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), QueenCard.Create(CardId.QueenOfHearts).Value, CardColor.Hearts);
    }

    [Theory]
    [InlineData(CardId.EightOfHearts, true)]
    [InlineData(CardId.EightOfDiamonds, true)]
    public void Validate_WhenRegularCardWasPlayed(CardId cardId, bool shouldPass)
    {
        var regularCard = RegularCard.Create(cardId).Value;
        var turn = GetTurn();
        var validationResult = turn.Validate(regularCard, applies: false);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.AceOfHearts, false, true)]
    [InlineData(CardId.AceOfDiamonds, false, true)]
    [InlineData(CardId.AceOfHearts, true, false)]
    [InlineData(CardId.AceOfDiamonds, true, false)]
    public void Validate_WhenAceCardWasPlayed(CardId cardId, bool applies, bool shouldPass)
    {
        var aceCard = AceCard.Create(cardId).Value;
        var turn = GetTurn();
        var validationResult = turn.Validate(aceCard, applies);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.SevenOfHearts, false, true)]
    [InlineData(CardId.SevenOfDiamonds, false, true)]
    [InlineData(CardId.SevenOfHearts, true, false)]
    [InlineData(CardId.SevenOfDiamonds, true, false)]
    public void Validate_WhenSevenWasPlayed(CardId cardId, bool applies, bool shouldPass)
    {
        var sevenCard = SevenCard.Create(cardId).Value;
        var turn = GetTurn();
        var validationResult = turn.Validate(sevenCard, applies);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.QueenOfHearts, true)]
    [InlineData(CardId.QueenOfDiamonds, true)]
    public void Validate_WhenQueenIsPlayed(CardId cardId, bool shouldPass)
    {
        var aceCard = QueenCard.Create(cardId).Value;
        var turn = GetTurn();
        var validationResult = turn.Validate(aceCard, applies: false);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }
}
