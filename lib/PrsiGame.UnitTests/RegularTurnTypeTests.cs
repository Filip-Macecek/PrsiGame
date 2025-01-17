using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class RegularTurnTypeTests
{
    private RegularTurn GetTurn()
    {
        var regularCard = RegularCard.Create(CardId.EightOfHearts).Value;
        return RegularTurn.Create(new Player(CardsOnHand: [regularCard.Id]), regularCard);
    }

    [Theory]
    [InlineData(CardId.EightOfClubs)]
    [InlineData(CardId.JackOfHearts)]
    public void Validate_WhenRegularCardWasPlayed_SucceedsIfColorOrValueMatches(CardId cardId)
    {
        var turn = GetTurn();
        var validationResult = turn.Validate(RegularCard.Create(cardId).Value, null, false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(CardId.NineOfClubs)]
    [InlineData(CardId.JackOfDiamonds)]
    public void Validate_WhenRegularCardWasPlayed_FailsIfColorOrValueMismatches(CardId cardId)
    {
        var turn = GetTurn();
        var validationResult = turn.Validate(RegularCard.Create(cardId).Value, null, false);

        validationResult.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(CardId.AceOfHearts, false, true)]
    [InlineData(CardId.AceOfDiamonds, false, false)]
    [InlineData(CardId.AceOfHearts, true, false)]
    [InlineData(CardId.AceOfDiamonds, true, false)]
    public void Validate_WhenAceCardWasPlayed(CardId cardId, bool applies, bool shouldPass)
    {
        var turn = GetTurn();
        var aceCard = AceCard.Create(cardId).Value;
        var validationResult = turn.Validate(aceCard, null, applies);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.SevenOfHearts, false, true)]
    [InlineData(CardId.SevenOfDiamonds, false, false)]
    [InlineData(CardId.SevenOfHearts, true, false)]
    [InlineData(CardId.SevenOfDiamonds, true, false)]
    public void Validate_WhenSevenWasPlayed(CardId cardId, bool applies, bool shouldPass)
    {
        var turn = GetTurn();
        var aceCard = SevenCard.Create(cardId).Value;
        var validationResult = turn.Validate(aceCard, null, applies);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }

    [Theory]
    [InlineData(CardId.QueenOfHearts, false, CardColor.Hearts, true)]
    [InlineData(CardId.QueenOfHearts, false, CardColor.Diamonds, false)]
    [InlineData(CardId.QueenOfDiamonds, false, CardColor.Diamonds, false)]
    [InlineData(CardId.QueenOfHearts, true, CardColor.Hearts, true)]
    [InlineData(CardId.QueenOfDiamonds, true, CardColor.Diamonds, false)]
    public void Validate_WhenQueenIsPlayed(CardId cardId, bool applies, CardColor colorOverride, bool shouldPass)
    {
        var turn = GetTurn();
        var aceCard = QueenCard.Create(cardId).Value;
        var validationResult = turn.Validate(aceCard, colorOverride, applies);

        validationResult.IsSuccess.Should().Be(shouldPass);
    }
}
