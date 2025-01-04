using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record LickTurn : Turn
{
    private LickTurn(Player player, int lickCount) : base(player)
    {
        LickCount = lickCount;
    }

    public int LickCount { get; }

    public static LickTurn Create(Player player, int lickCount)
    {
        return new LickTurn(player, lickCount);
    }

    public Result Validate(TurnType lastTurn, int? requiredLickCount)
    {
        return lastTurn switch
        {
            TurnType.AceTurn => new InvalidTurnError("The player is currently stunned."),
            TurnType.LickTurn => ValidateSingleCardLick(),
            TurnType.QueenTurn => ValidateSingleCardLick(),
            TurnType.RegularTurn => ValidateSingleCardLick(),
            TurnType.SevenTurn => Result.FailIf(requiredLickCount!.Value != LickCount, () => new InvalidTurnError($"Player must lick {requiredLickCount.Value} cards.")),
            TurnType.SkipTurn => ValidateSingleCardLick(),
            _ => throw new ArgumentOutOfRangeException(nameof(lastTurn), lastTurn, null)
        };
    }

    private Result ValidateSingleCardLick()
    {
        return LickCount == 1 ? Result.Ok() : new InvalidTurnError("The player can lick only a single card.");
    }
}
