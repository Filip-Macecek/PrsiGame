using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class LickTurnTypeTests
{
    [Theory]
    [InlineData(TurnType.SkipTurn, null, true)]
    [InlineData(TurnType.AceTurn, null, false)]
    [InlineData(TurnType.LickTurn, null, true)]
    [InlineData(TurnType.QueenTurn, null, true)]
    [InlineData(TurnType.RegularTurn, null, true)]
    [InlineData(TurnType.SevenTurn, 2, false)]
    public void Validate_WhenSingleLick_ReturnsCorrectResult(TurnType turnType, int? requiredLicks, bool shouldPass)
    {
        var turn = LickTurn.Create(new Player([]), lickCount: 1);

        var result = turn.Validate(turnType, requiredLicks);

        result.IsSuccess.Should().Be(shouldPass);
    }

    [Fact]
    public void Validate_WhenLickCountIsCorrect_Passes()
    {
        var turn = LickTurn.Create(new Player([]), lickCount: 6);

        var result = turn.Validate(TurnType.SevenTurn, 6);

        result.IsSuccess.Should().BeTrue();
    }
}
