using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class AceTurnTypeTests
{
    [Theory]
    [InlineData(CardId.AceOfHearts)]
    [InlineData(CardId.AceOfDiamonds)]
    public void Validate_WhenAceWasPlayed_Succeeds(CardId cardId)
    {
        var aceCard = AceCard.Create(cardId).Value;
        var turn = AceTurn.Create(new Player(CardsOnHand: [aceCard.Id]), aceCard);
        var validationResult = turn.Validate(AceCard.Create(CardId.AceOfClubs).Value, colorOverride: null, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRegularCardWasPlayedAndDoesntApply_SucceedsIfColorMatches()
    {
        var aceCard = AceCard.Create(CardId.AceOfHearts).Value;
        var turn = AceTurn.Create(new Player(CardsOnHand: [aceCard.Id]), aceCard);
        var validationResult = turn.Validate(RegularCard.Create(CardId.EightOfHearts).Value, colorOverride: null, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenSevenCardWasPlayedAndApplies_Fails()
    {
        var aceCard = AceCard.Create(CardId.AceOfHearts).Value;
        var turn = AceTurn.Create(new Player(CardsOnHand: [aceCard.Id]), aceCard);
        var validationResult = turn.Validate(SevenCard.Create(CardId.SevenOfHearts).Value, colorOverride: null, specialCardApplies: true);

        validationResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenSevenCardWasPlayed_SucceedsIfTheCardDoesntApply()
    {
        var aceCard = AceCard.Create(CardId.AceOfHearts).Value;
        var turn = AceTurn.Create(new Player(CardsOnHand: [aceCard.Id]), aceCard);
        var validationResult = turn.Validate(SevenCard.Create(CardId.SevenOfHearts).Value, colorOverride: null, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenSevenCardWasPlayed_FailsIfTheCardApplies()
    {
        var aceCard = AceCard.Create(CardId.AceOfHearts).Value;
        var turn = AceTurn.Create(new Player(CardsOnHand: [aceCard.Id]), aceCard);
        var validationResult = turn.Validate(SevenCard.Create(CardId.SevenOfHearts).Value, colorOverride: null, specialCardApplies: true);

        validationResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenQueenWasPlayed_SucceedsIfColorMatches()
    {
        var aceCard = AceCard.Create(CardId.AceOfHearts).Value;
        var turn = AceTurn.Create(new Player(CardsOnHand: [aceCard.Id]), aceCard);
        var validationResult = turn.Validate(QueenCard.Create(CardId.QueenOfHearts).Value, colorOverride: CardColor.Hearts, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenQueenWasPlayed_FailsIfColorMismatchWithOverride()
    {
        var aceCard = AceCard.Create(CardId.AceOfHearts).Value;
        var turn = AceTurn.Create(new Player(CardsOnHand: [aceCard.Id]), aceCard);
        var validationResult = turn.Validate(QueenCard.Create(CardId.QueenOfDiamonds).Value, colorOverride: CardColor.Clubs, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeFalse();
    }
}
