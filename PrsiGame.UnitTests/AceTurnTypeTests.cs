using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class AceTurnTypeTests
{
    [Fact]
    public void Validate_WhenPreviousTurnIsAce_Succeeds()
    {
        var aceOfClubs = AceCard.Create(CardId.AceOfClubs).Value;
        var previousTurn = AceTurn.Create(new Player(1, PlayerState.Waiting, CardsOnHand: []), aceOfClubs);
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfDiamonds).Value);

        var validationResult = turn.Validate(previousTurn, aceOfClubs);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPreviousTurnIsSkipAndColorMatches_Succeeds()
    {
        var previousTurn = SkipTurn.Create(new Player(1, PlayerState.Waiting, CardsOnHand: []));
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfDiamonds).Value);

        var validationResult = turn.Validate(previousTurn, RegularCard.Create(CardId.EightOfDiamonds).Value);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPreviousTurnIsSkipAndColorMismatches_Failed()
    {
        var previousTurn = SkipTurn.Create(new Player(1, PlayerState.Waiting, CardsOnHand: []));
        var turn = AceTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []), AceCard.Create(CardId.AceOfDiamonds).Value);

        var validationResult = turn.Validate(previousTurn, RegularCard.Create(CardId.EightOfHearts).Value);

        validationResult.IsFailed.Should().BeTrue();
    }
}