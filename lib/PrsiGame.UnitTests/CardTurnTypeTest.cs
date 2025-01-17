using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class CardTurnTypeTest
{
    [Fact]
    public void Validate_WhenCardIsNotOnPlayersHand_Fails()
    {
        var turn = RegularTurn.Create(new Player([CardId.EightOfClubs, CardId.AceOfClubs, CardId.AceOfHearts]), RegularCard.Create(CardId.EightOfSpades).Value);

        var result = turn.Validate(RegularCard.Create(CardId.JackOfDiamonds).Value, colorOverride: null, specialCardApplies: false);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Message.Should().Contain("Card is not in the player's hand.");
    }
}
