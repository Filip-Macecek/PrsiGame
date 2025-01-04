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
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(cardId).Value);
        var validationResult = turn.Validate(AceCard.Create(CardId.AceOfClubs).Value, currentColorOverride: null, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRegularCardWasPlayedAndDoesntApply_SucceedsIfColorMatches()
    {
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfHearts).Value);
        var validationResult = turn.Validate(RegularCard.Create(CardId.EightOfHearts).Value, currentColorOverride: null, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenSevenCardWasPlayedAndApplies_Fails()
    {
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfHearts).Value);
        var validationResult = turn.Validate(SevenCard.Create(CardId.SevenOfHearts).Value, currentColorOverride: null, specialCardApplies: true);

        validationResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenSevenCardWasPlayed_SucceedsIfTheCardDoesntApply()
    {
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfHearts).Value);
        var validationResult = turn.Validate(SevenCard.Create(CardId.SevenOfHearts).Value, currentColorOverride: null, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenSevenCardWasPlayed_FailsIfTheCardApplies()
    {
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfHearts).Value);
        var validationResult = turn.Validate(SevenCard.Create(CardId.SevenOfHearts).Value, currentColorOverride: null, specialCardApplies: true);

        validationResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenQueenWasPlayed_SucceedsIfColorMatches()
    {
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfHearts).Value);
        var validationResult = turn.Validate(QueenCard.Create(CardId.QueenOfHearts).Value, currentColorOverride: CardColor.Hearts, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenQueenWasPlayed_FailsIfColorMismatchWithOverride()
    {
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfHearts).Value);
        var validationResult = turn.Validate(QueenCard.Create(CardId.QueenOfDiamonds).Value, currentColorOverride: CardColor.Clubs, specialCardApplies: false);

        validationResult.IsSuccess.Should().BeFalse();
    }
}
