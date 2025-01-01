using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class SkipTurnTypeTests
{
    [Fact]
    public void Validate_WhenPreviousTurnIsAce_Passes()
    {
        var previousTurn = AceTurn.Create(new Player(1, PlayerState.Waiting, CardsOnHand: []), AceCard.Create(CardId.AceOfClubs).Value);
        var skipTurn = SkipTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []));

        var validationResult = skipTurn.Validate(previousTurn);

        validationResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPreviousTurnIsNotAce_Fails()
    {
        var previousSkipTurn = SkipTurn.Create(new Player(1, PlayerState.Waiting, CardsOnHand: []));
        var skipTurn = SkipTurn.Create(new Player(0, PlayerState.OnTurn, CardsOnHand: []));

        var validationResult = skipTurn.Validate(previousSkipTurn);

        validationResult.IsSuccess.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
    }
}